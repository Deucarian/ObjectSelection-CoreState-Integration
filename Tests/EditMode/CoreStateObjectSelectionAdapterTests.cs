using Deucarian.CoreState;
using Deucarian.ObjectSelection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.ObjectSelection.CoreStateIntegration.Tests
{
    public sealed class CoreStateObjectSelectionAdapterTests
    {
        private Repository<int, TestItem> _repository;
        private SelectionService<int, TestItem> _coreSelection;
        private ObjectSelectionRegistry<int> _objectRegistry;
        private CoreStateObjectSelectionAdapter<int, TestItem> _adapter;
        private GameObject _target;

        [SetUp]
        public void SetUp()
        {
            _repository = new Repository<int, TestItem>();
            _repository.AddOrUpdate(new TestItem(7));
            _coreSelection = new SelectionService<int, TestItem>(_repository);
            _objectRegistry = new ObjectSelectionRegistry<int>();
            _target = new GameObject("Core State Object Selection Adapter Test Target");
            _objectRegistry.Register(new SelectableObject<int>(7, _target));
            _adapter = new CoreStateObjectSelectionAdapter<int, TestItem>(
                _coreSelection,
                _objectRegistry,
                new DefaultObjectSelectionChangeModeStrategy());
        }

        [TearDown]
        public void TearDown()
        {
            _adapter?.Dispose();
            _coreSelection?.Dispose();
            if (_target != null)
            {
                Object.DestroyImmediate(_target);
            }
        }

        [Test]
        public void ObjectCommandSelectsTheSingleCoreStateSource()
        {
            SelectionChangedEventArgs<int> observed = null;
            _adapter.SelectionChanged += (_, args) => observed = args;

            bool selected = _adapter.TrySelect(7, SelectionChangeReason.Raycast);

            Assert.IsTrue(selected);
            Assert.IsTrue(_coreSelection.HasSelection);
            Assert.AreEqual(7, _coreSelection.SelectedKey);
            Assert.IsTrue(_adapter.HasSelection);
            Assert.AreSame(_target, _adapter.CurrentObject);
            Assert.NotNull(observed);
            Assert.AreEqual(SelectionChangeReason.Raycast, observed.Reason);
        }

        [Test]
        public void CoreStateSelectionIsObservedWithoutDuplicateSelectionState()
        {
            SelectionChangedEventArgs<int> observed = null;
            _adapter.SelectionChanged += (_, args) => observed = args;

            _coreSelection.Select(7, SelectionChangeMode.Programmatic);

            Assert.IsTrue(_adapter.HasSelection);
            Assert.AreEqual(7, _adapter.CurrentKey);
            Assert.AreSame(_target, _adapter.CurrentObject);
            Assert.NotNull(observed);
            Assert.AreEqual(SelectionChangeReason.Programmatic, observed.Reason);
        }

        [Test]
        public void ObjectCommandRejectsItemsWithoutARegisteredWorldObject()
        {
            _repository.AddOrUpdate(new TestItem(9));

            Assert.IsFalse(_adapter.TrySelect(9));
            Assert.IsFalse(_coreSelection.HasSelection);
        }

        [Test]
        public void ExistingVisualObserverCanUseTheCoreStateAdapter()
        {
            var visual = new RecordingVisual();
            using (var observer = new ObjectSelectionVisualController<int>(_adapter, visual))
            {
                _adapter.Select(7, SelectionChangeReason.Raycast);
                _adapter.ClearSelection(SelectionChangeReason.Cleared);
            }

            Assert.AreEqual(1, visual.SelectedCount);
            Assert.AreEqual(1, visual.DeselectedCount);
        }

        [Test]
        public void DisposeStopsObjectSelectionEvents()
        {
            int eventCount = 0;
            _adapter.SelectionChanged += (_, __) => eventCount++;
            _adapter.Dispose();

            _coreSelection.Select(7);

            Assert.AreEqual(0, eventCount);
        }

        private sealed class TestItem : IIdentifiable<int>
        {
            public TestItem(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        private sealed class RecordingVisual : IObjectSelectionVisual<int>
        {
            public int SelectedCount { get; private set; }
            public int DeselectedCount { get; private set; }

            public void ApplySelected(int key, Object target)
            {
                SelectedCount++;
            }

            public void ApplyDeselected(int key, Object target)
            {
                DeselectedCount++;
            }
        }
    }
}
