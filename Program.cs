﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using dotenv.net;
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

            DotEnv.Load();

            if (args.Length != 0 && args[0] == "sample-output")
            {
                SampleOutput();
            }
            else if (args.Length != 0 && args[0] == "evaluate-corpus-and-orders")
            {
                EvaluateCorpusAndOrders();
            }
            else
            {
                GenerateQuoteAndPost();
            }
        }

        static void SampleOutput()
        {
            string[] quotes = GetQuotes();

            for (int i = 0; i < 20; i++)
            {
                string quote = GenerateQuoteWithBackoff();
                bool isCopy = false;
                foreach (string inputQuote in quotes)
                {
                    if (inputQuote.Contains(quote))
                    {
                        isCopy = true;
                        break;
                    }
                }
                Console.WriteLine(quote + " Is Copy: " + isCopy);
            }
        }

        static void EvaluateCorpusAndOrders()
        {
            EvaluateCorpus();
            EvaluateOrders();
        }

        static void GenerateQuoteAndPost()
        {
            string quote = GenerateQuoteWithBackoff();

            Task twitter = Twitter.PostStatus(quote);
            Task mastodon = Mastodon.PostStatus(quote);

            Task.WaitAll(twitter, mastodon);
        }

        static string GenerateQuote()
        {
            Random rand = new Random();

            int order = 1;
            if (rand.NextDouble() > 0.5)
            {
                // Add a small chance of a higher order chain. The higher order chains
                // produce output that is a lot closer to the source text. Too close
                // to have on all the time.
                order = 2;
            }

            Console.WriteLine("order " + order);

            MarkovChain<string> chain = GetChain(order);

            string generatedQuote = string.Join(" ", chain.Chain(rand));

            return TruncateQuote(generatedQuote);
        }

        static string GenerateQuoteWithBackoff()
        {
            Random rand = new Random();

            // Order 3 is the highest order that actually makes a meaningful difference.
            // Targeting 2 next states seems like it hits a balance of high and low orders.
            MarkovChainWithBackoff<string> chain = GetChainWithBackoff(3, 2);

            IEnumerable<string> result = chain.Chain(rand);
            string generatedQuote = string.Join(" ", result);

            return TruncateQuote(generatedQuote);
        }

        static string TruncateQuote(string quote)
        {
            // Truncate long quotes to one sentence
            if (quote.Length < 140)
            {
                return quote;
            }

            char[] sentenceEnders = new char[] { '.', '!', '?' };

            int earliestSentenceEnderIndex = Int32.MaxValue;
            foreach (char ender in sentenceEnders)
            {
                int enderIndex = quote.IndexOf(ender);
                if (enderIndex > 0 && enderIndex < earliestSentenceEnderIndex)
                {
                    earliestSentenceEnderIndex = enderIndex;
                }
            }
            Console.WriteLine("truncating quote. Original: " + quote);

            return quote.Substring(0, earliestSentenceEnderIndex + 1);
        }

        static MarkovChain<string> GetChain(int order)
        {
            MarkovChain<string> chain = new MarkovChain<string>(order);

            foreach (string sourceQuote in GetQuotes())
            {
                string[] words = sourceQuote.Split(' ');
                chain.Add(words);
            }

            return chain;
        }

        static MarkovChainWithBackoff<string> GetChainWithBackoff(int maxOrder, int desiredNumNextStates)
        {
            MarkovChainWithBackoff<string> chain = new MarkovChainWithBackoff<string>(maxOrder, desiredNumNextStates);

            foreach (string sourceQuote in GetQuotes())
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

            foreach (string line in quotes)
            {
                string[] words = line.Split(' ');

                numTotalWords += words.Length;
                uniqueWords.UnionWith(words);

                foreach (string word in words)
                {
                    if (word == null || word == "")
                    {
                        throw new Exception("Invalid string in corpus.");
                    }
                }
            }

            Console.WriteLine("NumTotalWords: " + numTotalWords);
            Console.WriteLine("NumUniqueWords: " + uniqueWords.Count);
        }

        public static string[] GetQuotes()
        {
            return new string[] {
                "99, hey? Nice touch.",
                "A few of General Skywalker's plans seemed reckless too, but they worked.",
                "A live signal?",
                "A lot of General Skywalker's plans seemed reckless at times, but they worked.",
                "A lot of the General's plans involve falling.",
                "Actually my name is Rex. Captain, 501st Clone Battalion. Meet Commanders Gregor and Wolffe.",
                "Actually, I was thinking we'd take a page from your book. Rush them head-on.",
                "After Clone Order 66, the whole droid army was given a shutdown command. How are you even operating?",
                "Ah, he's spot-checking my gear, Sir.",
                "Ah, Kanan. I'm sorry. I thought you were someone else.",
                "Ah, one last glorious day in the Grand Army of the Republic.",
                "Ah, we roll 'em! The proton bombs. We roll 'em across the hangar and into the feet of the walkers. Then we blast 'em!",
                "Ah, you're right. He is.",
                "Ahsoka, you don't have to go to Malachor alone. I can be there in two rotations.",
                "All here, Sir.",
                "All right, get in your groups. Let's move out.",
                "All right, I'm putting my pistols down.",
                "All right, let's go!",
                "All right, listen up. We'll assemble the squads into two divisions. We'll move straight up this gorge towards the airbase at the far side.",
                "All right, there it is. The Cyber Center.",
                "All right. Our first goal is to get inside the hangar. Kalani is a war machine, programmed to kill, and he's got the numbers and the firepower to do it. Our only chance is to be aggressive, surprise him, hopefully put him on the defensive.",
                "Already called in Evac. Kix will stay with Cody until it arrives. I'm in charge now. And I've got a plan to get into that Cyber Center.",
                "Ambush!",
                "And all you had to do to get it was put the rest of us all at risk.",
                "And we needed to hear it.",
                "And you are a natural.",
                "Any ideas?",
                "Anyone else in the mess able to confirm what you two are saying?",
                "Anyone with you?",
                "Are you sure about this?",
                "As sure as it is my duty to remain loyal to your command, I also have another duty, to protect those men.",
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
                "Can't advance with those destroyers up there. There's no way to breach their shields.",
                "Can't we destroy the supply ship?",
                "Cody!",
                "Come on, Wolffe. It's just a scratch. Keep moving forward, soldier! Onward!",
                "Come out and fight, you cowards!",
                "Commander. You got old.",
                "Company, attention!",
                "Congratulations. You're not shinies anymore.",
                "Copy that.",
                "CT-1409... that was Echo's number. He's alive.",
                "Despite Hardcase's flying, you two saved us all.",
                "Do I have a choice?",
                "Do you have any idea what you've done?",
                "Doesn't matter, kid. We have to retake this base, so we will retake this base.",
                "Don't do a job till you've guaranteed the best odds, right?",
                "Don't worry about a thing, Sir. We'll have this city under Republic control by the time you're back.",
                "Don't worry, kid. I'll take good care of him.",
                "Easy. Our turbolasers are no match for that Star Destroyer.",
                "Echo, it's Rex. I'm here.",
                "Echo. Tech. We got to get him out of here. Figure out how to unplug him from-- from this mess.",
                "Either way, he's in charge and we've got a job to do. Just treat him with respect and we'll all get along fine.",
                "Everyone regroup, now! Take cover!",
                "Everyone, find cover. We'll hold this position and let them come to us.",
                "Everyone, keep moving. Hey, Kix, leave it.",
                "Everyone, stop firing! We're shooting at our own men! They're not Umbarans, they're clones! Take off your helmets. Show them you're not the enemy.",
                "Everyone, take cover!",
                "Exactly. The counterattacks are so specific, it's my strategy the droids know, my playbook.",
                "Excuse me, Sir?",
                "Excuse the interruption, Sir, but it is time to depart.",
                "Explain your actions.",
                "Ezra, form it up! We've gotta stay together.",
                "Ezra, incoming!",
                "Fine. Fine, we'll do it.",
                "Fives! Fives! Fives...",
                "Fives, Echo, and before that, Hevy. There's so many troopers gone.",
                "Fives, no!",
                "Fives, we are listening to you. We only want to help.",
                "Fives... It would help if you'd ease their minds.",
                "Fives? Are you clear?",
                "Forget it. We have to leave them.",
                "Four? Well, how are they not falling over?",
                "Full stop, Wolffe! Dig in! This is where we finish the battle!",
                "General Fisto is expecting us.",
                "General Kenobi's battalion, Sir?",
                "General Krell, the top of this ridge will make a good place for the men to make camp.",
                "General Krell, we've come up with a plan to infiltrate the airbase.",
                "General Skywalker, explosives are in place, Sir. Objective completed.",
                "General Skywalker, where are you?",
                "General, the Defender is contacting us. There seems to be a problem.",
                "General, we have taken the base and cut off enemy supply lines to the capital.",
                "General, we've been over the same area a dozen times. There's no sign of Commander Tano.",
                "Generals, they had all our intel.",
                "Get ready! Here they come!",
                "Get those rocket launchers down there. Move it, troopers!",
                "Glad to have you back, Commander.",
                "Go! Go, go, go! Keep moving! Keep moving!",
                "Good job. All right, let's move out.",
                "Great shot, kid. Now get yourself moving. This is your only chance.",
                "Great. Now what?",
                "Gregor, I've lost power to the main cannon!",
                "Gregor, you've still got it.",
                "Hands above your heads. Take your sun bonnets off.",
                "Hang in there, Cody.",
                "He gave us a bit of a chase, Sir.",
                "He knew where we'd look. He's not trying to escape.",
                "He wants to get around the lockdown. He's blinded us by taking out the power. He could disable the entire security system.",
                "Here they are, the coordinates of every Republic base, Separatist installation, pirate hideout and smuggler's den in the Outer Rim.",
                "Here's the algorithm. You're looking for a program using this sequence.",
                "He's just trying to keep us on schedule.",
                "He's running. Hit that line! Bring him up.",
                "He's wound tight, but he's loyal.",
                "Hevy, get out of there.",
                "Hey! Stop!",
                "Hey, careful, kid. The droids used to protect their armories with ray shields.",
                "Hey, hey. Easy with those, son.",
                "Hey, I bet you know a thing or two about mechanics.",
                "Hey, kid.",
                "Hit it! Now!",
                "Hold on! Hold on! Don't just run out there.",
                "Hold up. Let me recon first.",
                "Hold your ground!",
                "How do we get this guy?",
                "How is Commander Tano?",
                "How many legs they got?",
                "Huh. Just like me.",
                "Huh. Reinforced armor plating, heavy cannons and antipersonnel blasters. Mmm.",
                "I admit that the idea that Echo is still alive is a long shot.",
                "I always trust my general.",
                "I am leaving here with my friend.",
                "I am not training him.",
                "I am.",
                "I bet you sold out your brothers for some real shiny coin, huh?",
                "I can't get to the control panel. You've got to extend the bridges.",
                "I didn't betray my Jedi. Wolffe, Gregor and I all removed our control chips. We all have a choice.",
                "I didn't tell the Generals. They might think I'm crazy. In fact, you might think I'm crazy.",
                "I do love a battle, but on my terms. Gregor, drop the joopa. We gotta get moving.",
                "I don't believe it.",
                "I don't know. I'll get back to you on that if we survive this battle.",
                "I don't know. Thousands. Probably tens of thousands. Never kept count like some of the boys.",
                "I fought by her side from the Battle of Christophsis to the Siege of Mandalore, and a friend of hers is a friend of mine.",
                "I guess we are.",
                "I had no choice. You hear me?",
                "I have a bad feeling about this.",
                "I have a few ideas.",
                "I have no idea.",
                "I haven't heard those digits in... Well, that's my birth number.",
                "I honor my code. That's what I believe.",
                "I hope you're right. But the fact is, Echo's fingerprints are all over these Separatist strategies.",
                "I know Commander Tano. She would never do something like this.",
                "I know you work with Cody sometimes, but who do you guys report to?",
                "I know. But they weren't the ones that betrayed us. Remember? Wolffe, remember?",
                "I raised my objection to General Krell's plan, but he didn't agree. So this is it.",
                "I think Echo's alive.",
                "I think freedom's gonna have to wait, kid.",
                "I... I don't understand. You said it was coming from this city.",
                "If it's a fight you want, I hope you brought a better class of soldier than those Stormtroopers.",
                "If they follow procedure, they'll fly search patterns based on our last confirmed position.",
                "If you're certain he'll approve the mission, why wait? Let's get going.",
                "I-I know, I know. Don't worry.",
                "I'll get 'em. But you might consider staying for dinner.",
                "I'm always first, kid.",
                "I'm glad you're still alive.",
                "I'm going for the legs, all four of them.",
                "I'm good. Generation one armor always holds up.",
                "I'm no Jedi.",
                "I'm on it. He must have gone in the mess hall.",
                "I'm part of the most pivotal moment in the history of the Republic. If we fall then our children and their children could be forced to live under an evil I can't well imagine.",
                "In my book, experience outranks everything.",
                "In my experience, when it comes to Jedi, the worse the plan, the better the result.",
                "Incoming!",
                "Is everyone clear on the plan? Hardcase?",
                "Is everyone in position?",
                "Is that true, Sir?",
                "It can't be.",
                "It just doesn't make sense. Blast!",
                "It wasn't all luck, Sir. A lot of men died to take this base.",
                "It worked!",
                "It's Captain, Sir.",
                "It's gaining altitude over the sea.",
                "It's okay, Echo. You're safe now. Just sit tight, trooper. You're going home.",
                "It's really bad. That droid's extremely intelligent.",
                "It's Slick? Slick's the traitor?",
                "It's the pattern. The band's only coming off one terminal in the whole base. Check it out.",
                "It's too hard to tell.",
                "I've assembled a list of potential bases and clearance codes and a few protocols the Imperials still use. Should be of some use.",
                "I've called in an air strike on the enemy positions.",
                "I've dispatched two men on a stealth incursion into the airbase. They've been ordered to co-opt starfighters and use them against the tanks.",
                "I've watched so many of my brothers fall during this war: Fives, Echo, Hevy.",
                "Jesse, take the right flank. Dogma, take the left flank.",
                "Just like the old days.",
                "Just make it fast. Those droids are getting close.",
                "Kanan's right. We need one Jedi up there manning the cannon and another Jedi down here to lead us out of this mess. You are the only ones who can see in this storm. Sabine, spot him. Hey, kid. You might need that. And hang on tight.",
                "Keep an eye on this regulator. The line can overheat and shut down. No line, no joopa, no Zeb.",
                "Keep movin'. We've got to claim that ridge. The other battalions are counting on us!",
                "Keep the wounded as quiet as possible. All right, you heard the General. Let's go.",
                "Krell may do things differently, but he is effective at getting them done. He's a recognized war hero.",
                "Lead shuttle, bank right! Bank right!",
                "Let's go.",
                "Let's move.",
                "Liar!",
                "Listen up. We have to move out.",
                "Listen, those droids wiped out a lot of Republic troopers. Many of them were my friends.",
                "Look sharp, rookies. As long as those tweezers occupy this post, our home planet of Kamino is at risk.",
                "Look, every mission could be a trap. This one is no different. I'm telling you that signal is being sent by Echo himself! He's alive!",
                "Look, Kix, it's more important to save yourself right now. If we survive, you can patch up the wounded later.",
                "Looks like we got ourselves a batch of shinies, Commander.",
                "Looks like we live to fight another day.",
                "May the force be with you.",
                "Maybe this tactical droid will tell us how they knew our plan.",
                "Maybe, back in the day.",
                "Mines! Nobody move!",
                "More droids! We gotta take cover.",
                "Move it, trooper.",
                "My cover's blown, it's time to go, but I decide not to use a ship because it's too obvious.",
                "Next time, just tell me to jump.",
                "Nice to have you on board.",
                "No ray shields this time.",
                "No, Fives, come on stay with me. Stay with me, Fives! Fives!",
                "No, General.",
                "No, it was a victory. We all just won the Clone War, and you ended it, Ezra. A galaxy of senators couldn't do that. An army of Jedi, clones and droids couldn't find the middle ground but you did.",
                "No, it won't, because we're not fighting.",
                "No. A good soldier follows orders. That plan was based on timing and execution. And you took too long!",
                "Not to kill the moment, but the empire won't just let us keep this planet. We need to prepare for how we're going to fight back.",
                "Nothing, Sir. I was just waiting for the General... uh, General.",
                "Now!",
                "Of course we did. We're getting to the bottom of this. Now.",
                "Of course we knew. You think we wouldn't have a plan?",
                "Oh, I'm proud of my service. But I really hate this armor.",
                "Oh, no. The war, it's not over!",
                "Oh, we can take care of ourselves.",
                "Oh, yeah. Little piece of one anyway. This place used to be crawling with 'em. We called 'em clankers.",
                "Ok, listen up. We're going to form two divisions, march down, and head through the gorge to the airbase.",
                "Okay. Let's gear up and move out.",
                "On this part of Skako, there's a race of locals, the Poletecs. All we know is they're very primitive.",
                "One of us? Great, but which one?",
                "Or does he? Yeah, I see what you're getting at.",
                "Our problems are multiplying. We could use some help down here.",
                "Paint job's a little crude, but we think it gets the idea across.",
                "Patch me in. I want to hear it.",
                "Probably easier than going around.",
                "Put him down!",
                "R2, come over here and plug in.",
                "R2's found something.",
                "Ready, Zeb?",
                "Relax. Just as I thought. Looks like one of those new commando droids.",
                "Remember what you were saying about finding another way to destroy those tanks?",
                "Right, Commander. Uh, Kanan.",
                "Scrap 'em!",
                "She distracted him while we completed the mission. It was on her orders, Sir. The droid was with her.",
                "She engaged General Grievous.",
                "Sir, I have watched so many of my brothers fall during this war, and I try not to hang on to any one of them. But that changed when I heard that Separatist transmission. It was no algorithm. That was Echo's voice. I know it.",
                "Sir, I thought you said you'd never have a Padawan.",
                "Sir, if I may address your accusation; I followed your orders, even in the face of a plan that was in my opinion severely flawed, a plan that cost us men, not clones! Men!",
                "Sir, looks like the General's up to something.",
                "Sir, the rocket launchers don't work on these tanks, and it'll be easier to slip by undetected while the rest of us keep the tanks occupied.",
                "Sir, the terrain is extremely hostile. Despite the difficulty of the conditions, the battalion is making good time. These men just need a little break.",
                "Sir, we can't possibly...",
                "Sir, we're overpowered. We need reinforcements.",
                "Sir, we're ready to bring our forward platoons in for a surgical strike on the city's defenses.",
                "Sir, yes, Sir.",
                "Slick pretty much scorched the whole thing. That seemed to be what he was going for all along.",
                "Slick's not gonna like that.",
                "So much for stealth.",
                "So why haven't I heard of this squad?",
                "So, Chopper, old boy, what's your alibi?",
                "So, what squad are we taking in?",
                "Something's not right here.",
                "Sorry for wasting your time.",
                "Sorry, son. My days as a soldier are over.",
                "South tower? We're in the North.",
                "Stand down, troopers. Now! That's an order, soldier.",
                "Stand fast. Hit 'em with everything you've got.",
                "Sure thing, Commander.",
                "Surrender, General, you're outnumbered.",
                "Surrender, General.",
                "Take them off! Now!",
                "Take us to the Sergeant in command.",
                "Tech, find out who's sending that signal. Ask who that is.",
                "Tech, open this door.",
                "Tell him we apologize for what's happened. But tell him the enemy is holding one of our men prisoner in Purkoll. As soon as we rescue him, we'll leave his planet, for good.",
                "Thank you, General.",
                "That one's still got some juice in it. Waste it!",
                "That scattered 'em! Everybody, forward!",
                "That was close.",
                "That was some show you put on just now.",
                "That will never happen. You're a traitor, General, and you will be dealt with as one.",
                "That's all we might get, one shot.",
                "That's just great.",
                "That's not your choice to make. You swore an oath to the Republic. You have a duty.",
                "That's right. Your armor, it's shiny and new, just like you.",
                "That's the price of war, Fives. We're soldiers. We have a duty to follow orders, and, if we must, lay down our lives for victory.",
                "That's what I'm worried about.",
                "That's why we always won.",
                "The droid army uses analytics to predict our strategy. The first time we use a tactic, it's very effective. The next, less so. In fact the more we use a certain tactic, the less effective it becomes. They learn our tendencies and use that data against us. To counter them, we're constantly working out ways to vary our attack.",
                "The droids usually keep coming, wave after wave.",
                "The General just gave you an order, soldier.",
                "The General's giving you an order, Dogma.",
                "The General's right.",
                "The generators just went offline.",
                "The gorge is narrow, Sir. We'll only be able to move our platoons in single squads. Perhaps a closer recon will tell us if there's a more secure route.",
                "The guard got his messages out there somehow. We just got to find them.",
                "The name's Rex, but you can call me Captain or Sir.",
                "The only people in here are brothers.",
                "The traverse controls are over here on the left, and the elevation's on the right.",
                "The Umbarans are advancing!",
                "The Umbarans must've regrouped for a counterattack. Everyone, we must hold this position!",
                "The war is over. We are free men. We can't live under the fear of the Empire for the rest of our lives, Wolffe. That's not freedom.",
                "The way I figure it, you tell the truth, you got nothing to be nervous about.",
                "The way the droids are countering us here, the strategies I'm using, they're all old battle plans Echo and I drew up together.",
                "Then I'll deal with it.",
                "Then we'll destroy the outpost instead.",
                "There is no algorithm. We know you're holding a prisoner of war here.",
                "There was a battle here during the Clone War. This old transport's the perfect place to find weapons, ammo, maybe even some proton bombs.",
                "There's a base there, all right. And it's heavily guarded. At least three tank divisions, plus guns.",
                "There's a lot of surface fire.",
                "There's an opening to our South. I recommend we move all platoons off the ridge in case the air strike overshoots.",
                "There's going to be a lot of fire. We'll have to be quick.",
                "There's hope for you yet, rookie.",
                "There's no escape now, you piece of rankweed! Move. Move!",
                "These boys are sloppy. There should always be an officer on duty.",
                "They'll be coming around any second. Bring up the launchers. Spread detonators along that corridor. Trap them into the bottleneck. We're going to blow those things sky high. Hurry up. We only have a few seconds.",
                "They're falling back!",
                "Things like what?",
                "Think so? Wolffe, evasive maneuvers.",
                "This detonator isn't working.",
                "This is a ray shield.",
                "This is not a game! This is life and death! Every move you make affects the rest of us. If we're gonna survive this, we're going to do it with strategy and discipline.",
                "This isn't the time for a debate. Right now we have to stay alert.",
                "This map has a ridge at 23 degrees North, Northwest.",
                "Too late. The walkers are already on top of us.",
                "Try shortening your leads. You're wasting ammo.",
                "Tup, can you hear me?",
                "Ugh.",
                "Ugh. Where... Where are we?",
                "Uh, General. Kanan.",
                "Uh, it's hard to tell.",
                "Uh, what thing?",
                "Understood, Sir.",
                "Wait, what? You're mistaken. We would never do that.",
                "Wait. That's not normally how it goes.",
                "Waiting for orders, Sir.",
                "Wat Tambor.",
                "Watch your left!",
                "Wavelength interference, weak frequencies, spotty, irregular. See, how it shows up every few days then disappears? Day to day, you wouldn't notice it.",
                "We clones were bred for combat. With few exceptions, there was no other way of life for us.",
                "We did it, Gregor. We did it.",
                "We don't have any cover! We need to pull back. Get them to follow us. If we can draw them out, we can see them. If we can see them, we can hit them. All squads, pull back now!",
                "We don't have the firepower to take on walkers.",
                "We don't have time for that, Sir.",
                "We don't have time. Look.",
                "We have got to move before those fighters come back.",
                "We have no assault craft, Sir, only a couple of fighters and the Twilight.",
                "We have to do something. I'll get him.",
                "We have to stop him.",
                "We have to warn command.",
                "We have to warn the Republic about the invasion. They'll take notice when the all-clear signal stops.",
                "We need to fall back, get them to follow us. If we can draw them out, we can see them. If we can see them, we can hit them! All squads, fall back now!",
                "We need to hold out as long as we can. I'm trusting Fives and Hardcase to pull this thing off.",
                "We roll the proton bombs. We roll them across the hangar into the feet of the walkers, and then we blast them.",
                "We should move out before reinforcements arrive. Our position has been compromised.",
                "We'll need every thermal detonator in the inventory.",
                "Well, General, he's, um...",
                "Well, I have a mission for you.",
                "Well, I told you!",
                "Well, if you think that was bad, let me tell you about the battle of Geonosis.",
                "Well, if you've got a better idea, Sir, now's the time.",
                "Well, it doesn't matter how it ended. The war is over. Let us go.",
                "Well, just in case it wasn't us, I mean...",
                "Well, look at that. We hit pay dirt. The munitions depot is fully loaded. More proton bombs in there than we can carry.",
                "Well, looks like we win.",
                "Well, that sure complicates things, Commander.",
                "Well, that's too bad, 'cause there's nobody out here.",
                "Well, the Empire certainly isn't the Republic, but you can't do anything about that.",
                "Well, the Empire is here because of us. We'll deal with the consequences.",
                "Well, this takes trust to a whole new level.",
                "Well...",
                "We're cut off. Off the platform!",
                "We're going in, but remember what the General said. \"No casualties, disarm only.\"",
                "We're soldiers, Ezra. This is what we were born to do.",
                "We've been outflanked.",
                "We've got a problem. Fall back! Fall back now!",
                "We've gotta move swiftly.",
                "We've gotta scatter them. Use the sword and shield maneuver.",
                "We've still got some fight left in us, Tup, and I've got an idea.",
                "What are you doing?",
                "What are you gonna do?",
                "What did you just say?",
                "What do you want us to do, surrender?",
                "What do you want?",
                "What happened out there?",
                "What have they done to you?",
                "What is it?",
                "What the...",
                "What took you so long, Wrecker?",
                "What? How can that be? There's no \"atmospheric disturbances\" up here.",
                "What? I never got any messages from Commander Tano.",
                "Where is that kid? We're gonna get trapped here. We need to move. Now.",
                "Where were you before you went to the mess, Chopper?",
                "Who's the youngling?",
                "Why General? Why kill your own men?",
                "With all due respect, General, we don't know what we're up against. It might be wiser to think first.",
                "With all due respect, Senator, it's what these men were born to do.",
                "Wolffe, bring us around.",
                "Wolffe, set vector two-niner-zero.",
                "Wolffe, turn 180.",
                "Wolffe, what did you do?",
                "Yeah, it means a lot to his programming. It means a lot to mine as well.",
                "Yeah, well you should have used kill.",
                "Yeah. Like I said, our war's over, kid. Don't much care to get mixed up in another. Oh, and say hello to Commander Tano for me.",
                "Yeah. Make sure you keep an eye on those incoming Separatist forces. I want to know when they reach this outpost.",
                "Yeah. Ready your weapons.",
                "Yep. And the Empire's on its way.",
                "Yes, General.",
                "Yes, Sir, Commander.",
                "Yes, Sir.",
                "Yes, Sir. On some beat-up old space freighter. I’ll be surprised if he even makes it to Tatooine in that junker.",
                "Yes. Yes, I did.",
                "You betrayed everyone of us.",
                "You don't have a choice. That's an order.",
                "You got any ideas? Then this is it.",
                "You have to start somewhere.",
                "You know what's funny, traitor? We knew you'd never take a chance on the exits while they were blocked.",
                "You know, I can't figure those villagers not wanting to fight. No pride I guess.",
                "You know, I could've ordered you to take me along.",
                "You know, I don't think he likes me. Or ever will. Can't say I blame him. The war left its scars on all of us.",
                "You know, I've outserved my purpose for that kind of fighting, I'm afraid. After the war, I questioned the point of the whole thing. All those men died, and for what?",
                "You put us right in the middle of 'em to get us a shot. It's crazy, but it's probably our best chance.",
                "You shot us!",
                "You showed me something today. You're exactly the kind of men I need in the 501st.",
                "You want the shot?",
                "You were brave today, kid. You jumped right in there to help. A great Jedi once told me that the best leaders lead by example. You do that well.",
                "You'll be a whole lot smaller when I'm through with you.",
                "Your reputation precedes you, General. It is an honor to be serving you.",
                "You're a Separatist.",
                "You're an agent of Dooku.",
                "You're gonna need all of 'em.",
                "You're not abandoning anyone. We're covering your escape. Now, move.",
                "Artoo, how much longer are we stuck down here?",
                "Great. I'll tell the boys.",
                "All right, men. Hang in there a little longer.",
                "The paint job is a little crude, but we think it gets the idea across.",
                "Yes, Sir. Men, with me.",
                "Put him through.",
                "Sorry. I didn't think to bring you a jetpack.",
                "Some things never change.",
                "No sign of Maul yet.",
                "If he's here. If he's not, then all of this plays right into his hands.",
                "It's her. Come on! We'll trace the signal.",
                "Sorry to interrupt, but there's been an attack.",
                "Yes, Ma'am.",
                "This is Commander Rex. We have a man down...",
                "You asked for our help. My men don't want to be acting as a police force.",
                "Are you all right, Jesse?",
                "Come on. Get the gunships.",
                "We'll take it from here, Commander!",
                "Commander, I have the council waiting.",
                "He was at the meeting when I left to get you.",
                "You didn't tell them what Maul said about General Skywalker.",
                "Something on your mind?",
                "Well, I've known no other way. Gives us clones all a mixed feeling about the war. Many people wish it never happened. But without it, we clones wouldn't exist.",
                "Want to have a look? It might have an update on General Kenobi's efforts.",
                "Yes, Lord Sidious.",
                "No! I'll do it.",
                "Stay back! Find him. Find him. Fives. Find him! Fives!",
                "Fine, just tired is all. I want you to go to the detention level. Execute Maul.",
                "All right. We know Ahsoka Tano is on board. She's been marked for termination by Order 66. Under this directive, any and all Jedi leadership must be executed for treason against the Republic. Any soldier that does not comply with the order will also be executed for treason. Understood?",
                "There are only so many places to hide on this ship. Gather up search parties. We'll fan out and move section by section. Come on. Let's get moving.",
                "Destroy the escape pods and increase security on the hanger decks.",
                "I already know this report is going to fall on deaf ears, but I owe it to Fives to record what I saw. I'm not sure I believe it myself, but there's a possibility that the inhibitor chips the Kaminoans put inside of us have a purpose that we don't yet fully understand.",
                "Trooper, did you seal the doors? Trooper? If you read this, seal the door. We want to redirect his approach. Over.",
                "Hey, hey. Out of the way!",
                "Are you cross-wired?",
                "Where is she?",
                "I am one with the Force and the Force is with me. I am one with the Force and the Force is with me.",
                "Yeah. Yeah, kid. I'm okay. I'm sorry for what happened earlier. I almost killed you.",
                "Ahsoka, it's all of us. The entire Grand Army of the Republic has been ordered to hunt down and destroy the Jedi Knights.",
                "How are we going to get out of here?",
                "Which is?",
                "Yeah, well, tell that to them.",
                "They're almost through.",
                "All right. Now what?",
                "The boys are having a rough time of it. Did you hear Maul also escaped?",
                "What? Why?",
                "That's one word for it.",
                "Oh, I don't like the sound of that.",
                "The hangar bay doors are sealed. They've got everything locked down. If they weren't trying to kill us, I'd be proud.",
                "What do you mean the Hyperdrive is offline?",
                "Destroyed? It's completely gone?",
                "We're caught in that moon's gravitational field.",
                "Any luck with those doors?",
                "They were waiting for us.",
                "So what do we do? Fight our way to the shuttle?",
                "I hate to tell you this, but they don't care. This ship is going down, and those soldiers, my brothers, are willing to die and take you and me along with them.",
                "So, we're just going to surrender? Admit defeat? Is that it?",
                "Well, I don't see any other option.",
                "Hold your fire!",
                "I said hold your fire, Jesse. I have the situation under control.",
                "The order was to execute the Jedi for treason against the Republic. The problem is, Ahsoka Tano is no longer a Jedi. Hasn't been for some time.",
                "Jesse. Jesse, listen to me. We've known each other a long time. If we don't get this right, we will be the ones committing treason, not her.",
                "Yeah, I didn't much like being a commander anyway.",
                "Incoming.",
                "Can't hold them off. Too many of them.",
                "Nothing. Nothing. Nothing. Everything down here is under maintenance.",
                "Get me over there.",
                "It's been a while, boys.",
                "Hmm. That's a long story.",
                "And when the war ended, I guess you could say I've been... keeping a low profile.",
                "Being dead in the Empire's eyes has its advantages.",
                "Trace and Rafa Martez. They said a squad of rogue clones helped them on Corellia. They told me I could find you here. And that you were traveling with a kid. Who is she?",
                "Yeah. Good to see you too, Wrecker.",
                "I've met many clones in my time, but never one like you.",
                "Now how'd you know that?",
                "Yeah, I guess I've been around.",
                "What's wrong?",
                "Is that so?",
                "You're telling me you haven't removed your chips?",
                "Those chips make you a threat to everyone around you. Even her. You're all ticking time bombs.",
                "What's in your head is more dangerous than you can imagine. I've seen what happens when the chip activates, and I don't want to bury any more of our brothers. Trust me. It is not something you can control. I couldn't. It's a risk you do not want to take.",
                "Good question. I'll be in touch.",
                "Right on time.",
                "Follow me.",
                "Bracca may not be much to look at, but it has exactly what we need. I had my inhibitor chip taken out on a Jedi cruiser just like that. That's where we're heading.",
                "That's why.",
                "They control this entire planet. We need to keep out of sight from their patrols. Let's move.",
                "Fives tried to warn me about the chips, but I didn't understand at the time.",
                "How did you boys find out about them?",
                "The kid?",
                "This is an original Venator-class ship from the first batch off the line.",
                "The last time I was aboard one of these it didn't end so well.",
                "I had help.",
                "The medical bay's at the other end. Wrecker, grab that cable.",
                "Nicely done.",
                "Wrecker!",
                "Are you all right?",
                "Makes you miss battling clankers, doesn't it?",
                "This will do nicely.",
                "Do you prefer to use the facility on Kamino?",
                "It's more dangerous to leave their inhibitor chips in.",
                "We need to speed this up.",
                "You boys got lucky. Very few clones were immune to the effects of Order 66. It's rare.",
                "I'm not sure. I've never been on this end of it.",
                "This could be a while. Why don't you take Omega topside and get some air?",
                "One chip down. Three to go. Who's next?",
                "Copy that. I'll meet you at the rendezvous by the next rotation.",
                "I spent my life defending the Republic. Can't stop now.",
                "Ah, not all of it. We're here. Others are out there too. Your squad's skills would be a tremendous asset.",
                "So I've noticed.",
                "Which is what?",
                "I guess we all are. When you sort things out, let me know where you land. Tell the boys I'll see them around.",
                "Funny. I was gonna say the same to you.",
                "Take care, trooper.",
                "Hello, boys. Sorry to cut right to it, but I could use your help.",
                "I received a distress signal from a clone trooper, but I'm a bit tied up at the moment to retrieve him.",
                "He's an old friend, and he's in trouble. I need you to get him out.",
                "Can't talk right now. Sending you his signal. I'll be in touch.",
                "It's our privilege, Commander.",
                "Yeah, Jesse really tagged you.",
                "I don't know how much more of this she can take.",
                "Not bad. It took us five minutes to knock you out that time.",
                "Let's hope all that training pays off."
            };
        }
    }
}
