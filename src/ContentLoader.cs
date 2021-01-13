using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Poker
{
    public class ContentLoader
    {
        private readonly ContentManager _contentManager;
        public SpriteFont DefaultFont;
        public SpriteFont ButtonFont;
        public Texture2D ButtonTexture;
        public Texture2D BackgroundImage;
        public Texture2D InitialCardSprites;
        public Texture2D InitialChipSprites;
        public List<Texture2D> CardSprites;
        public List<Texture2D> ChipSprites;


        public ContentLoader(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }
        
        /// <summary>
        /// Method that loads all necessary textures.
        /// </summary> 
        public void LoadTextures()
        {
            DefaultFont = _contentManager.Load<SpriteFont>("font");
            ButtonFont = _contentManager.Load<SpriteFont>("ButtonFont");
            ButtonTexture = _contentManager.Load<Texture2D>("button");
            BackgroundImage = _contentManager.Load<Texture2D>("pokertable");
            InitialCardSprites = _contentManager.Load<Texture2D>("cards");
            InitialChipSprites = _contentManager.Load<Texture2D>("chips");
        }

        /// <summary>
        /// Method that splits the card sprites from the sprite sheet.
        /// </summary> 
        public void SplitCardSprites()
        {
            const int cardWidth = 72;
            const int cardHeight = 96;
            CardSprites = SplitTexture(InitialCardSprites, cardWidth, cardHeight);
        }
        
         /// <summary>
         /// Method that splits the chip sprites from the sprite sheet.
         /// </summary> 
         public void SplitChipSprites()
        {
            const int chipWidth = 162;
            const int chipHeight = 162;
            ChipSprites = SplitTexture(InitialChipSprites, chipWidth, chipHeight);
        }
        
        /// <summary>
        /// Method that splits sprites from a sprite sheet.
        /// </summary>
        /// <param name="original">The sprite sheet you want to split</param>
        /// <param name="partWidth">Width of a single sprite</param>
        /// <param name="partHeight">Height of a single sprite</param>
        /// <returns>A list of sprites cut from the sprite sheet</returns>
        private static List<Texture2D> SplitTexture(Texture2D original, int partWidth, int partHeight)
        {
            var yCount = original.Height / partHeight;
            var xCount = original.Width / partWidth;
            var textures = new Texture2D[xCount * yCount];

            var dataPerPart = partWidth * partHeight;
            var originalData = new Color[original.Width * original.Height];
            original.GetData(originalData);

            var textureIndex = 0;
            for (var y = 0; y < yCount * partHeight; y += partHeight)
            {
                for (var x = 0; x < xCount * partWidth; x += partWidth)
                {
                    var part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
                    var partData = new Color[dataPerPart];

                    for (var partY = 0; partY < partHeight; partY++)
                    {
                        for (var partX = 0; partX < partWidth; partX++)
                        {
                            var partIndex = partX + partY * partWidth;
                            if (y + partY >= original.Height || x + partX >= original.Width)
                            {
                                partData[partIndex] = Color.Transparent;
                            }
                            else
                            {
                                partData[partIndex] = originalData[(x + partX) + (y + partY) * original.Width];
                            }
                        }
                    }
                    part.SetData(partData);
                    textures[textureIndex++] = part;
                }
            }
            return textures.ToList();
        }
    }
}
