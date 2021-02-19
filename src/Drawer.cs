using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Poker
{
    public class Drawer
    {
        public SpriteBatch SpriteBatch;
        private UI _ui;

        public Drawer(UI ui, SpriteBatch spriteBatch)
        {
            _ui = ui;
            SpriteBatch = spriteBatch;
        }

        public void DrawBackground()
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(_ui.ContentLoader.BackgroundImage, new Rectangle(0, 0, 800, 480), Color.White);
            SpriteBatch.End();
        }

        public void DrawCardElement(CardElement element)
        {
            SpriteBatch.Draw(
                element.Card.Texture,
                element.Position,
                null,
                Color.White,
                0f,
                new Vector2(0, 0),
                new Vector2(_ui.ScaleX, _ui.ScaleY),
                SpriteEffects.None,
                0f
            );
        }

        public void DrawChipElement(ChipElement element)
        {
            SpriteBatch.Draw(
                element.Chip.Texture,
                element.Position,
                null,
                Color.White,
                0f,
                new Vector2(0, 0),
                new Vector2(_ui.ScaleX * UI.ChipScale, _ui.ScaleY * UI.ChipScale),
                SpriteEffects.None,
                0f
            );
        }

        public void DrawButtonElement(ButtonElement element)
        {
            SpriteBatch.Draw(
                _ui.ContentLoader.ButtonTexture,
                element.Position,
                null,
                Color.White,
                0f,
                new Vector2(0, 0),
                new Vector2(UI.ButtonScale, UI.ButtonScale),
                SpriteEffects.None,
                0f
            );
            DrawInfoElement(element.TextElement);
        }
        
        public void DrawInfoElement(InfoElement element)
        {
            SpriteBatch.DrawString(element.Font, element.Text, element.Position, Color.White);
        }
    }
}