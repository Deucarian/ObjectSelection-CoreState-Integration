# Contributing

## Scope

ObjectSelection CoreState Bridge is a standalone bridge package. It may depend on `com.deucarian.object-selection` and `com.deucarian.core-state`.

Keep this package independent from UIBinding, API, Session, UI Toolkit, UGUI, ServiceLocator, backend services, and application-specific architecture.

The bridge synchronizes shared keys only. ObjectSelection owns world-object selection. CoreState owns data/application selection.

## Local Validation

Run structural validation from the package root:

```powershell
powershell -ExecutionPolicy Bypass -File ./Tools/Validate-Package.ps1
```

For Unity validation, use a separate test project that references this package and its dependencies by file path:

```json
{
  "dependencies": {
    "com.deucarian.object-selection": "file:C:/Repositories/ObjectSelection",
    "com.deucarian.core-state": "file:C:/Repositories/Core-State",
    "com.deucarian.object-selection.core-state-bridge": "file:C:/Repositories/Deucarian.ObjectSelection-CoreState-Bridge"
  }
}
```

Package source should stay in this repository. Do not copy package runtime code into the test project.

## Pull Requests

- Keep bridge behavior focused on selection synchronization.
- Add or update EditMode tests for behavior changes.
- Keep runtime asmdef free of editor-only references.
- Do not add UI, backend, service-location, or application dependencies.
