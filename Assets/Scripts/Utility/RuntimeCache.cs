﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum DockType
{
    Import,
    Export
}

namespace HoloIslandVis.Utility
{
    public class RuntimeCache : Singleton<RuntimeCache>
    {
        private const int _numBuildingLevels = 8;

        private GameObject _visualizationContainer;
        private GameObject _contentSurface;
        private GameObject _surfaceGlow;
        private GameObject _canvas;


        private Text _progressInfo;
        private Material _combinedHoloMaterial;
        private Material _wireFrame;
        private Material _highlightMaterial;

        private List<GameObject> _cuPrefabs;
        private List<GameObject> _siPrefabs;
        private List<GameObject> _sdPrefabs;
        Dictionary<DockType, GameObject> _dockPrefabs;

        private GameObject _importDockPrefab;
        private GameObject _exportDockPrefab;

        public int NumBuildingLevels {
            get { return _numBuildingLevels; }
            private set { }
        }

        public List<GameObject> IslandGameObjects { get; set; }
        public GameObject CurrentFocus { get; set; }

        public GameObject VisualizationContainer {
            get { return _visualizationContainer; }
            private set { }
        }

        public GameObject ContentSurface {
            get { return _contentSurface; }
            private set { }
        }

        public GameObject Canvas {
            get { return _canvas; }
            private set { }
        }

        public Text ProgressInfo {
            get { return _progressInfo; }
            private set { }
        }

        public Material WireFrame {
            get { return _wireFrame; }
            private set { }
        }

        public Material CombinedHoloMaterial {
            get { return _combinedHoloMaterial; }
            private set { }
        }

        public Material HighlightMaterial {
            get { return _highlightMaterial; }
            private set { }
        }

        public List<GameObject> CUPrefabs {
            get { return _cuPrefabs; }
            private set { }
        }

        public List<GameObject> SIPrefabs {
            get { return _siPrefabs; }
            private set { }
        }

        public List<GameObject> SDPrefabs {
            get { return _sdPrefabs; }
            private set { }
        }

        public Dictionary<DockType, GameObject> DockPrefabs {
            get { return _dockPrefabs; }
            private set { }
        }

        private RuntimeCache()
        {
            // Init
            _dockPrefabs = new Dictionary<DockType, GameObject>();

            // Object references
            _visualizationContainer = GameObject.Find("VisualizationContainer");
            _contentSurface = GameObject.Find("ContentSurface");
            _surfaceGlow = GameObject.Find("Glow");

            _contentSurface.SetActive(false);

            // Stuff
            //_canvas = GameObject.Find("Canvas");
            //_canvas.transform.localScale = Vector3.one * _canvas.transform.position.z * 0.00415f;
            //_progressInfo = _canvas.GetComponentInChildren<Text>();

            _combinedHoloMaterial = (Material) Resources.Load("Materials/CombinedHoloMaterial");
            _wireFrame = (Material) Resources.Load("Materials/WireFrame");
            _highlightMaterial = (Material) Resources.Load("Materials/Glow");

            // Prefabs
            _cuPrefabs = Resources.LoadAll<GameObject>("Prefabs/CompilationUnit/LOD0").ToList();
            _siPrefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceImpl/LOD0").ToList();
            _sdPrefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceDecl/LOD0").ToList();

            _dockPrefabs.Add(DockType.Import, (GameObject) Resources.Load("Prefabs/Docks/iDock_1"));
            _dockPrefabs.Add(DockType.Export, (GameObject) Resources.Load("Prefabs/Docks/eDock_1"));
        }
    }

}