using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Poker
{
    public class Drawer
    {
        public SpriteBatch SpriteBatch;
        private GameUI _gameUI;

        public Drawer(GameUI gameUI, SpriteBatch spriteBatch)
        {
            _gameUI = gameUI;
            SpriteBatch = spriteBatch;
        }

        public void DrawBackground()
        {
            SpriteBatch.Begin();
            SpriteBatch.Draw(_gameUI.ContentLoader.BackgroundImage, new Rectangle(0, 0, 800, 480), Color.White);
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
                new Vector2(_gameUI.ScaleX, _gameUI.ScaleY),
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
                new Vector2(_gameUI.ScaleX * GameUI.ChipScale, _gameUI.ScaleY * GameUI.ChipScale),
                SpriteEffects.None,
                0f
            );
        }

        public void DrawButtonElement(ButtonElement element)
        {
            SpriteBatch.Draw(
                _gameUI.ContentLoader.ButtonTexture,
                element.Position,
                null,
                Color.White,
                0f,
                new Vector2(0, 0),
                new Vector2(GameUI.ButtonScale, GameUI.ButtonScale),
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