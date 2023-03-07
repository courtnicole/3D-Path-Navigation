namespace PathNav.PathPlanning
{
    using Extensions;

    public sealed class PointVisualData : IData
    {
        public UniqueId Id { get; }
        public int Index { get; private set; }

        public PointVisualData(int index, UniqueId id)
        {
            Index    = index;
            Id       = id;
            Id.Index = index;
        }

        public void UpdateIndex(int index)
        {
            Index    = index;
            Id.Index = index;
        }
    }
}
