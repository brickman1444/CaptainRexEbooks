using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CaptainRexEbooks
{
    public static class TestExtensionMethods
    {
        [Fact]
        public static void GetFirstUnsortedElement_ReturnsSortedValueOnSortedList()
        {
            int[] sortedValues = { 1, 2, 3, 4 };

            int firstUnsortedElement = sortedValues.GetFirstUnsortedElement(-1);

            Assert.Equal(-1, firstUnsortedElement);
        }

        [Fact]
        public static void GetFirstUnsortedElement_ReturnsFirstUnsortedElement()
        {
            int[] sortedValues = { 1, 3, 2, 4 };

            int firstUnsortedElement = sortedValues.GetFirstUnsortedElement(-1);

            Assert.Equal(3, firstUnsortedElement);
        }
    }
}