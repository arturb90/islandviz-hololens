﻿using HoloIslandVis;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Utility;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloIslandVis.Interaction.Input
{
    public class GestureInputListener : SingletonComponent<GestureInputListener>, 
        IInputHandler, ISourceStateHandler
    {
        public delegate void GestureInputHandler(GestureInputEventArgs eventData);

        public event GestureInputHandler OneHandTap             = delegate { };
        public event GestureInputHandler TwoHandTap             = delegate { };
        public event GestureInputHandler OneHandDoubleTap       = delegate { };
        public event GestureInputHandler TwoHandDoubleTap       = delegate { };

        public event GestureInputHandler OneHandManipStart      = delegate { };
        public event GestureInputHandler TwoHandManipStart      = delegate { };
        public event GestureInputHandler OneHandManipUpdate     = delegate { };
        public event GestureInputHandler TwoHandManipUpdate     = delegate { };
        public event GestureInputHandler OneHandManipEnd        = delegate { };
        public event GestureInputHandler TwoHandManipEnd        = delegate { };

        private Dictionary<uint, GestureSource> _gestureSources;
        private Dictionary<short, Action<GestureInputEventArgs>> _gestureEventTable;
        private int _inputTimeout;
        private bool _timerSet;

        protected override void Awake()
        {
            base.Awake();
            _gestureEventTable = new Dictionary<short, Action<GestureInputEventArgs>>()
            {
                { Convert.ToInt16("0000000010001001", 2), eventArgs => OneHandTap(eventArgs) },
                { Convert.ToInt16("1000100110001001", 2), eventArgs => TwoHandTap(eventArgs) },
                { Convert.ToInt16("0000000010010010", 2), eventArgs => OneHandDoubleTap(eventArgs) },
                { Convert.ToInt16("1001001010010010", 2), eventArgs => TwoHandDoubleTap(eventArgs) },
                { Convert.ToInt16("0000000011000001", 2), eventArgs => OneHandManipStart(eventArgs) },
                { Convert.ToInt16("1100000111000001", 2), eventArgs => TwoHandManipStart(eventArgs) },
                { Convert.ToInt16("0000000010001000", 2), eventArgs => OneHandManipEnd(eventArgs) },
                { Convert.ToInt16("1000100010001000", 2), eventArgs => TwoHandManipEnd(eventArgs) },
                { Convert.ToInt16("1000000010001000", 2), eventArgs => TwoHandManipEnd(eventArgs) },
                { Convert.ToInt16("1000100010000000", 2), eventArgs => TwoHandManipEnd(eventArgs) }
            };

            _gestureSources = new Dictionary<uint, GestureSource>(2);
            InputManager.Instance.AddGlobalListener(gameObject);
            _inputTimeout = 250;
            _timerSet = false;
        }

        private void Update()
        {
            int sourcesManipulating = 0;
            foreach(GestureSource source in _gestureSources.Values)
            {
                if (source.IsManipulating && !source.IsEvaluating)
                    sourcesManipulating++;
            }

            if (sourcesManipulating == 1)
            {
                GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
                short gestureUpdate = Convert.ToInt16("1111111111111111", 2);
                _gestureSources.Values.CopyTo(gestureSources, 0);

                OneHandManipUpdate(new GestureInputEventArgs(gestureUpdate, gestureSources));
            }

            if (sourcesManipulating == 2)
            {
                GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
                short gestureUpdate = Convert.ToInt16("1111111111111111", 2);
                _gestureSources.Values.CopyTo(gestureSources, 0);

                TwoHandManipUpdate(new GestureInputEventArgs(gestureUpdate, gestureSources));
            }
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            IInputSource inputSource = eventData.InputSource;
            if (_gestureSources.ContainsKey(eventData.SourceId))
                return;

            _gestureSources.Add(eventData.SourceId, new GestureSource(inputSource, eventData.SourceId));
            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "InputSource: " + _gestureSources.Count + "   SourceId: " + eventData.SourceId + "  detected";
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            IInputSource inputSource = eventData.InputSource;
            if (!_gestureSources.ContainsKey(eventData.SourceId))
                return;

            if (_gestureSources[eventData.SourceId].IsManipulating)
            {
                GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
                _gestureSources.Values.CopyTo(gestureSources, 0);

                OneHandManipEnd(new GestureInputEventArgs(Convert.ToInt16("0000000010001000", 2), gestureSources));
                TwoHandManipEnd(new GestureInputEventArgs(Convert.ToInt16("1000100010001000", 2), gestureSources));
            }

            _gestureSources.Remove(eventData.SourceId);
            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "InputSources: " + _gestureSources.Count + "   SourceId: " + eventData.SourceId + "  lost";
        }

        public void OnInputDown(InputEventData eventData)
        {
            _gestureSources[eventData.SourceId].InputDown++;
            if (!_timerSet)
            {
                _timerSet = true;
                new Task(() => processInput(eventData)).Start();
            }
        }

        public void OnInputUp(InputEventData eventData)
        {
            _gestureSources[eventData.SourceId].InputUp++;
            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "InputUp.";
            if (!_timerSet)
            {
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TryProcess.";
                _timerSet = true;
                new Task(() => processInput(eventData)).Start();
            }
        }

        private async void processInput(InputEventData eventData)
        {
            await Task.Delay(_gestureSources[eventData.SourceId].InputTimeout);

            GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
            _gestureSources.Values.CopyTo(gestureSources, 0);

            short inputData = 0;
            for (int i = 0; i < gestureSources.Length; i++)
                inputData += (short) (gestureSources[i].Evaluate() << i * 8);

            _timerSet = false;
            Action<GestureInputEventArgs> action;
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Taskytask";
            });
            if (_gestureEventTable.TryGetValue(inputData, out action))
            {
                GestureInputEventArgs eventArgs = new GestureInputEventArgs(inputData, gestureSources);
                UnityMainThreadDispatcher.Instance.Enqueue(action, eventArgs);
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ProcessingInput";
                });
            }
        }
    }
}