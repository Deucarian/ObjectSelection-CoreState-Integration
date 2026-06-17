# Deucarian ObjectSelection CoreState Integration

## Overview

Deucarian ObjectSelection CoreState Integration is a standalone Unity package that synchronizes keyed selection between:

- `Deucarian Object Selection`
- `Deucarian Core State`

ObjectSelection owns world-object selection. CoreState owns data/application selection. This integration only synchronizes shared keys.

Package ID: `com.deucarian.object-selection.core-state-integration`

Migration note: replace old manifest entries for `com.deucarian.object-selection.core-state-bridge` with `com.deucarian.object-selection.core-state-integration`. The source repository URL still uses `Object-Selection-Bridge.git` until the GitHub repository is renamed.

## Installation

Install the dependencies and this integration through Unity Package Manager:

```json
{
  "dependencies": {
    "com.deucarian.object-selection": "https://github.com/Deucarian/Object-Selection.git#main",
    "com.deucarian.core-state": "https://github.com/Deucarian/Core-State.git#main",
    "com.deucarian.object-selection.core-state-integration": "https://github.com/Deucarian/Object-Selection-Bridge.git#main"
  }
}
```

For local development:

```json
{
  "dependencies": {
    "com.deucarian.object-selection": "file:C:/Repositories/ObjectSelection",
    "com.deucarian.core-state": "file:C:/Repositories/Core-State",
    "com.deucarian.object-selection.core-state-integration": "file:C:/Repositories/Deucarian.ObjectSelection-CoreState-Integration"
  }
}
```

The integration requires Unity `2021.3` or newer and depends on `com.deucarian.logging`.

## Logging

This package uses `com.deucarian.logging`.

ObjectSelection CoreState integration diagnostics use stable package categories: `Selection.CoreStateIntegration` and `Selection.CoreStateIntegration.Samples`. Configure Deucarian Logging filters by category and level to isolate integration or sample output. Entries flow through the shared ring buffer for recent-diagnostic inspection and remain compatible with future telemetry sinks.

## Core Flow

World object clicked:

```text
ObjectSelectionService<TKey> selects key
-> ObjectSelectionCoreStateIntegration<TKey, T>
-> CoreState ISelectionService<TKey, T> selects key
```

Data/application selection changed:

```text
CoreState ISelectionService<TKey, T> selects key
-> ObjectSelectionCoreStateIntegration<TKey, T>
-> ObjectSelectionService<TKey> selects key
```

## Public API

`ObjectSelectionCoreStateIntegration<TKey, T>` subscribes to both selection services and forwards changes by key.

```csharp
using Deucarian.CoreState;
using Deucarian.ObjectSelection;
using Deucarian.ObjectSelection.CoreStateIntegration;

ObjectSelectionService<string> objectSelection = new ObjectSelectionService<string>(objectRegistry);
ISelectionService<string, ProjectData> coreSelection = new SelectionService<string, ProjectData>(repository);

using var integration = new ObjectSelectionCoreStateIntegration<string, ProjectData>(
    objectSelection,
    coreSelection);
```

The two-argument constructor binds immediately. You can also control lifecycle explicitly:

```csharp
var integration = new ObjectSelectionCoreStateIntegration<string, ProjectData>(
    objectSelection,
    coreSelection,
    bindImmediately: false);

integration.Bind();
integration.Unbind();
integration.Dispose();
```

## Behavior

- If ObjectSelection selects key `x`, CoreState tries to select key `x`.
- If CoreState selects key `x`, ObjectSelection tries to select key `x`.
- If either side clears selection, the other side clears selection.
- Same-key changes are idempotent.
- A guard prevents recursive feedback loops.
- Missing keys are handled with `TrySelect` and do not throw.
- The integration does not duplicate selection state.
- The integration does not use ServiceLocator, singletons, UI Toolkit, UGUI, UIBinding, API, Session, or backend/application code.

## Samples

The package contains one sample:

- `Core State Integration Sample`: `Samples~/CoreStateIntegrationSample/CoreStateIntegrationSample.unity`

Open the scene and enter Play Mode. The sample creates a cube, sphere, capsule, and cylinder with matching ObjectSelection keys and CoreState repository keys.

Click a primitive to select it through ObjectSelection and watch CoreState selection update. Use the sample's CoreState buttons to programmatically select data and watch ObjectSelection update the world highlight.

## Validation

Run structural validation from the package root:

```powershell
powershell -ExecutionPolicy Bypass -File ./Tools/Validate-Package.ps1
```

For Unity validation, use a separate test project that references this package and its dependencies by file path, then run EditMode tests for `Deucarian.ObjectSelection.CoreStateIntegration.Tests`.
