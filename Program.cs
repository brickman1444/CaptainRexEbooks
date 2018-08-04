﻿using System;

using Markov;

namespace AppraisalBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var chain = new MarkovChain<string>(2);

            foreach (string quote in GetQuotes() )
            {
                string[] words = quote.Split(' ');
                chain.Add(words);
            }

            var rand = new Random();

            for (int i = 0; i < 30; i++)
            {
                var sentence = string.Join(" ", chain.Chain(rand));
                Console.WriteLine(sentence);
            }
        }

        static string[] GetQuotes()
        {
            return new string[] {
                "Alright, I'm putting my pistols down",
                "By whom?",
                "Do you have any idea what you've done?",
                "Don't worry, kid. I'll take good care of him.",
                "Fives! Fives! Fives...",
                "Fives, no, Fives, come on, Fives. Come on, stay with me, stay with me, Fives. Fives, don't go.",
                "Fives, no!",
                "Fives, we are listening to you. We only want to help.",
                "Hands above your heads. Take your sun bonnets off.",
                "In my book, experience outranks everything.",
                "I have no idea.",
                "Is that true, sir?",
                "I am not training him.",
                "I hope you've brought a better class of soldiers than those Stormtroopers.",
                "I'm always first, kid.",
                "Looks like we got ourselves a batch of shinies, Commander.",
                "No, Fives, come on stay with me. Stay with me Fives! Fives!",
                "Scrap 'em!",
                "Sir, I thought you said you'd never have a Padawan.",
                "Something's not right here.",
                "That's just great.",
                "That's not your choice to make. You swore an oath to the Republic. You have a duty.",
                "That's right. Your armor, it's shiny and new, just like you.",
                "There's hope for you yet, rookie.",
                "The name's Rex, but you can call me captain or sir.",
                "Tup, can you hear me?",
                "What happened out there?",
                "What is it?",
                "Who's the youngling?",
                "With all due respect, Senator, it's what these men were born to do.",
                "Yes, sir. On some beat-up old space freighter. I’ll be surprised if he even makes it to Tatooine in that junker.",
                "You know, I can't figure those villagers not wanting to fight. No pride I guess.",
                "You showed me something today. You're exactly the kind of men I need in the 501st.",
            };
        }
    }
}
