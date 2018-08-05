﻿using System;

using Markov;

namespace AppraisalBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var chain = new MarkovChain<string>(1);

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
                "Actually my name is Rex. Captain, 501st Clone Battalion. Meet Commanders Gregor and Wolffe.",
                "Alright, I'm putting my pistols down.",
                "Ambush!",
                "And all you had to do to get it was put the rest of us all at risk.",
                "And you are a natural.",
                "A lot of the General's plans involve falling.",
                "A lot of General Skywalker's plans seemed reckless at times, but they worked.",
                "By whom?",
                "But you're a Jedi. How could you?",
                "Can't we destroy the supply ship?",
                "Commander. You got old.",
                "Congratulations. You're not shinies anymore.",
                "Doesn't matter, kid. We have to retake this base, so we will retake this base.",
                "Do I have a choice?",
                "Do you have any idea what you've done?",
                "Don't worry, kid. I'll take good care of him.",
                "Everyone, stop firing! We're shooting at our own men! They're not Umbarans, they're clones! Take off your helmets. Show them you're not the enemy.",
                "Explain your actions.",
                "Fives! Fives! Fives...",
                "Fives, no, Fives, come on, Fives. Come on, stay with me, stay with me, Fives. Fives, don't go.",
                "Fives, no!",
                "Fives, we are listening to you. We only want to help.",
                "Full stop, Wolffe! Dig in! This is where we finish the battle!",
                "General Skywalker, explosives are in place, Sir. Objective completed.",
                "General Skywalker, where are you?",
                "General, the Defender is contacting us. There seems to be a problem.",
                "Hands above your heads. Take your sun bonnets off.",
                "Hevy, get out of there.",
                "Hey, hey. Easy with those, son.",
                "Hey, I bet you know a thing or two about mechanics.",
                "Hey, kid.",
                "He gave us a bit of a chase, Sir.",
                "He's a runnin'. Hit that line! Bring him up.",
                "He's wound up tight, but he's a good soldier.",
                "Hit it! Now!",
                "Hold your ground!",
                "How is Commander Tano?",
                "If it's a fight you want, I hope you brought a better class of soldier than those Stormtroopers",
                "In my book, experience outranks everything.",
                "Is that true, Sir?",
                "It's Captain, Sir.",
                "It's Slick? Slick's the traitor?",
                "It's too hard to tell.",
                "I am not training him.",
                "I bet you sold out your brothers for some real shiny coin, huh?",
                "I didn't betray my Jedi. Wolffe, Gregor and I all removed our control chips. We all have a choice.",
                "I fought by her side from the Battle of Christophsis to the Siege of Mandalore, and a friend of hers is a friend of mine.",
                "I haven't heard those digits in... Well, that's my birth number.",
                "I have a bad feeling about this.",
                "I have a few ideas.",
                "I have no idea.",
                "I know Commander Tano. She would never do something like this.",
                "I know. But they weren't the ones that betrayed us. Remember? Wolffe, remember?",
                "I'll get 'em. But you might consider staying for dinner.",
                "I'm always first, kid.",
                "I'm no Jedi.",
                "I'm part of the most pivotal moment in the history of the Republic. If we fall then our children and their children could be forced to live under an evil I can't well imagine.",
                "I've assembled a list of potential bases and clearance codes and a few protocols the Imperials still use. Should be of some use.",
                "I've dispatched two men on a stealth incursion into the airbase. They've been ordered to co-opt starfighters and use them against the tanks.",
                "Just make it fast. Those droids are getting close.",
                "Keep an eye on this regulator. The line can overheat and shut down. No line, no joopa, no Zeb.",
                "Let's move.",
                "Looks like we got ourselves a batch of shinies, Commander.",
                "Looks like we live to fight another day.",
                "Look sharp, rookies. As long as those tweezers occupy this post, our home planet of Kamino is at risk.",
                "Maybe, back in the day.",
                "Next time, just tell me to jump.",
                "No, Fives, come on stay with me. Stay with me Fives! Fives!",
                "Oh, I'm proud of my service. But I really hate this armor.",
                "Ok, listen up. We're going to form two divisions, march down, and head through the gorge to the airbase.",
                "Relax. Just as I thought. Looks like one of those new commando droids.",
                "Scrap 'em!",
                "She distracted him while we completed the mission. It was on her orders, Sir. The droid was with her.",
                "She engaged General Grievous.",
                "Sir, I thought you said you'd never have a Padawan.",
                "Sir, looks like the General's up to something.",
                "Something's not right here.",
                "Sorry, son. My days as a soldier are over.",
                "Stand down, troopers. Now! That's an order, soldier.",
                "Surrender, General.",
                "Surrender, General, you're outnumbered.",
                "Take them off! Now!",
                "Take us to the Sergeant in command.",
                "That will never happen. You're a traitor, General, and you will be dealt with as one.",
                "That's just great.",
                "That's not your choice to make. You swore an oath to the Republic. You have a duty.",
                "That's right. Your armor, it's shiny and new, just like you.",
                "Then we'll destroy the outpost instead.",
                "There's hope for you yet, rookie.",
                "These boys are sloppy. There should always be an officer on duty.",
                "The General just gave you an order, soldier.",
                "The name's Rex, but you can call me Captain or Sir.",
                "The war is over. We are free men. We can't live under the fear of the Empire for the rest of our lives, Wolffe. That's not freedom.",
                "This detonator isn't working.",
                "Tup, can you hear me?",
                "Waiting for orders, Sir.",
                "Wait, what? You're mistaken. We would never do that.",
                "Well, I told you!",
                "Well, just in case it wasn't us, I mean...",
                "Well, that sure complicates things, Commander.",
                "Well, that's too bad, 'cause there's nobody out here.",
                "Well, the Empire certainly isn't the Republic, but you can't do anything about that.",
                "Well, this takes trust to a whole new level.",
                "We don't have the firepower to take on walkers.",
                "We don't have time. Look.",
                "We have no assault craft, Sir, only a couple of fighters and the Twilight.",
                "We have to warn command.",
                "We have to warn the Republic about the invasion. They'll take notice when the all-clear signal stops.",
                "We need to fall back, get them to follow us. If we can draw them out, we can see them. If we can see them, we can hit them! All squads, fall back now!",
                "We roll them - the proton bombs. We roll them across the hangar into the feet of the walkers, and then we blast them.",
                "We'll need every thermal detonator in the inventory.",
                "We're cut off. Off the platform!",
                "What did you just say?",
                "What do you want?",
                "What happened out there?",
                "What is it?",
                "What? I never got any messages from Commander Tano.",
                "Who's the youngling?",
                "Why General? Why kill your own men?",
                "With all due respect, Senator, it's what these men were born to do.",
                "Wolffe, what did you do?",
                "Yeah well you should have used kill.",
                "Yes, Sir. On some beat-up old space freighter. I’ll be surprised if he even makes it to Tatooine in that junker.",
                "Yep. And the Empire's on its way.",
                "You know, I can't figure those villagers not wanting to fight. No pride I guess.",
                "You know, I don't think he likes me. Or ever will. Can't say I blame him. The war left its scars on all of us.",
                "You know, I've outserved my purpose for that kind of fighting, I'm afraid. After the war, I questioned the point of the whole thing. All those men died, and for what?",
                "You shot us!",
                "You showed me something today. You're exactly the kind of men I need in the 501st.",
                "You were brave today, kid. You jumped right in there to help. A great Jedi once told me that the best leaders lead by example. You do that well.",
                "You're an agent of Dooku.",
                "You're a Separatist.",
            };
        }
    }
}
