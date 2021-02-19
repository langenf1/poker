using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Poker
{
    public class UI
    {
        private readonly Resolution _resolution;

        public ContentLoader ContentLoader { get; }

        public List<ButtonElement> ButtonElements;
        public List<ChipElement> ChipElements;
        public List<CardElement> UserCardElements;
        public List<CardElement> OpponentCardElements;
        public List<CardElement> TableCardElements;
        public List<InfoElement> UserInfoElements;
        public List<InfoElement> OpponentInfoElements;

        public float ScaleX;
        public float ScaleY;
        public float ElementMarginX = 10;
        public float ElementMarginY = 10;
        public const float CardScale = 1.0f;
        public const float ChipScale = 0.4f;
        public const float ButtonScale = 0.15f;

        public float ChipHeight;
        public float ChipWidth;
        public float CumulativeChipHeight;
        public float CardHeight;
        public float CardWidth;
        public float UserCardsWidth;
        public float TableCardsWidth;

        public int InitialChipHeight = 162;
        public int InitialChipWidth = 162;
        public int InitialCardHeight = 96;
        public int InitialCardWidth = 72;

        public List<int> ChipValues = new List<int> {5, 10, 25, 50, 100, 250};
        public const int StartAtChipIndex = 2;
        public int ChipsAmount;

        // Changing this will probably break stuff (so don't)
        public const int UserCardsAmount = 2;
        public const int TableCardsAmount = 5;

        public UI(Resolution resolution, ContentLoader contentLoader)
        {
            _resolution = resolution;
            ContentLoader = contentLoader;
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

            InitialChipHeight = (int) (InitialChipHeight * ScaleY);
            InitialChipWidth = (int) (InitialChipWidth * ScaleY);
            ChipWidth = InitialChipWidth * ChipScale;
            ChipHeight = InitialChipWidth * ChipScale;
            ChipsAmount = ChipValues.Count - StartAtChipIndex;
            CumulativeChipHeight = ChipsAmount * ChipHeight + (ChipsAmount - 1) * ElementMarginY;

            InitialCardHeight = (int) (InitialCardHeight * ScaleY);
            InitialCardWidth = (int) (InitialCardWidth * ScaleY);
            CardHeight = InitialCardHeight * ScaleY;
            CardWidth = InitialCardWidth * ScaleX;
            UserCardsWidth = UserCardsAmount * CardWidth + (UserCardsAmount - 1) * ElementMarginX;
            TableCardsWidth = TableCardsAmount * CardWidth + (TableCardsAmount - 1) * ElementMarginX;
        }

        /// <summary>
        /// Creates the Button UI Elements.
        /// </summary>
        public void CreateButtonElements()
        {
            if (ButtonElements != null)
                ButtonElements.Clear();
            else
                ButtonElements = new List<ButtonElement>();

            ButtonElements.Add(new ButtonElement(
                ContentLoader.ButtonTexture,
                new Vector2(ElementMarginX,
                    _resolution.VirtualScreen.X / ScaleX - ContentLoader.ButtonTexture.Height * ButtonScale * 2 -
                    ElementMarginY * 2),
                "CLEAR BET",
                ContentLoader.ButtonFont
            ));

            ButtonElements.Add(new ButtonElement(
                ContentLoader.ButtonTexture,
                new Vector2(
                    ButtonElements[0].Position.X + ContentLoader.ButtonTexture.Width * ButtonScale + ElementMarginX,
                    ButtonElements[0].Position.Y),
                "BET",
                ContentLoader.ButtonFont
            ));

            ButtonElements.Add(new ButtonElement(
                ContentLoader.ButtonTexture,
                new Vector2(ButtonElements[0].Position.X,
                    ButtonElements[0].Position.Y + ContentLoader.ButtonTexture.Height * ButtonScale + ElementMarginY),
                "FOLD",
                ContentLoader.ButtonFont
            ));

            ButtonElements.Add(new ButtonElement(
                ContentLoader.ButtonTexture,
                new Vector2(ButtonElements[1].Position.X,
                    ButtonElements[2].Position.Y),
                "CALL",
                ContentLoader.ButtonFont
            ));
        }

        /// <summary>
        /// Creates the Info UI Elements for the user's stats/info (CASH, BET & POT).
        /// </summary>
        public void UpdateUserInfoElements(int cash, int bet, int pot)
        {
            if (UserInfoElements != null)
                UserInfoElements.Clear();
            else
                UserInfoElements = new List<InfoElement>();
            
            UserInfoElements.Add(new InfoElement("CASH: $" + cash,
                new Vector2(ChipElements[ChipsAmount - 1].Position.X,
                    ChipElements[ChipsAmount - 1].Position.Y + ChipHeight + ElementMarginY),
                ContentLoader.DefaultFont
            ));

            UserInfoElements.Add(new InfoElement(
                "BET:" + AddAlignmentSpaces(ContentLoader.DefaultFont, "CASH:", "BET:") + "$" + bet,
                new Vector2(ChipElements[ChipsAmount - 1].Position.X,
                    ChipElements[ChipsAmount - 1].Position.Y + ChipHeight + ElementMarginY +
                    ContentLoader.DefaultFont.LineSpacing),
                ContentLoader.DefaultFont
            ));

            UserInfoElements.Add(new InfoElement(
                "POT:" + AddAlignmentSpaces(ContentLoader.DefaultFont, "CASH:", "POT:") + "$" + pot,
                new Vector2(ChipElements[ChipsAmount - 1].Position.X,
                    ChipElements[ChipsAmount - 1].Position.Y + ChipHeight + ElementMarginY +
                    ContentLoader.DefaultFont.LineSpacing * 2),
                ContentLoader.DefaultFont
            ));
        }

        /// <summary>
        /// Creates the Info UI Elements for the opponent's stats (CASH & BET).
        /// </summary>
        public void UpdateOpponentInfoElements(string opponentName, int opponentCash, int opponentBet)
        {
            if (OpponentInfoElements != null)
                OpponentInfoElements.Clear();
            else
                OpponentInfoElements = new List<InfoElement>();
            
            OpponentInfoElements.Add(new InfoElement(
                opponentName + "'S CASH: $" + opponentCash,
                new Vector2(
                    _resolution.VirtualScreen.X - ContentLoader.DefaultFont
                        .MeasureString(opponentName + "'S CASH: $" + opponentCash).X -
                    ElementMarginX, ElementMarginY),
                ContentLoader.DefaultFont));

            // X coord is aligned to CASH string since that should always be the longest
            OpponentInfoElements.Add(new InfoElement(
                opponentName + "'S BET:" + AddAlignmentSpaces(ContentLoader.DefaultFont, "CASH:", "BET:") + "$" +
                opponentBet,
                new Vector2(
                    _resolution.VirtualScreen.X - ContentLoader.DefaultFont
                        .MeasureString(opponentName + "'S CASH: $" + opponentCash).X -
                    ElementMarginX, ElementMarginY + ContentLoader.DefaultFont.LineSpacing),
                ContentLoader.DefaultFont));
        }

        /// <summary>
        /// Creates the Chip UI Elements with their corresponding Chip instances.
        /// </summary>
        public void CreateChipElements()
        {
            if (ChipElements != null)
                ChipElements.Clear();
            else
                ChipElements = new List<ChipElement>();

            foreach (var value in ChipValues.Skip(StartAtChipIndex))
            {
                var currentY = (int) (ElementMarginY + ChipElements.Count * (ChipHeight + ElementMarginY));
                ChipElements.Add(new ChipElement(new Chip(value,
                        ContentLoader.ChipSprites[StartAtChipIndex + ChipElements.Count]),
                    new Vector2(ElementMarginX, currentY),
                    new Rectangle((int) ElementMarginX, currentY,
                        (int) (ChipWidth + ElementMarginX),
                        (int) (ChipHeight + ElementMarginY)
                    )));
            }
        }

        /// <summary>
        /// Creates the user's Card UI Elements.
        /// </summary>
        public void UpdateUserCardElements(List<Card> userCards)
        {
            if (UserCardElements != null)
                UserCardElements.Clear();
            else
                UserCardElements = new List<CardElement>();
            
            UserCardElements.Add(new CardElement(userCards[0],
                new Vector2(_resolution.VirtualScreen.X / 2 - UserCardsWidth / 2,
                    _resolution.VirtualScreen.Y - CardHeight - ElementMarginX)));

            UserCardElements.Add(new CardElement(userCards[1],
                new Vector2(
                    _resolution.VirtualScreen.X / 2 - UserCardsWidth / 2 + CardWidth + ElementMarginX,
                    _resolution.VirtualScreen.Y - CardHeight - ElementMarginX)));
        }

        /// <summary>
        /// Creates/Updates the opponent's Card UI Elements.
        /// </summary>
        public void UpdateOpponentCardElements(List<Card> opponentCards)
        {
            if (OpponentCardElements != null)
                OpponentCardElements.Clear();
            else
                OpponentCardElements = new List<CardElement>();
            
            OpponentCardElements.Add(new CardElement(opponentCards[0],
                new Vector2(_resolution.VirtualScreen.X / 2 - UserCardsWidth / 2,
                    ElementMarginY)));

            OpponentCardElements.Add(new CardElement(opponentCards[1],
                new Vector2(_resolution.VirtualScreen.X / 2 - UserCardsWidth / 2 + CardWidth + ElementMarginX,
                    ElementMarginY)));
        }

        /// <summary>
        /// Creates/Updates the table's Card UI Elements.
        /// </summary>
        public void UpdateTableCardElements(List<Card> tableCards)
        {
            if (TableCardElements != null)
                TableCardElements.Clear();
            else
                TableCardElements = new List<CardElement>();
            
            for (var i = 0; i < TableCardsAmount; i++)
            {
                TableCardElements.Add(new CardElement(tableCards[i],
                    new Vector2(
                        _resolution.VirtualScreen.X / 2 - TableCardsWidth / 2 + i * (CardWidth + ElementMarginX),
                        _resolution.VirtualScreen.Y / 2 - CardHeight / 2)));
            }
        }

        /// <summary>
        /// Returns the string with added alignment spaces.
        /// Is not always 100% accurate due to different character sizes.
        /// </summary>
        /// <param name="font">Font of the text</param>
        /// <param name="reference">Reference string to align the text to.</param>
        /// <param name="toAlign">Text to align to the reference string.</param>
        /// <returns>Returns the spaces needed to align the strings</returns>
        private static string AddAlignmentSpaces(SpriteFont font, string reference, string toAlign)
        {
            return string.Concat(Enumerable.Repeat(" ",
                (int) ((font.MeasureString(reference).X - font.MeasureString(toAlign).X)
                       / font.MeasureString(" ").X) + 1).ToArray());
        }
    }
}