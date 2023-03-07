namespace PathNav.PathPlanning
{
    using Extensions;

    public sealed class SegmentData : IData
    {
        public UniqueId Id { get; }
        public int Index { get; private set; }
        public bool IsFirst => Index == 0;

        public SegmentData(int index, UniqueId id)
        {
            Id    = id;
            Index = index;
            Id.Index = index;
        }

        public void UpdateIndex(int index)
        {
            Index    = index;
            Id.Index = index;
        }
    }
}
