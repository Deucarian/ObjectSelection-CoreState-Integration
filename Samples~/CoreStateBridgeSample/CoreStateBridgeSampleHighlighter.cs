using Deucarian.ObjectSelection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.ObjectSelection.CoreStateBridge.Samples
{
    public sealed class CoreStateBridgeSampleHighlighter : MonoBehaviour, IObjectSelectionHighlighter<string>
    {
        [SerializeField] private Color selectedColor = new Color(1f, 0.72f, 0.18f, 1f);

        private MaterialPropertyBlock _propertyBlock;
        private Renderer _currentRenderer;

        public void OnSelectionChanged(SelectionChangedEventArgs<string> args)
        {
            RestoreCurrent();

            GameObject currentGameObject = ResolveGameObject(args.CurrentObject);
            if (currentGameObject == null)
            {
                return;
            }

            Renderer renderer = currentGameObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            MaterialPropertyBlock propertyBlock = GetPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", selectedColor);
            renderer.SetPropertyBlock(propertyBlock);
            _currentRenderer = renderer;
        }

        private void OnDestroy()
        {
            RestoreCurrent();
        }

        private void RestoreCurrent()
        {
            if (_currentRenderer == null)
            {
                return;
            }

            _currentRenderer.SetPropertyBlock(null);
            _currentRenderer = null;
        }

        private MaterialPropertyBlock GetPropertyBlock()
        {
            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            return _propertyBlock;
        }

        private static GameObject ResolveGameObject(Object unityObject)
        {
            if (unityObject == null)
            {
                return null;
            }

            GameObject gameObject = unityObject as GameObject;
            if (gameObject != null)
            {
                return gameObject;
            }

            Component component = unityObject as Component;
            return component != null ? component.gameObject : null;
        }
    }
}
