using System.Collections.Generic;

namespace MaceEvolve.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static void RemoveRange<T>(this ICollection<T> self, IEnumerable<T> itemsToRemove)
        {
            foreach (var item in itemsToRemove)
            {
                self.Remove(item);
            }
        }
    }
}
