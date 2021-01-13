using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Poker
{

    public class ButtonElement : UIElement
    {
        public Vector2 TextPosition;
        public ButtonElement(Texture2D texture, Vector2 position, string text, SpriteFont font, float scale = 1.0f)
        {
            Texture = texture;
            Position = position;
            Scale = scale;
            Text = text;
            Font = font;
            Container = CreateContainer();
            TextPosition = CalculateTextPosition();
        }
        /// <summary>
        /// Calculates the position of the text inside the button.
        /// </summary>
        /// <returns>The position of the text inside the button.</returns>
        public Vector2 CalculateTextPosition()
        {
            return new Vector2(
                Position.X + (Texture.Width * Scale / 2 - Font.MeasureString(Text).X / 2),
                Position.Y + (Texture.Height * Scale / 2 - (float) Font.LineSpacing / 2));
        }
    }
    
    public class CardElement : UIElement
    {
        public Card Card;
        public CardElement(Card card, Vector2 position, float scale = 1.0f)
        {
            Card = card;
            Position = position;
            Scale = scale;
            Container = CreateContainer();
        }
        private new Rectangle CreateContainer()
        {
            return new Rectangle((int) Position.X, (int) Position.Y, 
                (int) (Card.Texture.Width * Scale), (int) (Card.Texture.Height * Scale));
        }
    }
    
    public class ChipElement : UIElement
    {
        public Chip Chip;
        public ChipElement(Chip chip, Vector2 position, float scale = 1.0f)
        {
            Chip = chip;
            Position = position;
            Scale = scale;
            Container = CreateContainer();
        }
        
        private new Rectangle CreateContainer()
        {
            return new Rectangle((int) Position.X, (int) Position.Y, 
                (int) (Chip.Texture.Width * Scale), (int) (Chip.Texture.Height * Scale));
        }
    }
    
    public class InfoElement : UIElement
    {
        public InfoElement(string text, Vector2 position, SpriteFont font, float scale = 1.0f)
        {
            Text = text;
            Position = position;
            Font = font;
            Scale = scale;
            Container = CreateContainer();
        }
    }
    
    public class UIElement
    {
        protected Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Rectangle Container { get; set; }
        protected float Scale { get; set; }
        protected SpriteFont Font { get; set; }
        public string Text { get; set; }

        /// <summary>
        /// Creates a container for the element based on its Texture, Position and Scale.
        /// </summary>
        /// <returns>The element's container (Rectangle)</returns>
        protected Rectangle CreateContainer()
        {
            return new Rectangle((int) Position.X, (int) Position.Y, 
                (int) (Texture.Width * Scale), (int) (Texture.Height * Scale));
        }
    }
}
