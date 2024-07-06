using System.Collections.Generic;

namespace QArantine.Code.FrameworkUtils
{
    public static class CollectionUtils
    {
        public static bool DictionaryEquals<TKey, TValue>( this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if (first == null || second == null)
                return false;

            if (first.Count != second.Count)
                return false;

            foreach (var key in first.Keys)
            {
                if (!second.TryGetValue(key, out var secondValue) ||
                    !Equals(first[key], secondValue))
                {
                    return false;
                }
            }

            return true;
        }
    }
}