# Changelog

## 1.0.1

- Standardized package logging on com.deucarian.logging.
- Added `ObjectSelectionCoreStateLog` package categories for bridge and sample diagnostics.

## 1.0.0

- Created standalone UPM package `com.deucarian.object-selection.core-state-bridge`.
- Added `ObjectSelectionCoreStateBridge<TKey, T>` for two-way key synchronization between ObjectSelection and CoreState.
- Added guard-based feedback loop prevention, idempotent forwarding, missing-key safety, and disposal/unsubscription support.
- Added EditMode tests, sample scene/scripts, README, contributing guide, validation tooling, and CI workflows.
