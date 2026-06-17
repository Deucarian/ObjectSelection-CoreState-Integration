# Core State Integration Sample

Open `CoreStateIntegrationSample.unity` and enter Play Mode.

The sample creates four primitives and registers matching ObjectSelection and CoreState keys:

- `cube`
- `sphere`
- `capsule`
- `cylinder`

Click a primitive to select it through ObjectSelection. The integration updates CoreState selection by the same key.

Use the on-screen CoreState buttons or number keys `1` through `4` to select CoreState data. The integration updates ObjectSelection, and the sample highlighter reacts to the world selection change.

Press Backspace or the Clear CoreState button to clear selection on both sides.
