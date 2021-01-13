namespace Poker
{
    public class UI
    {
        private readonly Resolution _resolution;
        private float _scaleX;
        private float _scaleY;
        private float _elementMarginX = 10;
        private float _elementMarginY = 10;

        public int InitialChipHeight = 162;
        public int InitialChipWidth = 162;
        public int InitialCardHeight = 96;
        public int InitialCardWidth = 72;

        public float ChipHeight;
        public float ChipWidth;
        public float CumulativeChipHeight;
        public float CardHeight;
        public float CardWidth;
        public float UserCardsWidth;

        private const int UserCardsAmount = 2;
        private const int ChipsAmount = 4;
        private const float ChipScale = 0.4f;

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
            _scaleX = 1 / _resolution.ScreenScale.X;
            _scaleY = 1 / _resolution.ScreenScale.Y;
            _elementMarginX *= _scaleX;
            _elementMarginY *= _scaleY;
            
            ChipWidth = InitialChipWidth * ChipScale;
            ChipHeight = InitialChipWidth * ChipScale;
            CumulativeChipHeight = ChipsAmount * ChipHeight + (ChipsAmount - 1) * _elementMarginY;

            CardHeight = InitialCardHeight * _scaleY;
            CardWidth = InitialCardWidth * _scaleX;
            UserCardsWidth = UserCardsAmount * CardWidth + (UserCardsAmount - 1) * _elementMarginX;
        }
    }
}