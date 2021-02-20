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
        public bool IsChangedByServer { get; set; }
        
        public bool IsChangedByClient { get; set; }
        public bool HasFolded { get; set; }
        public string Key { get; set; }
        public bool HasLostRound { get; set; }
        public bool HasLostGame { get; set; }

        public User(string key, string name = "User")
        {
            Key = key;
            Name = name.ToUpper();
            Init();
        }

        public User(string key, string name = "User", int cash = 1000)
        {
            Key = key;
            Name = name.ToUpper();
            if (cash < 25)
            {
                Logger.Error("Default user cash must be at least $25, defaulting to $25");
                cash = 25;
            }
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
            Key ??= "";
            Bets = new List<int>();
            HasAddedBet = false;
            HasFolded = false;
            HasLostRound = false;
            HasLostGame = false;
            IsChangedByServer = true;
            HasBetted = false;
            Cards = new List<Card>();
            BetIsProcessed = new List<bool>();
        }
    }
}