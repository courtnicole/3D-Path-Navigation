namespace PathNav.Extensions   
{
    using System.Collections.Generic;

    public static class ListExtensions 
    {
        public static bool AddListValue<TValue>(List<TValue> list, TValue value, bool preventDuplicates = false)
        {
            if (list == null || (preventDuplicates && list.Contains(value))) return false;

            list.Add(value);
            return true;
        }

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            var dest = new List<T>(source);
            dest.RemoveAt(index);
            return dest.ToArray();
        }
    }
}
