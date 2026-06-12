using Deucarian.CoreState;

namespace Deucarian.ObjectSelection.CoreStateBridge.Samples
{
    public sealed class CoreStateBridgeSampleData : IIdentifiable<string>
    {
        public CoreStateBridgeSampleData(string id, string label)
        {
            Id = id;
            Label = label;
        }

        public string Id { get; }
        public string Label { get; }
    }
}
