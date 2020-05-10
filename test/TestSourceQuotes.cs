using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CaptainRexEbooks
{
    public static class TestSourceQuotes
    {
        [Fact]
        public static void SourceQuotes_DontContainDuplicateEntries()
        {
            List<string> quotesSoFar = new List<string>();
            foreach (string quote in Program.GetQuotes())
            {
                Assert.DoesNotContain(quote, quotesSoFar);
                quotesSoFar.Add(quote);
            }
            Assert.Equal(Program.GetQuotes().Count(), quotesSoFar.Count());
        }

        [Fact]
        public static void SourceQuotes_AllHaveEnders()
        {
            string[] enders = { ".", "?", "!", "\"" };

            foreach (string quote in Program.GetQuotes())
            {
                bool quoteEndsWithEnder = enders.Any(ender => quote.EndsWith(ender));
                Assert.True(quoteEndsWithEnder, "Quote [" + quote + "] doesn't end with a valid ender.");
            }
        }
    }
}