using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Poker
{

    [Serializable]
    public class Table
    {
        public List<Card> Cards;
        public int Pot;
        public int RealCardsAmount;

        [JsonConstructor]
        public Table(int pot = 0, List<Card> cards = null)
        {
            Pot = pot;
            Cards = cards ?? new List<Card>();
            RealCardsAmount = 0;
        }
    }
}