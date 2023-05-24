namespace PathNav.ExperimentControl
{
    using Extensions;
    using PathPlanning;
    
    public sealed class ModelData : IData
    {
        #region Implementation of IData
        public UniqueId Id    { get; }
        public int      Index { get; private set; }

        public void UpdateIndex(int index)
        {
            Index    = index;
            Id.Index = index;
        }
        #endregion

        public ModelData(int index, UniqueId id)
        {
            Id       = id;
            Index    = index;
            Id.Index = index;
        }
    }
}