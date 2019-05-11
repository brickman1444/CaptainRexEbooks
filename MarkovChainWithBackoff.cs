using System;
using System.Collections.Generic;
using System.Linq;

using Markov;

namespace CaptainRexEbooks
{
    public class MarkovChainWithBackoff<T>
    where T : IEquatable<T>
    {
        private readonly int maximumOrder;
        private readonly int desiredNumNextStates;
        private readonly List<MarkovChain<T>> chains = new List<MarkovChain<T>>();

        public MarkovChainWithBackoff(int maximumOrder, int desiredNumNextStates)
        {
            this.maximumOrder = maximumOrder;
            this.desiredNumNextStates = desiredNumNextStates;

            for (int order = maximumOrder; order > 0; order--)
            {
                chains.Add(new MarkovChain<T>(order));
            }
        }

        public void Add(IEnumerable<T> items)
        {
            foreach (MarkovChain<T> chain in chains)
            {
                chain.Add(items);
            }
        }

        public IEnumerable<T> Chain(Random rand)
        {
            Queue<T> workingQueue = new Queue<T>();

            while (true)
            {
                foreach (MarkovChain<T> chain in chains)
                {
                    Dictionary<T, int> nextStates = chain.GetNextStates(workingQueue);
                    if (nextStates is null)
                    {
                        if (chain == chains.Last())
                        {
                            yield break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (nextStates.Count >= desiredNumNextStates || chain == chains.Last())
                    {
                        int totalNonTerminalWeight = nextStates.Sum(w => w.Value);

                        int terminalWeight = chain.GetTerminalWeight(workingQueue);
                        int randomValue = rand.Next(totalNonTerminalWeight + terminalWeight) + 1;

                        if (randomValue > totalNonTerminalWeight)
                        {
                            yield break;
                        }

                        int currentWeight = 0;
                        foreach (var nextItem in nextStates)
                        {
                            currentWeight += nextItem.Value;
                            if (currentWeight >= randomValue)
                            {
                                yield return nextItem.Key;
                                workingQueue.Enqueue(nextItem.Key);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}
