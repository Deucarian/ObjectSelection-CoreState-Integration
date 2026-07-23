using System;
using System.Collections.Generic;
using Deucarian.CoreState;
using Deucarian.ObjectSelection;
using Object = UnityEngine.Object;

namespace Deucarian.ObjectSelection.CoreStateIntegration
{
    public sealed class CoreStateObjectSelectionAdapter<TKey, T> :
        IReadOnlyObjectSelection<TKey>,
        IObjectSelectionCommands<TKey>,
        IDisposable
    {
        private readonly ISelectionService<TKey, T> _selection;
        private readonly IReadOnlyObjectSelectionRegistry<TKey> _registry;
        private readonly IObjectSelectionChangeModeStrategy _changeModeStrategy;
        private bool _hasPendingReason;
        private SelectionChangeReason _pendingReason;
        private bool _isDisposed;

        public CoreStateObjectSelectionAdapter(
            ISelectionService<TKey, T> selection,
            IReadOnlyObjectSelectionRegistry<TKey> registry,
            IObjectSelectionChangeModeStrategy changeModeStrategy)
        {
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _changeModeStrategy = changeModeStrategy ??
                                  throw new ArgumentNullException(nameof(changeModeStrategy));
            _selection.SelectionChanged += OnCoreSelectionChanged;
        }

        public event EventHandler<Deucarian.ObjectSelection.SelectionChangedEventArgs<TKey>>
            SelectionChanged;

        public IReadOnlyObjectSelectionRegistry<TKey> SelectionRegistry => _registry;
        public bool HasSelection => _selection.HasSelection;
        public TKey CurrentKey => _selection.SelectedKey;

        public Object CurrentObject
        {
            get
            {
                return _selection.HasSelection &&
                       _registry.TryGetObject(_selection.SelectedKey, out Object target)
                    ? target
                    : null;
            }
        }

        public void Select(
            TKey key,
            SelectionChangeReason reason = SelectionChangeReason.Programmatic,
            bool forceEvent = false)
        {
            ThrowIfDisposed();
            if (!TrySelect(key, reason, forceEvent))
            {
                throw new KeyNotFoundException(
                    "The selected key must exist in both Core State and the object registry.");
            }
        }

        public bool TrySelect(
            TKey key,
            SelectionChangeReason reason = SelectionChangeReason.Programmatic,
            bool forceEvent = false)
        {
            ThrowIfDisposed();
            if (!_registry.ContainsKey(key))
            {
                return false;
            }

            if (_selection.HasSelection &&
                EqualityComparer<TKey>.Default.Equals(_selection.SelectedKey, key))
            {
                if (forceEvent)
                {
                    RaiseSelectionChanged(
                        true,
                        key,
                        CurrentObject,
                        true,
                        key,
                        CurrentObject,
                        reason);
                }

                return true;
            }

            return RunWithReason(
                reason,
                () => _selection.TrySelect(
                    key,
                    _changeModeStrategy.ToCoreStateMode(reason)));
        }

        public void ClearSelection(
            SelectionChangeReason reason = SelectionChangeReason.Cleared,
            bool forceEvent = false)
        {
            ThrowIfDisposed();
            if (!_selection.HasSelection)
            {
                if (forceEvent)
                {
                    RaiseSelectionChanged(
                        false,
                        default(TKey),
                        null,
                        false,
                        default(TKey),
                        null,
                        reason);
                }

                return;
            }

            RunWithReason(
                reason,
                () =>
                {
                    _selection.Clear(_changeModeStrategy.ToCoreStateMode(reason));
                    return true;
                });
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _selection.SelectionChanged -= OnCoreSelectionChanged;
            _isDisposed = true;
        }

        private void OnCoreSelectionChanged(
            object sender,
            Deucarian.CoreState.SelectionChangedEventArgs<TKey, T> args)
        {
            SelectionChangeReason reason = _hasPendingReason
                ? _pendingReason
                : _changeModeStrategy.ToObjectSelectionReason(args.Mode, args.HasSelection);

            RaiseSelectionChanged(
                args.HadPreviousSelection,
                args.PreviousKey,
                ResolveObject(args.PreviousKey, args.HadPreviousSelection),
                args.HasSelection,
                args.SelectedKey,
                ResolveObject(args.SelectedKey, args.HasSelection),
                reason);
        }

        private Object ResolveObject(TKey key, bool hasKey)
        {
            return hasKey && _registry.TryGetObject(key, out Object target)
                ? target
                : null;
        }

        private void RaiseSelectionChanged(
            bool hadPreviousSelection,
            TKey previousKey,
            Object previousObject,
            bool hasSelection,
            TKey currentKey,
            Object currentObject,
            SelectionChangeReason reason)
        {
            SelectionChanged?.Invoke(
                this,
                new Deucarian.ObjectSelection.SelectionChangedEventArgs<TKey>(
                    hadPreviousSelection,
                    previousKey,
                    previousObject,
                    hasSelection,
                    currentKey,
                    currentObject,
                    reason));
        }

        private TResult RunWithReason<TResult>(
            SelectionChangeReason reason,
            Func<TResult> action)
        {
            _pendingReason = reason;
            _hasPendingReason = true;
            try
            {
                return action();
            }
            finally
            {
                _hasPendingReason = false;
                _pendingReason = default(SelectionChangeReason);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
