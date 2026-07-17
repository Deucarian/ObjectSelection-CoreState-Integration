# Deucarian Object Selection Core State Integration

## What this is

`com.deucarian.object-selection.core-state-integration` is a standalone Unity package that synchronizes keyed selection between:

- `Deucarian Object Selection`
- `Deucarian Core State`

ObjectSelection owns world-object selection. CoreState owns data/application selection. This integration only synchronizes shared keys.

Current package version: `1.0.4`.

## When to use it

- Your project already uses Object Selection and Core State.
- You want world-object selection and repository/data selection synchronized by shared key.
- You want bidirectional sync without making either target package depend on the other.

## When not to use it

- Do not use this package without both target packages installed.
- Do not put object selection ownership, repository ownership, UI binding, persistence, networking, API/session behavior, or service location here.
- Do not add app-specific selection visuals to this integration package.

Migration note: replace old manifest entries for `com.deucarian.object-selection.core-state-bridge` with `com.deucarian.object-selection.core-state-integration`. Current installs use the `ObjectSelection-CoreState-Integration.git` repository.

## Install

Stable:

```json
"com.deucarian.object-selection.core-state-integration": "https://github.com/Deucarian/ObjectSelection-CoreState-Integration.git#main"
```

Development:

```json
"com.deucarian.object-selection.core-state-integration": "https://github.com/Deucarian/ObjectSelection-CoreState-Integration.git#develop"
```

For local development:

```json
{
  "dependencies": {
    "com.deucarian.object-selection": "file:C:/Repositories/Object-Selection",
    "com.deucarian.core-state": "file:C:/Repositories/Core-State",
    "com.deucarian.object-selection.core-state-integration": "file:C:/Repositories/ObjectSelection-CoreState-Integration"
  }
}
```

## Dependencies

- `com.deucarian.object-selection` `1.0.4` supplies the world-object selection service.
- `com.deucarian.core-state` `1.0.2` supplies the repository and data-selection contracts.
- `com.deucarian.logging` `1.0.2` supplies runtime diagnostics for integration and sample categories.

Neither Object Selection nor Core State depends on this integration package.

## Unity compatibility

Requires Unity 2021.3 or newer.

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

Run the shared package validator from the repository root:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

For Unity validation, use a separate test project that references this package and its dependencies by file path, then run EditMode tests for `Deucarian.ObjectSelection.CoreStateIntegration.Tests`.

Documentation-only updates should still pass:

```powershell
git diff --check
```

## Architecture / Contributor Notes

- [AGENTS.md](AGENTS.md) contains repository-specific ownership and Codex guidance.
- Deucarian architecture rules live in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md).
- Capability ownership is tracked in [CAPABILITY_OWNERSHIP.md](https://github.com/Deucarian/Package-Registry/blob/develop/CAPABILITY_OWNERSHIP.md).

## License

See [LICENSE.md](LICENSE.md).

## Quick Start

1. Install the package through Deucarian Package Installer or Unity Package Manager using the URL above.
2. Let Unity finish resolving packages and compiling assemblies.
3. Import the `Core State Integration Sample` sample if you want a working reference scene or setup.
4. Start from the package README sections above and the public runtime/editor APIs in this repository.

## Troubleshooting

- Package does not resolve: confirm the stable or development Git URL matches the Package Registry entry and that required Deucarian dependencies are installed.
- Unity compile errors after install: let Package Manager finish resolving dependencies, then check asmdef references against `package.json` dependencies.
- Behavior appears to belong in another package: consult `AGENTS.md` and the Package Registry governance docs before moving or duplicating code.
