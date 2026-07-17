# Changelog

## 1.0.4 - 2026-07-17

- Aligned package metadata and exact Core State, Object Selection, and Logging dependencies with the coordinated portfolio release.

## 1.0.3 - 2026-06-22

- Updated exact Core State, Object Selection, and Logging dependencies for the accepted stable release line.

## 1.0.2 - 2026-06-17

- Renamed the package identity from `com.deucarian.object-selection.core-state-bridge` to `com.deucarian.object-selection.core-state-integration`.
- Renamed CoreStateBridge assemblies, namespaces, tests, tools, and samples to CoreStateIntegration.
- Migration: remove the old bridge package ID from Unity manifests and add `com.deucarian.object-selection.core-state-integration`.

## 1.0.1

- Standardized package logging on com.deucarian.logging.
- Added `ObjectSelectionCoreStateLog` package categories for integration and sample diagnostics.

## 1.0.0

- Created standalone UPM package `com.deucarian.object-selection.core-state-integration`.
- Added `ObjectSelectionCoreStateIntegration<TKey, T>` for two-way key synchronization between ObjectSelection and CoreState.
- Added guard-based feedback loop prevention, idempotent forwarding, missing-key safety, and disposal/unsubscription support.
- Added EditMode tests, sample scene/scripts, README, contributing guide, validation tooling, and CI workflows.
