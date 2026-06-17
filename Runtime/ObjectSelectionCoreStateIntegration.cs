using System;
using System.Collections.Generic;
using Deucarian.CoreState;
using Deucarian.ObjectSelection;

namespace Deucarian.ObjectSelection.CoreStateIntegration
{
    /// <summary>
    /// Synchronizes Object Selection and Core State selection services by shared key.
    /// </summary>
    /// <typeparam name="TKey">The stable selection key type shared by both services.</typeparam>
    /// <typeparam name="T">The Core State item type.</typeparam>
    public sealed class ObjectSelectionCoreStateIntegration<TKey, T> : IDisposable
    {
        private readonly ObjectSelectionService<TKey> _objectSelection;
        private readonly ISelectionService<TKey, T> _coreSelection;
        private bool _isBound;
        private bool _isSyncing;

        /// <summary>
        /// Creates and immediately binds a integration between Object Selection and Core State.
        /// </summary>
        /// <param name="objectSelection">The world/object selection service.</param>
        /// <param name="coreSelection">The Core State data selection service.</param>
        public ObjectSelectionCoreStateIntegration(
            ObjectSelectionService<TKey> objectSelection,
            ISelectionService<TKey, T> coreSelection)
            : this(objectSelection, coreSelection, true)
        {
        }

        /// <summary>
        /// Creates a integration between Object Selection and Core State.
        /// </summary>
        /// <param name="objectSelection">The world/object selection service.</param>
        /// <param name="coreSelection">The Core State data selection service.</param>
        /// <param name="bindImmediately">When true, subscribes to both selection services immediately.</param>
        public ObjectSelectionCoreStateIntegration(
            ObjectSelectionService<TKey> objectSelection,
            ISelectionService<TKey, T> coreSelection,
            bool bindImmediately)
        {
            if (objectSelection == null)
            {
                throw new ArgumentNullException(nameof(objectSelection));
            }

            if (coreSelection == null)
            {
                throw new ArgumentNullException(nameof(coreSelection));
            }

            _objectSelection = objectSelection;
            _coreSelection = coreSelection;

            if (bindImmediately)
            {
                Bind();
            }
        }

        /// <summary>
        /// Gets the Object Selection service synchronized by this integration.
        /// </summary>
        public ObjectSelectionService<TKey> ObjectSelection
        {
            get { return _objectSelection; }
        }

        /// <summary>
        /// Gets the Core State selection service synchronized by this integration.
        /// </summary>
        public ISelectionService<TKey, T> CoreSelection
        {
            get { return _coreSelection; }
        }

        /// <summary>
        /// Gets whether the integration is currently subscribed to both services.
        /// </summary>
        public bool IsBound
        {
            get { return _isBound; }
        }

        /// <summary>
        /// Subscribes to both selection services.
        /// </summary>
        public void Bind()
        {
            if (_isBound)
            {
                return;
            }

            _objectSelection.SelectionChanged += OnObjectSelectionChanged;
            _coreSelection.SelectionChanged += OnCoreSelectionChanged;
            _isBound = true;
        }

        /// <summary>
        /// Unsubscribes from both selection services.
        /// </summary>
        public void Unbind()
        {
            if (!_isBound)
            {
                return;
            }

            _objectSelection.SelectionChanged -= OnObjectSelectionChanged;
            _coreSelection.SelectionChanged -= OnCoreSelectionChanged;
            _isBound = false;
        }

        /// <summary>
        /// Unsubscribes from both selection services.
        /// </summary>
        public void Dispose()
        {
            Unbind();
        }

        private void OnObjectSelectionChanged(
            object sender,
            Deucarian.ObjectSelection.SelectionChangedEventArgs<TKey> args)
        {
            if (_isSyncing)
            {
                return;
            }

            RunGuarded(
                delegate
                {
                    if (args.HasSelection)
                    {
                        SelectCoreState(args.CurrentKey);
                    }
                    else
                    {
                        ClearCoreState();
                    }
                });
        }

        private void OnCoreSelectionChanged(
            object sender,
            Deucarian.CoreState.SelectionChangedEventArgs<TKey, T> args)
        {
            if (_isSyncing)
            {
                return;
            }

            RunGuarded(
                delegate
                {
                    if (args.HasSelection)
                    {
                        SelectObjectSelection(args.SelectedKey);
                    }
                    else
                    {
                        ClearObjectSelection();
                    }
                });
        }

        private void SelectCoreState(TKey key)
        {
            if (_coreSelection.HasSelection &&
                EqualityComparer<TKey>.Default.Equals(_coreSelection.SelectedKey, key))
            {
                return;
            }

            _coreSelection.TrySelect(key, SelectionChangeMode.Programmatic);
        }

        private void SelectObjectSelection(TKey key)
        {
            if (_objectSelection.HasSelection &&
                EqualityComparer<TKey>.Default.Equals(_objectSelection.CurrentKey, key))
            {
                return;
            }

            _objectSelection.TrySelect(key, SelectionChangeReason.Programmatic);
        }

        private void ClearCoreState()
        {
            if (_coreSelection.HasSelection)
            {
                _coreSelection.Clear(SelectionChangeMode.Programmatic);
            }
        }

        private void ClearObjectSelection()
        {
            if (_objectSelection.HasSelection)
            {
                _objectSelection.ClearSelection(SelectionChangeReason.Cleared);
            }
        }

        private void RunGuarded(Action action)
        {
            _isSyncing = true;

            try
            {
                action();
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
