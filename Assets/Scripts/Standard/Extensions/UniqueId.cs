namespace PathNav.Extensions
{
    using System;
    using System.Collections.Generic;
    using Random = System.Random;

    public class UniqueId : IEqualityComparer<UniqueId>, IEquatable<UniqueId>
    {
        private static Random _random = new();
        private static HashSet<int> _identifierSet = new();

        public int ID { get; }
        public int Index { get; set; }

        private UniqueId(int identifier) => ID = identifier;

        public static UniqueId Generate()
        {
            while (true)
            {
                int identifier = _random.Next(int.MaxValue);
                if (_identifierSet.Contains(identifier)) continue;

                _identifierSet.Add(identifier);
                return new UniqueId(identifier);
            }
        }

        public static void Release(UniqueId identifier)
        {
            _identifierSet.Remove(identifier.ID);
        }

        #region Implementation of Interfaces
        public bool Equals(UniqueId other)
        {
            if (other == null) return false;

            return ID == other.ID;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;

            UniqueId otherID = other as UniqueId;
            if (otherID == null) return false;

            return Equals(otherID);
        }

        public bool Equals(UniqueId x, UniqueId y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            if (x.ID == y.ID) return true;

            return false;
        }

        public int GetHashCode(UniqueId id) => id.ID.GetHashCode();

        public override int GetHashCode() => ID.GetHashCode();

        public static bool operator ==(UniqueId id1, UniqueId id2)
        {
            if ((object)id1 is null || (object)id2 is null)
                return object.Equals(id1, id2);

            return id1.Equals(id2);
        }

        public static bool operator !=(UniqueId id1, UniqueId id2)
        {
            if ((object)id1 is null || (object)id2 is null)
                return !object.Equals(id1, id2);

            return !id1.Equals(id2);
        }
        #endregion
    }
}