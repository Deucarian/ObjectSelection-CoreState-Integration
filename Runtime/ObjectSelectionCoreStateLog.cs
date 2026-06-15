using Deucarian.Logging;

namespace Deucarian.ObjectSelection.CoreStateBridge
{
    /// <summary>
    /// Package-level log categories for the ObjectSelection CoreState bridge.
    /// </summary>
    public static class ObjectSelectionCoreStateLog
    {
        public static readonly DLog General = DLog.For("Selection.CoreStateBridge");
        public static readonly DLog Samples = DLog.For("Selection.CoreStateBridge.Samples");
    }
}
