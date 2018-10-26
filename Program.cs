﻿using System;
using System.IO;
using System.Collections.Generic;

using Markov;

namespace CaptainRexEbooks
{
    static class Program
    {
        public static Stream awsLambdaHandler(Stream inputStream)
        {
            Console.WriteLine("starting via lambda");
            Main(new string[0]);
            return inputStream;
        }

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Beginning program");

            InitializeTwitterCredentials();

            //EvaluateCorpus();
            //EvaluateOrders();

            // string[] quotes = GetQuotes();

            // for (int i = 0; i < 10; i++)
            // {
            //     string quote = GenerateQuote();
            //     bool isCopy = false;
            //     foreach ( string inputQuote in quotes )
            //     {
            //         if (inputQuote.Contains(quote))
            //         {
            //             isCopy = true;
            //             break;
            //         }
            //     }
            //     Console.WriteLine(quote + " " + isCopy);
            // }

            string quote = GenerateQuote();

            TweetQuote(quote);
        }

        static void InitializeTwitterCredentials()
        {
            string consumerKey = System.Environment.GetEnvironmentVariable ("twitterConsumerKey");
            string consumerSecret = System.Environment.GetEnvironmentVariable ("twitterConsumerSecret");
            string accessToken = System.Environment.GetEnvironmentVariable ("twitterAccessToken");
            string accessTokenSecret = System.Environment.GetEnvironmentVariable ("twitterAccessTokenSecret");

            if (consumerKey == null)
            {
                using ( StreamReader fs = File.OpenText( "localconfig/twitterKeys.txt" ) )
                {
                    consumerKey = fs.ReadLine();
                    consumerSecret = fs.ReadLine();
                    accessToken = fs.ReadLine();
                    accessTokenSecret = fs.ReadLine();
                }
            }

            Tweetinvi.Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        static void TweetQuote( string quote )
        {
            Console.WriteLine("Publishing tweet: " + quote);
            var tweet = Tweetinvi.Tweet.PublishTweet(quote);
        }

        static string GenerateQuote()
        {
            Random rand = new Random();

            int order = 1;
            if ( rand.NextDouble() > 0.5 )
            {
                // Add a small chance of a higher order chain. The higher order chains
                // produce output that is a lot closer to the source text. Too close
                // to have on all the time.
                order = 2;
            }

            Console.WriteLine("order " + order);

            MarkovChain<string> chain = GetChain(order);

            string generatedQuote = string.Join(" ", chain.Chain(rand));
            
            // Truncate long quotes to one sentence
            if ( generatedQuote.Length >= 140 )
            {
                char[] sentenceEnders = new char[] { '.', '!', '?' };

                int earliestSentenceEnderIndex = Int32.MaxValue;
                foreach ( char ender in sentenceEnders )
                {
                    int enderIndex = generatedQuote.IndexOf( ender );
                    if ( enderIndex > 0 && enderIndex < earliestSentenceEnderIndex )
                    {
                        earliestSentenceEnderIndex = enderIndex;
                    }
                }
                Console.WriteLine("truncating quote. Original: " + generatedQuote);

                generatedQuote = generatedQuote.Substring(0,earliestSentenceEnderIndex + 1);
            }

            return generatedQuote;
        }

        static MarkovChain<string> GetChain(int order)
        {
            MarkovChain<string> chain = new MarkovChain<string>(order);

            foreach (string sourceQuote in GetQuotes() )
            {
                string[] words = sourceQuote.Split(' ');
                chain.Add(words);
            }

            return chain;
        }

        // We want the average options per state to be between 1.5 to 2.0 ideally.
        // If it's less than that then we're likely directly quoting the input text.
        static void EvaluateOrders()
        {
            for (int order = 1; order < 5; order++)
            {
                MarkovChain<string> chain = GetChain(order);

                var states = chain.GetStates();
                int numStates = 0;
                int numOptions = 0;
                foreach (ChainState<string> state in states)
                {
                    var nextStates = chain.GetNextStates(state);
                    int terminalWeight = chain.GetTerminalWeight(state);
                    numStates++;
                    numOptions += (nextStates != null ? nextStates.Count : 0);
                    numOptions += (terminalWeight > 0 ? 1 : 0); // If this is a possible termination of the chain, that's one option
                }

                Console.WriteLine("Order: " + order + " NumStates: " + numStates + " NumOptionsTotal: " + numOptions + " AvgOptionsPerState " + ((float)numOptions / (float)numStates));
            }
        }

        static void EvaluateCorpus()
        {
            string[] quotes = GetQuotes();
            Console.WriteLine("NumLines: " + quotes.Length);

            HashSet<string> uniqueWords = new HashSet<string>();
            int numTotalWords = 0;

            foreach ( string line in quotes )
            {
                string[] words = line.Split(' ');

                numTotalWords += words.Length;
                uniqueWords.UnionWith( words );

                foreach ( string word in words )
                {
                    if ( word == null || word == "" )
                    {
                        throw new Exception("Invalid string in corpus.");
                    }
                }
            }

            Console.WriteLine("NumTotalWords: " + numTotalWords);
            Console.WriteLine("NumUniqueWords: " + uniqueWords.Count);
        }

        static string[] GetQuotes()
        {
            return new string[] {
                "Actually my name is Rex. Captain, 501st Clone Battalion. Meet Commanders Gregor and Wolffe.",
                "All here, Sir.",
                "All right, get in your groups. Let's move out.",
                "All right, I'm putting my pistols down.",
                "All right, let's go!",
                "All right, listen up. We'll assemble the squads into two divisions. We'll move straight up this gorge towards the airbase at the far side.",
                "Ambush!",
                "And all you had to do to get it was put the rest of us all at risk.",
                "And you are a natural.",
                "Anyone else in the mess able to confirm what you two are saying?",
                "Anyone with you?",
                "A few of General Skywalker's plans seemed reckless too, but they worked.",
                "A lot of the General's plans involve falling.",
                "A lot of General Skywalker's plans seemed reckless at times, but they worked.",
                "Battle droid fingers.",
                "Battle stations.",
                "Better contact the Jedi.",
                "Blast it!",
                "Boys, this might be it. At least we'll go down fighting like a clone should.",
                "But a Jedi won't.",
                "But you're a Jedi. How could you?",
                "But, Sir, General Skywalker's plan was to surprise them with multiple attacks. If we come in from the main route, they're likely to engage us in a full-frontal assault.",
                "By whom?",
                "Can you sweep 'em?",
                "Can't we destroy the supply ship?",
                "Come on, Wolffe. It's just a scratch. Keep moving forward, soldier! Onward!",
                "Come out and fight, you cowards!",
                "Commander. You got old.",
                "Congratulations. You're not shinies anymore.",
                "Despite Hardcase's flying, you two saved us all.",
                "Doesn't matter, kid. We have to retake this base, so we will retake this base.",
                "Don't do a job till you've guaranteed the best odds, right?",
                "Don't worry about a thing, SIr. We'll have this city under Republic control by the time you're back.",
                "Do I have a choice?",
                "Do you have any idea what you've done?",
                "Don't worry, kid. I'll take good care of him.",
                "Either way, he's in charge and we've got a job to do. Just treat him with respect and we'll all get along fine.",
                "Everyone regroup, now! Take cover!",
                "Everyone, keep moving. Hey, Kix, leave it.",
                "Everyone, stop firing! We're shooting at our own men! They're not Umbarans, they're clones! Take off your helmets. Show them you're not the enemy.",
                "Everyone, take cover!",
                "Excuse me, Sir?",
                "Explain your actions.",
                "Fives! Fives! Fives...",
                "Fives? Are you clear?",
                "Fives... It would help if you'd ease their minds.",
                "Fives, no!",
                "Fives, we are listening to you. We only want to help.",
                "Forget it. We have to leave them.",
                "Four? Well, how are they not falling over?",
                "Full stop, Wolffe! Dig in! This is where we finish the battle!",
                "Generals, they had all our intel.",
                "General Kenobi's battalion, Sir?",
                "General Krell, the top of this ridge will make a good place for the men to make camp.",
                "General Krell, we've come up with a plan to infiltrate the airbase.",
                "General Skywalker, explosives are in place, Sir. Objective completed.",
                "General Skywalker, where are you?",
                "General, the Defender is contacting us. There seems to be a problem.",
                "General, we have taken the base and cut off enemy supply lines to the capital.",
                "Get ready! Here they come!",
                "Get those rocket launchers down there. Move it, troopers!",
                "Good job. All right, let's move out.",
                "Go! Go, go, go! Keep moving! Keep moving!",
                "Great shot, kid. Now get yourself moving. This is your only chance.",
                "Gregor, I've lost power to the main cannon!",
                "Gregor, you've still got it.",
                "Hands above your heads. Take your sun bonnets off.",
                "Here they are, the coordinates of every Republic base, Separatist installation, pirate hideout and smuggler's den in the Outer Rim.",
                "Hevy, get out of there.",
                "Hey! Stop!",
                "Hey, hey. Easy with those, son.",
                "Hey, I bet you know a thing or two about mechanics.",
                "Hey, kid.",
                "He gave us a bit of a chase, Sir.",
                "He knew where we'd look. He's not trying to escape.",
                "He wants to get around the lockdown. He's blinded us by taking out the power. He could disable the entire security system.",
                "He's running. Hit that line! Bring him up.",
                "He's just trying to keep us on schedule.",
                "He's wound tight, but he's loyal.",
                "Hit it! Now!",
                "Hold your ground!",
                "How do we get this guy?",
                "How is Commander Tano?",
                "How many legs they got?",
                "Huh. Reinforced armor plating, heavy cannons and antipersonnel blasters. Mmm.",
                "If it's a fight you want, I hope you brought a better class of soldier than those Stormtroopers",
                "If they follow procedure, they'll fly search patterns based on our last confirmed position.",
                "In my book, experience outranks everything.",
                "Is everyone clear on the plan? Hardcase?",
                "Is that true, Sir?",
                "It just doesn't make sense. Blast!",
                "It wasn't all luck, Sir. A lot of men died to take this base.",
                "It's Captain, Sir.",
                "It's Slick? Slick's the traitor?",
                "It's the pattern. The band's only coming off one terminal in the whole base. Check it out.",
                "It's too hard to tell.",
                "I always trust my general.",
                "I am not training him.",
                "I bet you sold out your brothers for some real shiny coin, huh?",
                "I didn't betray my Jedi. Wolffe, Gregor and I all removed our control chips. We all have a choice.",
                "I do love a battle, but on my terms. Gregor, drop the joopa. We gotta get moving.",
                "I don't know. I'll get back to you on that if we survive this battle.",
                "I fought by her side from the Battle of Christophsis to the Siege of Mandalore, and a friend of hers is a friend of mine.",
                "I haven't heard those digits in... Well, that's my birth number.",
                "I have a bad feeling about this.",
                "I have a few ideas.",
                "I have no idea.",
                "I honor my code. That's what I believe.",
                "I know Commander Tano. She would never do something like this.",
                "I know. But they weren't the ones that betrayed us. Remember? Wolffe, remember?",
                "I raised my objection to General Krell's plan, but he didn't agree. So this is it.",
                "I think freedom's gonna have to wait, kid.",
                "I'll get 'em. But you might consider staying for dinner.",
                "I'm always first, kid.",
                "I'm glad you're still alive.",
                "I'm going for the legs, all four of them.",
                "I'm no Jedi.",
                "I'm on it. He must have gone in the mess hall.",
                "I'm part of the most pivotal moment in the history of the Republic. If we fall then our children and their children could be forced to live under an evil I can't well imagine.",
                "I've assembled a list of potential bases and clearance codes and a few protocols the Imperials still use. Should be of some use.",
                "I've called in an air strike on the enemy positions.",
                "I've dispatched two men on a stealth incursion into the airbase. They've been ordered to co-opt starfighters and use them against the tanks.",
                "Jesse, take the right flank. Dogma, take the left flank.",
                "Just like the old days.",
                "Just make it fast. Those droids are getting close.",
                "Kanan's right. We need one Jedi up there manning the cannon and another Jedi down here to lead us out of this mess. You are the only ones who can see in this storm. Sabine, spot him. Hey, kid. You might need that. And hang on tight.",
                "Keep an eye on this regulator. The line can overheat and shut down. No line, no joopa, no Zeb.",
                "Keep movin'. We've got to claim that ridge. The other battalions are counting on us!",
                "Keep the wounded as quiet as possible. All right, you heard the General. Let's go.",
                "Krell may do things differently, but he is effective at getting them done. He's a recognized war hero.",
                "Let's move.",
                "Looks like we got ourselves a batch of shinies, Commander.",
                "Looks like we live to fight another day.",
                "Look sharp, rookies. As long as those tweezers occupy this post, our home planet of Kamino is at risk.",
                "Look, Kix, it's more important to save yourself right now. If we survive, you can patch up the wounded later.",
                "Maybe this tactical droid will tell us how they knew our plan.",
                "Maybe, back in the day.",
                "Mines! Nobody move!",
                "Move it, trooper.",
                "My cover's blown, it's time to go, but I decide not to use a ship because it's too obvious.",
                "Next time, just tell me to jump.",
                "Nice to have you on board.",
                "No, Fives, come on stay with me. Stay with me, Fives! Fives!",
                "Of course we did. We're getting to the bottom of this. Now.",
                "Of course we knew. You think we wouldn't have a plan?",
                "Oh, I'm proud of my service. But I really hate this armor.",
                "Oh, we can take care of ourselves.",
                "Ok, listen up. We're going to form two divisions, march down, and head through the gorge to the airbase.",
                "One of us? Great, but which one?",
                "Or does he? Yeah, I see what you're getting at.",
                "Relax. Just as I thought. Looks like one of those new commando droids.",
                "Remember what you were saying about finding another way to destroy those tanks?",
                "R2, come over here and plug in.",
                "R2's found something.",
                "Scrap 'em!",
                "She distracted him while we completed the mission. It was on her orders, Sir. The droid was with her.",
                "She engaged General Grievous.",
                "Sir, if I may address your accusation; I followed your orders, even in the face of a plan that was in my opinion severely flawed, a plan that cost us men, not clones! Men! As sure as it is my duty to remain loyal to your command, I also have another duty, to protect those men.",
                "Sir, I thought you said you'd never have a Padawan.",
                "Sir, looks like the General's up to something.",
                "Sir, the rocket launchers don't work on these tanks, and it'll be easier to slip by undetected while the rest of us keep the tanks occupied.",
                "Sir, the terrain is extremely hostile. Despite the difficulty of the conditions, the battalion is making good time. These men just need a little break.",
                "Sir, we can't possibly...",
                "Sir, we're overpowered. We need reinforcements.",
                "Sir, we're ready to bring our forward platoons in for a surgical strike on the city's defenses.",
                "Sir, yes, Sir.",
                "Slick pretty much scorched the whole thing. That seemed to be what he was going for all along.",
                "Slick's not gonna like that.",
                "Something's not right here.",
                "Sorry for wasting your time.",
                "Sorry, son. My days as a soldier are over.",
                "South tower? We're in the North.",
                "So, Chopper, old boy, what's your alibi?",
                "Stand down, troopers. Now! That's an order, soldier.",
                "Stand fast. Hit 'em with everything you've got.",
                "Surrender, General.",
                "Surrender, General, you're outnumbered.",
                "Take them off! Now!",
                "Take us to the Sergeant in command.",
                "That one's still got some juice in it. Waste it!",
                "That was close.",
                "That will never happen. You're a traitor, General, and you will be dealt with as one.",
                "That's all we might get, one shot.",
                "That's just great.",
                "That's not your choice to make. You swore an oath to the Republic. You have a duty.",
                "That's right. Your armor, it's shiny and new, just like you.",
                "That's the price of war, Fives. We're soldiers. We have a duty to follow orders, and, if we must, lay down our lives for victory.",
                "Then we'll destroy the outpost instead.",
                "There's an opening to our South. I recommend we move all platoons off the ridge in case the air strike overshoots.",
                "There's a base there, all right. And it's heavily guarded. At least three tank divisions, plus guns.",
                "There's a lot of surface fire.",
                "There's hope for you yet, rookie.",
                "There's no escape now, you piece of rankweed! Move. Move!",
                "These boys are sloppy. There should always be an officer on duty.",
                "They'll be coming around any second. Bring up the launchers. Spread detonators along that corridor. Trap them into the bottleneck. We're going to blow those things sky high. Hurry up. We only have a few seconds.",
                "They're falling back!",
                "The General just gave you an order, soldier.",
                "The General's giving you an order, Dogma.",
                "The gorge is narrow, Sir. We'll only be able to move our platoons in single squads. Perhaps a closer recon will tell us if there's a more secure route.",
                "The guard got his messages out there somehow. We just got to find them.",
                "The name's Rex, but you can call me Captain or Sir.",
                "The only people in here are brothers.",
                "The traverse controls are over here on the left, and the elevation's on the right.",
                "The Umbarans are advancing!",
                "The Umbarans must've regrouped for a counterattack. Everyone, we must hold this position!",
                "The war is over. We are free men. We can't live under the fear of the Empire for the rest of our lives, Wolffe. That's not freedom.",
                "The way I figure it, you tell the truth, you got nothing to be nervous about.",
                "Things like what?",
                "Think so? Wolffe, evasive maneuvers.",
                "This detonator isn't working.",
                "This isn't the time for a debate. Right now we have to stay alert.",
                "This map has a ridge at 23 degrees North, Northwest.",
                "Too late. The walkers are already on top of us.",
                "Try shortening your leads. You're wasting ammo.",
                "Tup, can you hear me?",
                "Ugh.",
                "Uh, it's hard to tell.",
                "Understood, Sir.",
                "Waiting for orders, Sir.",
                "Wait, what? You're mistaken. We would never do that.",
                "Watch your left!",
                "Wavelength interference, weak frequencies, spotty, irregular. See, how it shows up every few days then disappears? Day to day, you wouldn't notice it.",
                "Well, I have a mission for you.",
                "Well, I told you!",
                "Well, if you've got a better idea, Sir, now's the time.",
                "Well, just in case it wasn't us, I mean...",
                "Well, that sure complicates things, Commander.",
                "Well, that's too bad, 'cause there's nobody out here.",
                "Well, the Empire certainly isn't the Republic, but you can't do anything about that.",
                "Well, the Empire is here because of us. We'll deal with the consequences.",
                "Well, this takes trust to a whole new level.",
                "We don't have any cover! We need to pull back. Get them to follow us. If we can draw them out, we can see them. If we can see them, we can hit them. All squads, pull back now!",
                "We don't have the firepower to take on walkers.",
                "We don't have time. Look.",
                "We have got to move before those fighters come back.",
                "We have no assault craft, Sir, only a couple of fighters and the Twilight.",
                "We have to warn command.",
                "We have to warn the Republic about the invasion. They'll take notice when the all-clear signal stops.",
                "We need to fall back, get them to follow us. If we can draw them out, we can see them. If we can see them, we can hit them! All squads, fall back now!",
                "We need to hold out as long as we can. I'm trusting Fives and Hardcase to pull this thing off.",
                "We roll the proton bombs. We roll them across the hangar into the feet of the walkers, and then we blast them.",
                "We'll need every thermal detonator in the inventory.",
                "We're cut off. Off the platform!",
                "We're soldiers, Ezra. This is what we were born to do.",
                "We've been outflanked.",
                "We've got a problem. Fall back! Fall back now!",
                "We've still got some fight left in us, Tup, and I've got an idea.",
                "What are you gonna do?",
                "What did you just say?",
                "What do you want?",
                "What happened out there?",
                "What is it?",
                "What the...",
                "What? I never got any messages from Commander Tano.",
                "Where were you before you went to the mess, Chopper?",
                "Who's the youngling?",
                "Why General? Why kill your own men?",
                "With all due respect, General, we don't know what we're up against. It might be wiser to think first.",
                "With all due respect, Senator, it's what these men were born to do.",
                "Wolffe, bring us around.",
                "Wolffe, set vector two-niner-zero.",
                "Wolffe, turn 180.",
                "Wolffe, what did you do?",
                "Yeah, well you should have used kill.",
                "Yeah. Like I said, our war's over, kid. Don't much care to get mixed up in another. Oh, and say hello to Commander Tano for me.",
                "Yeah. Ready your weapons.",
                "Yes, General.",
                "Yes, General.",
                "Yes, Sir.",
                "Yes, Sir. On some beat-up old space freighter. I’ll be surprised if he even makes it to Tatooine in that junker.",
                "Yep. And the Empire's on its way.",
                "Your reputation precedes you, General. It is an honor to be serving you.",
                "You betrayed everyone of us.",
                "You don't have a choice. That's an order.",
                "You got any ideas? Then this is it.",
                "You have to start somewhere.",
                "You know what's funny, traitor? We knew you'd never take a chance on the exits while they were blocked.",
                "You know, I can't figure those villagers not wanting to fight. No pride I guess.",
                "You know, I don't think he likes me. Or ever will. Can't say I blame him. The war left its scars on all of us.",
                "You know, I've outserved my purpose for that kind of fighting, I'm afraid. After the war, I questioned the point of the whole thing. All those men died, and for what?",
                "You put us right in the middle of 'em to get us a shot. It's crazy, but it's probably our best chance.",
                "You shot us!",
                "You showed me something today. You're exactly the kind of men I need in the 501st.",
                "You want the shot?",
                "You were brave today, kid. You jumped right in there to help. A great Jedi once told me that the best leaders lead by example. You do that well.",
                "You're an agent of Dooku.",
                "You're a Separatist.",
                "You're gonna need all of 'em.",
                "You're not abandoning anyone. We're covering your escape. Now, move.",
            };
        }
    }
}
