using System;
using System.Collections.Generic;
using Deucarian.CoreState;
using Deucarian.ObjectSelection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deucarian.ObjectSelection.CoreStateIntegration.Tests
{
    public sealed class ObjectSelectionCoreStateIntegrationTests
    {
        [Test]
        public void ObjectSelectionToCoreStateSync()
        {
            var fixture = new IntegrationFixture();

            try
            {
                using (new ObjectSelectionCoreStateIntegration<string, TestData>(
                    fixture.ObjectSelection,
                    fixture.CoreSelection))
                {
                    fixture.ObjectSelection.Select("cube", SelectionChangeReason.Raycast);

                    Assert.IsTrue(fixture.CoreSelection.HasSelection);
                    Assert.AreEqual("cube", fixture.CoreSelection.SelectedKey);
                    Assert.AreEqual("Cube", fixture.CoreSelection.SelectedItem.Label);
                }
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void CoreStateToObjectSelectionSync()
        {
            var fixture = new IntegrationFixture();

            try
            {
                using (new ObjectSelectionCoreStateIntegration<string, TestData>(
                    fixture.ObjectSelection,
                    fixture.CoreSelection))
                {
                    fixture.CoreSelection.Select("sphere", SelectionChangeMode.Programmatic);

                    Assert.IsTrue(fixture.ObjectSelection.HasSelection);
                    Assert.AreEqual("sphere", fixture.ObjectSelection.CurrentKey);
                    Assert.AreSame(fixture.Sphere, fixture.ObjectSelection.CurrentObject);
                }
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void ClearObjectSelectionClearsCoreState()
        {
            var fixture = new IntegrationFixture();

            try
            {
                using (new ObjectSelectionCoreStateIntegration<string, TestData>(
                    fixture.ObjectSelection,
                    fixture.CoreSelection))
                {
                    fixture.CoreSelection.Select("cube");
                    fixture.ObjectSelection.ClearSelection();

                    Assert.IsFalse(fixture.ObjectSelection.HasSelection);
                    Assert.IsFalse(fixture.CoreSelection.HasSelection);
                }
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void ClearCoreStateClearsObjectSelection()
        {
            var fixture = new IntegrationFixture();

            try
            {
                using (new ObjectSelectionCoreStateIntegration<string, TestData>(
                    fixture.ObjectSelection,
                    fixture.CoreSelection))
                {
                    fixture.ObjectSelection.Select("cube");
                    fixture.CoreSelection.Clear();

                    Assert.IsFalse(fixture.CoreSelection.HasSelection);
                    Assert.IsFalse(fixture.ObjectSelection.HasSelection);
                }
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void SameKeyChangesAreIdempotent()
        {
            var fixture = new IntegrationFixture();

            try
            {
                int coreEvents = 0;
                fixture.CoreSelection.SelectionChanged += (_, __) => coreEvents++;

                using (new ObjectSelectionCoreStateIntegration<string, TestData>(
                    fixture.ObjectSelection,
                    fixture.CoreSelection))
                {
                    fixture.ObjectSelection.Select("cube");
                    fixture.ObjectSelection.Select("cube");

                    Assert.AreEqual(1, coreEvents);
                    Assert.AreEqual("cube", fixture.CoreSelection.SelectedKey);
                }
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void GuardPreventsFeedbackLoops()
        {
            var registry = new ObjectSelectionRegistry<string>();
            var cube = new GameObject("Cube");
            var coreSelection = new RecordingCoreSelectionService();

            try
            {
                registry.Register(new SelectableObject<string>("cube", cube));
                var objectSelection = new ObjectSelectionService<string>(registry);

                using (new ObjectSelectionCoreStateIntegration<string, TestData>(
                    objectSelection,
                    coreSelection))
                {
                    coreSelection.RaiseSelected("cube", new TestData("cube", "Cube"));

                    Assert.IsTrue(objectSelection.HasSelection);
                    Assert.AreEqual("cube", objectSelection.CurrentKey);
                    Assert.AreEqual(0, coreSelection.TrySelectCallCount);
                }
            }
            finally
            {
                Object.DestroyImmediate(cube);
            }
        }

        [Test]
        public void DisposeUnsubscribesEvents()
        {
            var fixture = new IntegrationFixture();

            try
            {
                var integration = new ObjectSelectionCoreStateIntegration<string, TestData>(
                    fixture.ObjectSelection,
                    fixture.CoreSelection);

                integration.Dispose();

                fixture.ObjectSelection.Select("cube");
                Assert.IsFalse(fixture.CoreSelection.HasSelection);

                fixture.ObjectSelection.ClearSelection();
                fixture.CoreSelection.Select("sphere");
                Assert.IsFalse(fixture.ObjectSelection.HasSelection);
                Assert.IsFalse(integration.IsBound);
            }
            finally
            {
                fixture.Dispose();
            }
        }

        [Test]
        public void MissingKeyOrObjectHandlingIsSafe()
        {
            var fixture = new IntegrationFixture(includeMismatchedKeys: true);

            try
            {
                using (new ObjectSelectionCoreStateIntegration<string, TestData>(
                    fixture.ObjectSelection,
                    fixture.CoreSelection))
                {
                    Assert.DoesNotThrow(() => fixture.ObjectSelection.Select("world-only"));
                    Assert.AreEqual("world-only", fixture.ObjectSelection.CurrentKey);
                    Assert.IsFalse(fixture.CoreSelection.HasSelection);

                    fixture.ObjectSelection.ClearSelection();

                    Assert.DoesNotThrow(() => fixture.CoreSelection.Select("data-only"));
                    Assert.AreEqual("data-only", fixture.CoreSelection.SelectedKey);
                    Assert.IsFalse(fixture.ObjectSelection.HasSelection);
                }
            }
            finally
            {
                fixture.Dispose();
            }
        }

        private sealed class IntegrationFixture
        {
            public IntegrationFixture(bool includeMismatchedKeys = false)
            {
                ObjectRegistry = new ObjectSelectionRegistry<string>();
                Repository = new Repository<string, TestData>();

                Cube = new GameObject("Cube");
                Sphere = new GameObject("Sphere");

                ObjectRegistry.Register(new SelectableObject<string>("cube", Cube));
                ObjectRegistry.Register(new SelectableObject<string>("sphere", Sphere));

                Repository.AddOrUpdate(new TestData("cube", "Cube"));
                Repository.AddOrUpdate(new TestData("sphere", "Sphere"));

                if (includeMismatchedKeys)
                {
                    WorldOnly = new GameObject("World Only");
                    ObjectRegistry.Register(new SelectableObject<string>("world-only", WorldOnly));
                    Repository.AddOrUpdate(new TestData("data-only", "Data Only"));
                }

                ObjectSelection = new ObjectSelectionService<string>(ObjectRegistry);
                CoreSelection = new SelectionService<string, TestData>(Repository);
            }

            public ObjectSelectionRegistry<string> ObjectRegistry { get; }
            public Repository<string, TestData> Repository { get; }
            public ObjectSelectionService<string> ObjectSelection { get; }
            public SelectionService<string, TestData> CoreSelection { get; }
            public GameObject Cube { get; }
            public GameObject Sphere { get; }
            public GameObject WorldOnly { get; }

            public void Dispose()
            {
                Object.DestroyImmediate(Cube);
                Object.DestroyImmediate(Sphere);

                if (WorldOnly != null)
                {
                    Object.DestroyImmediate(WorldOnly);
                }
            }
        }

        private sealed class TestData : IIdentifiable<string>
        {
            public TestData(string id, string label)
            {
                Id = id;
                Label = label;
            }

            public string Id { get; }
            public string Label { get; }
        }

        private sealed class RecordingCoreSelectionService : ISelectionService<string, TestData>
        {
            public event EventHandler<SelectionChangedEventArgs<string, TestData>> SelectionChanged;

            public int TrySelectCallCount { get; private set; }
            public int ClearCallCount { get; private set; }
            public bool HasSelection { get; private set; }
            public string SelectedKey { get; private set; }
            public TestData SelectedItem { get; private set; }

            public void Select(string key, SelectionChangeMode mode = SelectionChangeMode.Manual)
            {
                if (!TrySelect(key, mode))
                {
                    throw new KeyNotFoundException();
                }
            }

            public bool TrySelect(string key, SelectionChangeMode mode = SelectionChangeMode.Manual)
            {
                TrySelectCallCount++;
                HasSelection = true;
                SelectedKey = key;
                SelectedItem = new TestData(key, key);
                return true;
            }

            public void Clear(SelectionChangeMode mode = SelectionChangeMode.Manual)
            {
                ClearCallCount++;
                HasSelection = false;
                SelectedKey = null;
                SelectedItem = null;
            }

            public void RaiseSelected(string key, TestData item)
            {
                bool hadPreviousSelection = HasSelection;
                string previousKey = SelectedKey;
                TestData previousItem = SelectedItem;

                HasSelection = true;
                SelectedKey = key;
                SelectedItem = item;

                SelectionChanged?.Invoke(
                    this,
                    new SelectionChangedEventArgs<string, TestData>(
                        hadPreviousSelection,
                        previousKey,
                        previousItem,
                        true,
                        key,
                        item,
                        SelectionChangeMode.Programmatic));
            }
        }
    }
}
