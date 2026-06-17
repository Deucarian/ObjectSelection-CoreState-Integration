using Deucarian.Logging;

namespace Deucarian.ObjectSelection.CoreStateIntegration
{
    /// <summary>
    /// Package-level log categories for the ObjectSelection CoreState integration.
    /// </summary>
    public static class ObjectSelectionCoreStateLog
    {
        public static readonly DLog General = DLog.For("Selection.CoreStateIntegration");
        public static readonly DLog Samples = DLog.For("Selection.CoreStateIntegration.Samples");
    }
}
