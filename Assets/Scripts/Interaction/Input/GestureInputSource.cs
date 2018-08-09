﻿using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloIslandVis.Interaction.Input
{
    enum SourceState
    {
        Pressed         = 1,
        Released        = 4
    }

    public class GestureSource
    {
        private Dictionary<SourceState, Func<GestureSource, byte>> _stateBytes;

        public IInputSource InputSource { get; private set; }
        public int InputBufferSize { get; private set; }
        public float InputTimeout { get; private set; }
        public byte InputData { get; private set; }
        public bool IsManipulating { get; private set; }
        public bool IsEvaluating { get; set; }
        public int InputDown { get; set; }
        public int InputUp { get; set; }

        public GestureSource(IInputSource inputSource)
            : this (inputSource, 2) { }

        public GestureSource(IInputSource inputSource, int inputBufferSize)
            : this(inputSource, inputBufferSize, 0.25f) { }

        public GestureSource(IInputSource inputSource, int inputBufferSize, float inputTimeout)
        {
            _stateBytes = new Dictionary<SourceState, Func<GestureSource, byte>>
            {
                { SourceState.Pressed, new Func<GestureSource, byte>((source) => source.getInputData(1, 0)) },
                { SourceState.Released, new Func<GestureSource, byte>((source) => source.getInputData(0, 1)) }
            };

            InputSource = inputSource;
            InputBufferSize = inputBufferSize;
            InputTimeout = inputTimeout;
            IsManipulating = false;
            IsEvaluating = false;
            InputDown = 0;
            InputUp = 0;
        }

        public IEnumerator Evaluate()
        {
            IsEvaluating = true;
            yield return new WaitForSeconds(InputTimeout);

            InputData = getInputData(InputDown, InputUp);
            if(InputData == _stateBytes[SourceState.Pressed].Invoke(this))
                IsManipulating = true;

            if(InputData == _stateBytes[SourceState.Released].Invoke(this))
                IsManipulating = false;

            InputData = getInputData(InputDown, InputUp);
            resetInputData(this);
            IsEvaluating = false;
        }

        private byte getInputData(int inputDown, int inputUp)
        {
            byte inputData = 0;
            inputData += (byte) (Math.Min(InputBufferSize, inputDown));
            inputData += (byte) (Math.Min(InputBufferSize, inputUp) << 3);
            inputData += (byte) ((IsManipulating ? 1 : 0) << 6);
            inputData += (byte) ((IsEvaluating   ? 1 : 0) << 7);
            return inputData;
        }

        private void resetInputData(GestureSource gestureSource)
        {
            gestureSource.InputDown = 0;
            gestureSource.InputUp = 0;
        }
    }
}