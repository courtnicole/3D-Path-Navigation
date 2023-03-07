namespace PathNav.PathPlanning
{
    using System;
    using Extensions;

    public interface IPathElement : IComparable<IPathElement>
    {
        UniqueId Id { get; }
        int Index { get; }

        IData Data { get; }

        bool Configure();
    }
}
