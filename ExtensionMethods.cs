using System;
using System.Linq;
using System.Collections.Generic;

namespace CaptainRexEbooks
{
    public static class ExtensionMethods
    {
        public static bool HasDuplicateEntries<T>( this IEnumerable<T> enumerable)
        {
            return enumerable.Count() != enumerable.Distinct().Count();
        }
        
        public static bool HasDuplicateEntries<T>( this IEnumerable<T> enumerable, IEqualityComparer<T> comparer )
        {
            return enumerable.Count() != enumerable.Distinct( comparer ).Count();
        }

        public static T GetFirstUnsortedElement<T>( this IEnumerable<T> enumerable, T sortedValue ) where T : IComparable
        {
            if ( enumerable.Count() == 0 )
            {
                throw new ArgumentException("enumerable shouldn't be empty.");
            }

            for ( int i = 0; i < enumerable.Count() - 1; i++)
            {
                T currentElement = enumerable.ElementAt(i);
                T nextElement = enumerable.ElementAt(i + 1);

                if (currentElement.CompareTo(nextElement) >= 0)
                {
                    return currentElement;
                }
            }

            return sortedValue;
        }
    }
}