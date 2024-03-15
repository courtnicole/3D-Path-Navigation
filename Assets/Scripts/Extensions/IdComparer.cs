namespace PathNav.Extensions
{
    using System.Collections.Generic;
    using UnityEngine;

    public class IdComparer : Comparer<UniqueId>
    {
        #region Overrides of Comparer<UniqueId>
        // return -1	x < y.
        // return 0	    x = y.
        // return 1	    x > y.
        public override int Compare(UniqueId x, UniqueId y)
        {
            if (x is null     && y is null) return 0;
            if (x is not null && y is null) return 1;
            if (x is null) return -1;
            
            if (x.Index > y.Index)
            {
                return 1;
            }
            if (x.Index < y.Index){
                return -1;
            }
            if (x.Index == y.Index) {
                return 0;
            }

            return 0;
        }
        #endregion
    }
}