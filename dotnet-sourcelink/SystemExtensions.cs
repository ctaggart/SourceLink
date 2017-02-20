using System.Collections.Generic;
using System.Linq;

namespace SourceLink
{
    // https://github.com/ctaggart/SourceLink/blob/v1/SourceLink/SystemExtensions.fs
    public static class SystemExtensions
    {
        public static bool CollectionEquals<T>(this ICollection<T> a, ICollection<T> b)
        {
            if (a.Count != b.Count)
                return false;
            return a.SequenceEqual(b);
        }
    }
}
