using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Poker
{
    [Serializable]
    public class Card
    {
        public Category CardCategory;
        public Type CardType;

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
            var color = category switch
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