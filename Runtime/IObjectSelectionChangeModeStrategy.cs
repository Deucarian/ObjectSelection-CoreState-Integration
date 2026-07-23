using Deucarian.CoreState;
using Deucarian.ObjectSelection;

namespace Deucarian.ObjectSelection.CoreStateIntegration
{
    public interface IObjectSelectionChangeModeStrategy
    {
        SelectionChangeMode ToCoreStateMode(SelectionChangeReason reason);

        SelectionChangeReason ToObjectSelectionReason(
            SelectionChangeMode mode,
            bool hasSelection);
    }
}
