using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Poker
{
    public class Chip
    {
        public int Value;
        public Texture2D Texture;
        public Vector2 Position;
        public Rectangle Container;

        public Chip(int value, Texture2D texture)
        {
            Value = value;
            Texture = texture;
        }
    }
}