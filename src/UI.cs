using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Poker
{
    public class UI
    {
        private readonly Resolution _resolution;
        private ContentLoader _contentLoader;

        public List<ButtonElement> ButtonElements;
        public List<ChipElement> ChipElements;
        public List<CardElement> CardElements;
        public List<InfoElement> InfoElements;
        
        public float ScaleX;
        public float ScaleY;
        public float ElementMarginX = 10;
        public float ElementMarginY = 10;

        public float ChipHeight;
        public float ChipWidth;
        public float CumulativeChipHeight;
        public float CardHeight;
        public float CardWidth;
        public float UserCardsWidth;
        
        public const int InitialChipHeight = 162;
        public const int InitialChipWidth = 162;
        public const int InitialCardHeight = 96;
        public const int InitialCardWidth = 72;
        public const float CardScale = 1.0f;
        public const float ChipScale = 0.4f;
        public const float ButtonScale = 0.15f;
        
        public const int UserCardsAmount = 2;
        public const int ChipsAmount = 4;
            
        public UI(Resolution resolution)
        {
            _resolution = resolution;
            Initialize();
        }
        
        /// <summary>
        /// Class initialization, sets variables.
        /// </summary> 
        private void Initialize()
        {
            ScaleX = 1 / _resolution.ScreenScale.X;
            ScaleY = 1 / _resolution.ScreenScale.Y;
            ElementMarginX *= ScaleX;
            ElementMarginY *= ScaleY;
            
            ChipWidth = InitialChipWidth * ChipScale;
            ChipHeight = InitialChipWidth * ChipScale;
            CumulativeChipHeight = ChipsAmount * ChipHeight + (ChipsAmount - 1) * ElementMarginY;

            CardHeight = InitialCardHeight * ScaleY;
            CardWidth = InitialCardWidth * ScaleX;
            UserCardsWidth = UserCardsAmount * CardWidth + (UserCardsAmount - 1) * ElementMarginX;
        }
        
                
        /// <summary>
        /// Creates the Button UI Elements.
        /// </summary>
        public void CreateButtonElements()
        {
            // Clear Bet
            ButtonElements.Add(new ButtonElement(
                _contentLoader.ButtonTexture,
                new Vector2(ElementMarginX,
                    _resolution.VirtualScreen.X / ScaleX - _contentLoader.ButtonTexture.Height * ButtonScale * 2 - ElementMarginY * 2),
                "CLEAR BET",
                _contentLoader.ButtonFont
            ));

            // Bet
            ButtonElements.Add(new ButtonElement(
                _contentLoader.ButtonTexture,
                new Vector2(ButtonElements[0].Position.X + _contentLoader.ButtonTexture.Width * ButtonScale + ElementMarginX, 
                    ButtonElements[0].Position.Y),
                "BET",
                _contentLoader.ButtonFont
            ));

            // Fold
            ButtonElements.Add(new ButtonElement(
                _contentLoader.ButtonTexture,
                new Vector2(ButtonElements[0].Position.X,
                    ButtonElements[0].Position.Y + _contentLoader.ButtonTexture.Height * ButtonScale + ElementMarginY),
                "FOLD",
                _contentLoader.ButtonFont
            ));

            // Call
            ButtonElements.Add(new ButtonElement(
                _contentLoader.ButtonTexture,
                new Vector2(ButtonElements[1].Position.X, 
                    ButtonElements[2].Position.Y),
                "CALL",
                _contentLoader.ButtonFont
            ));
        }
        
        /// <summary>
        /// Creates the Info UI Elements.
        /// </summary>
        public void CreateInfoElements()
        {
            // CASH
            InfoElements.Add(new InfoElement("CASH:", 
                new Vector2(ChipElements[ChipsAmount - 1].Position.X, 
                    ChipElements[ChipsAmount - 1].Position.Y + ChipHeight + ElementMarginY), 
                _contentLoader.DefaultFont
                ));
            
            // BET
            InfoElements.Add(new InfoElement("BET:", 
                new Vector2(ChipElements[ChipsAmount - 1].Position.X,
                    ChipElements[ChipsAmount - 1].Position.Y + ChipHeight + ElementMarginY + _contentLoader.DefaultFont.LineSpacing), 
                _contentLoader.DefaultFont
                ));

            // POT
            InfoElements.Add(new InfoElement("POT:",
                new Vector2(ChipElements[ChipsAmount - 1].Position.X, 
                    ChipElements[ChipsAmount - 1].Position.Y + ChipHeight + ElementMarginY + _contentLoader.DefaultFont.LineSpacing * 2), 
                _contentLoader.DefaultFont
                ));
        }
        
        /// <summary>
        /// Creates the Chip UI Elements with their corresponding Chip instances.
        /// </summary>
        public void CreateChipElements()
        {
            var chipValues = new List<int>() {25, 50, 100, 250};
            // Chip sprite in foreach loop starts at ChipSprites[2] (25)
            foreach (var value in chipValues)
            {
                var currentY = (int) (ElementMarginY + ChipElements.Count * (ChipHeight + ElementMarginY));
                ChipElements.Add(new ChipElement(new Chip(value,
                        _contentLoader.ChipSprites[ChipElements.Count + 2]), 
                    new Vector2(ElementMarginX, currentY), 
                    new Rectangle((int) ElementMarginX, currentY,
                        (int) (ChipWidth + ElementMarginX),
                        (int) (ChipHeight + ElementMarginY)
                    )));
            }
        }

        /// <summary>
        /// Creates the Card UI Elements.
        /// </summary>
        public void CreateCardElements()
        {
            
        }
    }
}