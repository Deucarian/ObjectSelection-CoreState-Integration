using Deucarian.CoreState;

namespace Deucarian.ObjectSelection.CoreStateIntegration.Samples
{
    public sealed class CoreStateIntegrationSampleData : IIdentifiable<string>
    {
        public CoreStateIntegrationSampleData(string id, string label)
        {
            Id = id;
            Label = label;
        }

        public string Id { get; }
        public string Label { get; }
    }
}
