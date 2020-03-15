using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CaptainRexEbooks
{
    public static class TestSourceQuotes
    {
        [Fact]
        public static void SourceQuotes_AreSorted()
        {
            string firstUnsortedValue = Program.GetQuotes().GetFirstUnsortedElement(null);

            var quotes = Program.GetQuotes();
            Array.Sort(quotes);
            Console.WriteLine(quotes);

            Assert.Null(firstUnsortedValue);
        }

        [Fact]
        public static void SourceQuotes_DontContainDuplicateEntries()
        {
            Assert.False(Program.GetQuotes().HasDuplicateEntries());
        }

        [Fact]
        public static void SourceQuotes_AllHaveEnders()
        {
            string[] enders = { ".", "?", "!", "\"" };

            foreach ( string quote in Program.GetQuotes() )
            {
                Assert.Contains( enders, ender => quote.EndsWith( ender ) );
            }
        }
    }
}