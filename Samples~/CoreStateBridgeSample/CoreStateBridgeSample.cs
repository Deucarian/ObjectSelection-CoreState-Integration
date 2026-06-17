using System.Collections.Generic;
using Deucarian.CoreState;
using Deucarian.ObjectSelection;
using UnityEngine;

namespace Deucarian.ObjectSelection.CoreStateBridge.Samples
{
    public sealed class CoreStateBridgeSample : MonoBehaviour
    {
        private readonly string[] _keys = { "cube", "sphere", "capsule", "cylinder" };
        private readonly Dictionary<string, GameObject> _objects = new Dictionary<string, GameObject>();

        private ObjectSelectionRegistry<string> _objectRegistry;
        private ObjectSelectionService<string> _objectSelection;
        private Repository<string, CoreStateBridgeSampleData> _repository;
        private SelectionService<string, CoreStateBridgeSampleData> _coreSelection;
        private ObjectSelectionCoreStateBridge<string, CoreStateBridgeSampleData> _bridge;
        private CoreStateBridgeSampleHighlighter _highlighter;
        private CoreStateBridgeSampleRaycastController _raycastController;
        private string _lastObjectEvent = "ObjectSelection: none";
        private string _lastCoreEvent = "CoreState: none";

        private void Awake()
        {
            _objectRegistry = new ObjectSelectionRegistry<string>();
            _objectSelection = new ObjectSelectionService<string>(_objectRegistry);
            _repository = new Repository<string, CoreStateBridgeSampleData>();
            _coreSelection = new SelectionService<string, CoreStateBridgeSampleData>(_repository);

            EnsureCamera();
            EnsureLight();
            EnsurePrimitives();
            RegisterSharedKeys();
            EnsureHighlighter();

            _objectSelection.SelectionChanged += OnObjectSelectionChanged;
            _coreSelection.SelectionChanged += OnCoreSelectionChanged;
            _bridge = new ObjectSelectionCoreStateBridge<string, CoreStateBridgeSampleData>(
                _objectSelection,
                _coreSelection);

            EnsureRaycastController();
        }

        private void OnDestroy()
        {
            if (_objectSelection != null)
            {
                _objectSelection.SelectionChanged -= OnObjectSelectionChanged;
            }

            if (_coreSelection != null)
            {
                _coreSelection.SelectionChanged -= OnCoreSelectionChanged;
            }

            if (_bridge != null)
            {
                _bridge.Dispose();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectCoreKey("cube");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectCoreKey("sphere");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectCoreKey("capsule");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SelectCoreKey("cylinder");
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                _coreSelection.Clear(SelectionChangeMode.Programmatic);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16f, 16f, 390f, 280f), GUI.skin.box);
            GUILayout.Label("Deucarian ObjectSelection CoreState Bridge");
            GUILayout.Label("World: " + (_objectSelection.HasSelection ? _objectSelection.CurrentKey : "(none)"));
            GUILayout.Label("CoreState: " + (_coreSelection.HasSelection ? _coreSelection.SelectedKey : "(none)"));
            GUILayout.Label(_lastObjectEvent);
            GUILayout.Label(_lastCoreEvent);

            GUILayout.Space(8f);
            GUILayout.Label("Select through CoreState");
            GUILayout.BeginHorizontal();
            DrawCoreButton("1 Cube", "cube");
            DrawCoreButton("2 Sphere", "sphere");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawCoreButton("3 Capsule", "capsule");
            DrawCoreButton("4 Cylinder", "cylinder");
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);
            if (GUILayout.Button("Clear CoreState"))
            {
                _coreSelection.Clear(SelectionChangeMode.Programmatic);
            }

            GUILayout.EndArea();
        }

        private void DrawCoreButton(string label, string key)
        {
            if (GUILayout.Button(label))
            {
                SelectCoreKey(key);
            }
        }

        private void SelectCoreKey(string key)
        {
            _coreSelection.Select(key, SelectionChangeMode.Programmatic);
        }

        private void OnObjectSelectionChanged(object sender, SelectionChangedEventArgs<string> args)
        {
            if (_highlighter != null)
            {
                _highlighter.OnSelectionChanged(args);
            }

            string previous = args.HadPreviousSelection ? args.PreviousKey : "(none)";
            string current = args.HasSelection ? args.CurrentKey : "(none)";
            _lastObjectEvent = "ObjectSelection: " + previous + " -> " + current + " (" + args.Reason + ")";
            ObjectSelectionCoreStateLog.Samples.Info(_lastObjectEvent);
        }

        private void OnCoreSelectionChanged(
            object sender,
            Deucarian.CoreState.SelectionChangedEventArgs<string, CoreStateBridgeSampleData> args)
        {
            string previous = args.HadPreviousSelection ? args.PreviousKey : "(none)";
            string current = args.HasSelection ? args.SelectedKey : "(none)";
            _lastCoreEvent = "CoreState: " + previous + " -> " + current + " (" + args.Mode + ")";
            ObjectSelectionCoreStateLog.Samples.Info(_lastCoreEvent);
        }

        private void EnsureCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 4f, -8f);
            camera.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
        }

        private void EnsureLight()
        {
            if (FindObjectOfType<Light>() != null)
            {
                return;
            }

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void EnsurePrimitives()
        {
            CreatePrimitive("cube", PrimitiveType.Cube, new Vector3(-3f, 0f, 0f));
            CreatePrimitive("sphere", PrimitiveType.Sphere, new Vector3(-1f, 0f, 0f));
            CreatePrimitive("capsule", PrimitiveType.Capsule, new Vector3(1f, 0f, 0f));
            CreatePrimitive("cylinder", PrimitiveType.Cylinder, new Vector3(3f, 0f, 0f));
        }

        private void CreatePrimitive(string key, PrimitiveType type, Vector3 position)
        {
            if (_objects.ContainsKey(key))
            {
                return;
            }

            GameObject existing = GameObject.Find(key);
            GameObject primitive = existing != null ? existing : GameObject.CreatePrimitive(type);
            primitive.name = key;
            primitive.transform.position = position;
            _objects.Add(key, primitive);
        }

        private void RegisterSharedKeys()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                string key = _keys[i];
                GameObject target;
                if (!_objects.TryGetValue(key, out target))
                {
                    continue;
                }

                _objectRegistry.Register(new SelectableObject<string>(key, target));
                _repository.AddOrUpdate(new CoreStateBridgeSampleData(key, ToLabel(key)));
            }
        }

        private static string ToLabel(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return char.ToUpperInvariant(key[0]) + key.Substring(1);
        }

        private void EnsureHighlighter()
        {
            _highlighter = GetComponent<CoreStateBridgeSampleHighlighter>();
            if (_highlighter == null)
            {
                _highlighter = gameObject.AddComponent<CoreStateBridgeSampleHighlighter>();
            }
        }

        private void EnsureRaycastController()
        {
            _raycastController = GetComponent<CoreStateBridgeSampleRaycastController>();
            if (_raycastController == null)
            {
                _raycastController = gameObject.AddComponent<CoreStateBridgeSampleRaycastController>();
            }

            _raycastController.SelectionCamera = Camera.main;
            _raycastController.Initialize(_objectSelection);
        }
    }
}
