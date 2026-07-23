using Deucarian.CoreState;
using Deucarian.ObjectSelection;

namespace Deucarian.ObjectSelection.CoreStateIntegration
{
    public sealed class DefaultObjectSelectionChangeModeStrategy :
        IObjectSelectionChangeModeStrategy
    {
        public SelectionChangeMode ToCoreStateMode(SelectionChangeReason reason)
        {
            return reason == SelectionChangeReason.Programmatic
                ? SelectionChangeMode.Programmatic
                : SelectionChangeMode.Manual;
        }

        public SelectionChangeReason ToObjectSelectionReason(
            SelectionChangeMode mode,
            bool hasSelection)
        {
            if (!hasSelection)
            {
                return SelectionChangeReason.Cleared;
            }

            return mode == SelectionChangeMode.Manual
                ? SelectionChangeReason.Raycast
                : SelectionChangeReason.Programmatic;
        }
    }
}
