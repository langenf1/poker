using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Poker
{
    [Serializable]
    public class Card
    {
        public Category CardCategory { get; set; }
        public Type CardType { get; set; }

        [JsonIgnore] [NonSerialized] public Texture2D Texture;

        public Card(Category category, Type type, Texture2D texture)
        {
            CardCategory = category;
            CardType = type;
            Texture = texture;
        }

        [JsonConstructor]
        public Card()
        {
        }

        private static Color GetColor(Category category)
        {
            Color color = category switch
            {
                Category.Clubs => Color.Black,
                Category.Spades => Color.Black,
                Category.Hearts => Color.Red,
                Category.Diamonds => Color.Red,
                _ => new Color()
            };

            return color;
        }
    }
}