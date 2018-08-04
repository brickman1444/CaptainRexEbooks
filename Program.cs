﻿using System;

using Markov;

namespace AppraisalBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var chain = new MarkovChain<string>(1);

            chain.Add(new[] { "Once", "upon", "a", "time." });
            chain.Add(new[] { "Once", "there", "was", "a", "pig." });
            chain.Add(new[] { "There", "once", "was", "a", "man", "from", "Nantucket." });

            var rand = new Random();

            for (int i = 0; i < 10; i++)
            {
                var sentence = string.Join(" ", chain.Chain(rand));
                Console.WriteLine(sentence);
            }
        }
    }
}
