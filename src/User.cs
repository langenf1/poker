using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Poker
{

    [Serializable]
    public class User
    {
        public int Cash { get; set; }
        public List<int> Bets { get; set; }
        public bool HasAddedBet { get; set; }
        public bool HasBetted { get; set; }

        public List<bool> BetIsProcessed { get; set; }
        public string Name { get; set; }
        public List<Card> Cards { get; set; }
        public bool HasChanged { get; set; }
        public bool HasFolded { get; set; }
        public string Key { get; set; }

        public bool HasLost { get; set; }
        public bool HasLostGame { get; set; }

        public User(string key, string name = "User")
        {
            Key = key;
            Name = name.ToUpper();
            Init();
        }

        public User(int cash)
        {
            Cash = cash;
            Init();
        }

        [JsonConstructor]
        public User()
        {
            Init();
        }

        private void Init()
        {
            if (Cash == 0)
            {
                Cash = 1000;
            }

            Key ??= "";
            Bets = new List<int>();
            HasAddedBet = false;
            HasFolded = false;
            HasLost = false;
            HasLostGame = false;
            HasChanged = true;
            HasBetted = false;
            Cards = new List<Card>();
            BetIsProcessed = new List<bool>();
        }
    }
}