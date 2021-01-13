using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Poker
{
    public class Deck
    {
        public List<Card> Cards;
        public Card Reverse;

        public Deck(bool addJokers, Texture2D[] textures)
        {
            var cardAmount = addJokers ? 55 : 53;
            Cards = GetCards(cardAmount, textures);
        }

        /// <summary>
        ///  Creates the deck of cards from the loaded textures.
        /// </summary>
        /// <param name="amount">Amount of cards</param>
        /// <param name="textures">The card textures</param>
        /// <returns>A list of cards</returns>
        private List<Card> GetCards(int amount, Texture2D[] textures) {
            List<Card> cards = new List<Card>();
            const int rows = 4;
            const int cols = 14;
            const int cardsWithoutJoker = 53; // Includes 1 reverse

            for (var i = 0; i < rows; i++)
            {
                for (int j = (i != 1) ? 0 : 1; j < cols; j++)
                {
                    if (j.Equals(0)) // First col (reverse, reverse, joker, joker)
                    {
                        if (i == 0)
                        {
                            Reverse = new Card(Category.Reverse, Type.None, textures[i * cols + j]);
                        }

                        if (amount > cardsWithoutJoker && i > 1)
                        {
                            cards.Add(new Card(Category.Joker, Type.None, textures[i * cols + j]));
                        }
                    }
                    else
                    {
                        cards.Add(new Card((Category) i, (Type) j + 1, textures[i * cols + j]));
                    }
                }
            }

            return cards;
        }

        public Deck Shuffle()
        {
            Cards = Cards.OrderBy(a => Guid.NewGuid()).ToList();
            return this;
        }
    }
}