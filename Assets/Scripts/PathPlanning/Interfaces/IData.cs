namespace PathNav.PathPlanning
{
    using Extensions;

    public interface IData 
    {
        public UniqueId Id { get; }
        public int Index { get; }

        void UpdateIndex(int index);
    }
}
