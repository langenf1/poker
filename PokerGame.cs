using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Threading;
using Microsoft.Xna.Framework.Content;

namespace Poker
{
    public class PokerGame : Game
    {
        // Cards
        private Texture2D _cardTexturesInitial;
        private Texture2D[] _cardTextures;
        private List<Vector2> _userCardsPositions = new List<Vector2>();
        private List<Vector2> _enemyCardsPositions = new List<Vector2>();
        private List<Vector2> _tableCardsPositions = new List<Vector2>();
        private int _cardWidth = 72;
        private int _cardHeight = 96;
        private int _userCardsWidth;
        private int _tableCardsWidth;
        private const int UserCardsAmount = 2;

        // Chips        
        private int ChipsAmount = 4;
        private Texture2D _chipTexturesInitial;
        private Texture2D[] _chipTextures;
        private List<Chip> _chips = new List<Chip>();
        private const float ChipScale = 0.4f;
        private int _initialChipWidth = 162;
        private int _initialChipHeight = 162;
        private int _chipWidth;
        private int _chipHeight;
        private int _allChipsHeight;

        // UI
        private SpriteFont _font;
        private SpriteFont _buttonFont;
        private Resolution _resolution = new Resolution();
        private float _marginX = 10;
        private float _marginY = 10;
        private float _scaleX;
        private float _scaleY;
        private List<Vector2> _uiPositions = new List<Vector2>();

        // Strings / Info
        private string _userCash;
        private string _userBet;
        private int _userBetAmount;
        private string _enemyCash;
        private string _enemyBet;
        private int _enemyBetAmount;
        private string _pot;

        // Buttons
        private Texture2D _buttonTexture;
        private const float ButtonScale = 0.15f;

        private List<Button> _buttons = new List<Button>();
        private Button _clearBetButton;
        private Button _betButton;
        private Button _foldButton;
        private Button _callButton;

        // Input
        private KeyboardState _previousKeyBoardState;
        private KeyboardState _keyBoardState;
        private MouseState _previousMouseState;
        private MouseState _mouseState;

        // General
        private GraphicsDeviceManager _graphics;
        private Texture2D _backgroundImage;
        private SpriteBatch _spriteBatch;
        private User _user;
        private User _enemy;
        private Table _table;
        private GameServer _gameServer;
        private Client _client;
        private Thread ServerThread;
        private Thread ConnThread;
        private static Random Random = new Random();
        private readonly string _clientKey = GetRandomString(16);
        private const string ServerPassword = "test1234";
        private bool _host;

        public PokerGame(bool host)
        {
            _host = host;
            if (_host)
            {
                _gameServer = new GameServer(key: ServerPassword) {UserCardsAmount = UserCardsAmount};
                ServerThread = new Thread(_gameServer.PollEvents);
            }

            _client = new Client(_clientKey, ServerPassword);
            ConnThread = new Thread(_client.PollEvents);

            _user = new User(_clientKey);
            _enemy = _client.Enemy;

            _graphics = new GraphicsDeviceManager(this);
            _resolution.Update(_graphics);
            TouchPanel.EnableMouseTouchPoint = true;
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            UpdateEnemy();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");
            _buttonFont = Content.Load<SpriteFont>("ButtonFont");
            _buttonTexture = Content.Load<Texture2D>("button");
            _backgroundImage = Content.Load<Texture2D>("pokertable");

            // Texture splitting
            _cardTexturesInitial = Content.Load<Texture2D>("cards");
            _chipTexturesInitial = Content.Load<Texture2D>("chips");
            _cardTextures = Split(_cardTexturesInitial, _cardWidth, _cardHeight);
            _chipTextures = Split(_chipTexturesInitial, _initialChipWidth, _initialChipHeight);

            // Scaling texture sizes
            _scaleX = 1 / _resolution.ScreenScale.X;
            _scaleY = 1 / _resolution.ScreenScale.Y;
            _marginX *= _scaleX;
            _marginY *= _scaleY;

            _initialChipHeight = (int) (_initialChipHeight * _scaleY);
            _initialChipWidth = (int) (_initialChipWidth * _scaleX);
            _chipWidth = (int) (_initialChipWidth * ChipScale);
            _chipHeight = (int) (_initialChipWidth * ChipScale);
            _allChipsHeight = (int) (ChipsAmount * _chipHeight + (ChipsAmount - 1) * _marginY);

            _cardHeight = (int) (_cardHeight * _scaleY);
            _cardWidth = (int) (_cardWidth * _scaleX);
            _userCardsWidth = (int) (UserCardsAmount * _cardWidth + (UserCardsAmount - 1) * _marginX);

            // Chips and CardsDeck Initialization
            if (_host)
            {
                ServerThread.Start();
                _gameServer.CreateDeck(_cardTextures);
            }

            _client.Deck = new Deck(false, _cardTextures);
            ConnThread.Start();

            if (_host)
            {
                _gameServer.ProcessRound(true);
            }

            //Chips.Add(new Chip(5, ChipTextures[0]));
            //Chips.Add(new Chip(10, ChipTextures[1]));
            _chips.Add(new Chip(25, _chipTextures[2]));
            _chips.Add(new Chip(50, _chipTextures[3]));
            _chips.Add(new Chip(100, _chipTextures[4]));
            _chips.Add(new Chip(250, _chipTextures[5]));

            for (int i = 0; i < ChipsAmount; i++)
            {
                // Y Center
                // float y = resolution.VirtualScreen.Y / 2 - (float) AllChipsHeight / 2 + i * (ChipHeight + YMargin);

                _chips[i].Position = new Vector2(_marginX, _marginY + i * (_chipHeight + _marginY));
                _chips[i].Container = new Rectangle(
                    (int) _chips[i].Position.X,
                    (int) _chips[i].Position.Y,
                    (int) (_chipWidth + _marginX),
                    (int) (_chipHeight + _marginY)
                );
            }

            // Add User, Enemy and Table Cards positions
            _userCardsPositions.Add(new Vector2(
                _resolution.VirtualScreen.X / 2 - (float) _userCardsWidth / 2,
                _resolution.VirtualScreen.Y - _cardHeight - _marginX));
            _userCardsPositions.Add(new Vector2(
                _resolution.VirtualScreen.X / 2 - (float) _userCardsWidth / 2 + _cardWidth + _marginX,
                _resolution.VirtualScreen.Y - _cardHeight - _marginX));

            _enemyCardsPositions.Add(new Vector2(
                _resolution.VirtualScreen.X / 2 - (float) _userCardsWidth / 2,
                _marginY));
            _enemyCardsPositions.Add(new Vector2(
                _resolution.VirtualScreen.X / 2 - (float) _userCardsWidth / 2 + _cardWidth + _marginX,
                _marginY));

            _tableCardsPositions = new List<Vector2>();
            _tableCardsWidth = (int) (5 * _cardWidth + (5 - 1) * _marginX);
            for (int i = 0; i < 5; i++)
            {
                _tableCardsPositions.Add(
                    new Vector2(
                        _resolution.VirtualScreen.X / 2 - (float) _tableCardsWidth / 2 +
                        i * (_cardWidth + _marginX),
                        _resolution.VirtualScreen.Y / 2 - (float) _cardHeight / 2));
            }

            // UI
            // User
            _uiPositions.Add(new Vector2(_chips[ChipsAmount - 1].Position.X,
                _chips[ChipsAmount - 1].Position.Y + _chipHeight + _marginY));
            _uiPositions.Add(new Vector2(_uiPositions[^1].X,
                _uiPositions[^1].Y + _font.LineSpacing));

            // Pot
            _uiPositions.Add(new Vector2(_uiPositions[1].X, _uiPositions[1].Y + _font.LineSpacing));
            
            // Buttons
            // Clear Bet
            _clearBetButton = new Button(
                _buttonTexture,
                new Vector2(
                    _uiPositions[1].X,
                    _resolution.VirtualScreen.X / _scaleX - _buttonTexture.Height * ButtonScale * 2 - _marginY * 2),
                "CLEAR BET",
                _buttonFont,
                ButtonScale
            );

            // Bet
            _betButton = new Button(
                _buttonTexture,
                new Vector2(
                    _clearBetButton.Position.X + _buttonTexture.Width * ButtonScale + _marginX,
                    _clearBetButton.Position.Y),
                "BET",
                _buttonFont,
                ButtonScale
            );

            // Fold
            _foldButton = new Button(
                _buttonTexture,
                new Vector2(
                    _clearBetButton.Position.X,
                    _clearBetButton.Position.Y + _buttonTexture.Height * ButtonScale + _marginY),
                "FOLD",
                _buttonFont,
                ButtonScale
            );

            // Call
            _callButton = new Button(
                _buttonTexture,
                new Vector2(
                    _betButton.Position.X,
                    _foldButton.Position.Y),
                "CALL",
                _buttonFont,
                ButtonScale
            );

            _buttons.Add(_clearBetButton);
            _buttons.Add(_betButton);
            _buttons.Add(_foldButton);
            _buttons.Add(_callButton);
        }

        protected override void Update(GameTime gameTime)
        {
            // Get latest variables from server and update enemy with info
            UpdateEnemy();
            BetButtonSwitcher();
            _user = _client.User;
            _enemy = _client.Enemy;
            _table = _client.Table;
            if (_enemy.Bets != null && _user.Bets.Count > 0)
            {
                if (_enemy.Bets.Sum() > _user.Bets.Sum())
                {
                    _user.HasBetted = false;
                }
            }

            if (_uiPositions.Count < 5 && _enemy.Name != null)
            {
                // Enemy UI Positions
                // Cash/Bet Positions have the same MeasureString() for better alignment
                _uiPositions.Add(new Vector2(
                    _resolution.VirtualScreen.X - _font.MeasureString(_enemy.Name + "'S CASH: $" + _enemy.Cash).X - _marginX,
                    _marginY));
                _uiPositions.Add(new Vector2(
                    _resolution.VirtualScreen.X - _font.MeasureString(_enemy.Name + "'S CASH: $" + _enemy.Cash).X - _marginX,
                    _marginY + _font.LineSpacing));
            }

            _keyBoardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();

            // Exit Game
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                _keyBoardState.IsKeyDown(Keys.Escape))
            {
                Exit();
                Environment.Exit(0);
            }

            // Buttons/Chips
            if (_mouseState.LeftButton == ButtonState.Pressed
                && _previousMouseState.LeftButton == ButtonState.Released)
            {
                Vector2 mousePos = GetMouseCoords();
                if (!_user.HasBetted && _table.RealCardsAmount <= 5)
                {
                    foreach (var chip in _chips.Where(chip => chip.Container.Contains(mousePos)).Where(chip => _user.Cash > 0))
                    {
                        if (_user.HasAddedBet)
                        {
                            if (_user.Cash - _user.Bets[^1] >= chip.ChipWorth)
                            {
                                // User has 0 or more money left
                                _user.Bets[^1] += chip.ChipWorth;
                            }
                            else
                            {
                                // User has less than the chips worth so add max money to bet
                                _user.Bets[^1] += _user.Cash - _user.Bets[^1];
                            }
                        }
                        else
                        {
                            _user.Bets.Add(_user.Cash >= chip.ChipWorth ? chip.ChipWorth : _user.Cash);
                            _user.BetIsProcessed.Add(false);
                            _user.HasAddedBet = true;
                        }

                        _user.HasChanged = true;
                    }
                }

                for (int i = 0; i < _buttons.Count; i++)
                {
                    if (_buttons[i].Container.Contains(mousePos) && _table.RealCardsAmount <= 5)
                    {
                        switch (i)
                        {
                            // Clear Bet
                            case 0:
                                if (_user.HasAddedBet)
                                {
                                    _user.Bets.Remove(_user.Bets[^1]);
                                    _user.HasAddedBet = false;
                                    _user.BetIsProcessed.Remove(_user.BetIsProcessed[^1]);
                                }

                                _user.HasChanged = true;
                                break;

                            // Confirm Bet
                            case 1:
                                if (_user.HasAddedBet)
                                {
                                    if (!(_enemy.HasBetted && _user.Bets.Sum() < _enemy.Bets?.Sum()))
                                    {
                                        _user.HasBetted = true;
                                        _user.HasAddedBet = false;
                                        _user.HasChanged = true;
                                    }
                                }

                                break;

                            // Fold
                            case 2:
                                _user.HasFolded = true;
                                _user.HasChanged = true;
                                break;

                            // Call
                            case 3:
                                if (!_user.HasBetted && _enemy.HasBetted && _enemy.Bets != null)
                                {
                                    if (_user.HasAddedBet)
                                    {
                                        _user.Bets[^1] = Math.Min(_enemy.Bets.Sum() - _user.Bets.Sum() + _user.Bets[^1], _user.Cash);
                                    }
                                    else
                                    {
                                        _user.Bets.Add(Math.Min(_enemy.Bets.Sum() - _user.Bets.Sum(), _user.Cash));
                                        _user.BetIsProcessed.Add(false);
                                    }

                                    _user.HasAddedBet = false;
                                    _user.HasBetted = true;
                                    _user.HasChanged = true;
                                }

                                break;
                        }
                    }
                }
            }

            _previousMouseState = _mouseState;
            _previousKeyBoardState = _keyBoardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundImage, new Rectangle(0, 0, 800, 480), Color.White);
            _spriteBatch.End();

            // Draw cards
            _spriteBatch.Begin(
                SpriteSortMode.Immediate,
                null,
                null,
                null,
                null,
                null,
                _resolution.Scale);

            for (int i = 0; i < UserCardsAmount; i++)
            {
                if (_enemy.Cards.Count == UserCardsAmount)
                {
                    // Draw Enemy Cards
                    _spriteBatch.Draw(
                        _enemy.Cards[i].CardTexture,
                        _enemyCardsPositions[i],
                        null,
                        Color.White,
                        0f,
                        new Vector2(0, 0),
                        new Vector2(_scaleX, _scaleY),
                        SpriteEffects.None,
                        0f
                    );
                }

                if (_user.Cards.Count == UserCardsAmount)
                {
                    // Draw User Cards
                    _spriteBatch.Draw(
                        _user.Cards[i].CardTexture,
                        _userCardsPositions[i],
                        null,
                        Color.White,
                        0f,
                        new Vector2(0, 0),
                        new Vector2(_scaleX, _scaleY),
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            // Draw Table Cards
            if (_table.Cards != null)
            {
                for (int i = 0; i < _table.Cards.Count; i++)
                {
                    _spriteBatch.Draw(
                        _table.Cards[i].CardTexture,
                        _tableCardsPositions[i],
                        null,
                        Color.White,
                        0f,
                        new Vector2(0, 0),
                        new Vector2(_scaleX, _scaleY),
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            // Draw Chips
            if (ChipsAmount > 0)
            {
                foreach (Chip chip in _chips)
                {
                    _spriteBatch.Draw(
                        chip.ChipTexture,
                        chip.Position,
                        null,
                        Color.White,
                        0f,
                        new Vector2(0, 0),
                        new Vector2(_scaleX * ChipScale, _scaleY * ChipScale),
                        SpriteEffects.None,
                        0f
                    );
                }
            }

            // Draw Info and Options
            // User/Pot Strings
            _userBetAmount = _user.HasAddedBet ? _user.Bets[^1] : 0;
            _userCash = "CASH: $" + _user.Cash;
            _userBet = "BET:" + AddAlignmentSpaces(_font, "CASH:", "BET:") + "$" + _userBetAmount;
            _pot = "POT:" + AddAlignmentSpaces(_font, "CASH:", "POT:") + "$" + _table.Pot;
            _spriteBatch.DrawString(_font, _userCash, _uiPositions[0], Color.White);
            _spriteBatch.DrawString(_font, _userBet, _uiPositions[1], Color.White);
            _spriteBatch.DrawString(_font, _pot, _uiPositions[2], Color.White);

            // Enemy Strings
            _enemyBetAmount = _enemy.HasAddedBet || _enemy.HasBetted ? _enemy.Bets.Count > 0 ? _enemy.Bets[^1] : 0 : 0;
            _enemyCash = _enemy.Name + "'S CASH: $" + _enemy.Cash;
            _enemyBet = _enemy.Name + "'S BET:" + AddAlignmentSpaces(_font, "CASH:", "BET:") + "$" +
                       _enemyBetAmount;
            if (_uiPositions.Count > 3)
            {
                _spriteBatch.DrawString(_font, _enemyCash, _uiPositions[3], Color.White);
                _spriteBatch.DrawString(_font, _enemyBet, _uiPositions[4], Color.White);
            }

            // Buttons
            foreach (var button in _buttons)
            {
                _spriteBatch.Draw(
                    button.ButtonTexture,
                    button.Position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(0, 0),
                    new Vector2(ButtonScale, ButtonScale),
                    SpriteEffects.None,
                    0f
                );
                _spriteBatch.DrawString(_buttonFont, button.Text, button.TextPosition, Color.White);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private static string AddAlignmentSpaces(SpriteFont font, string reference, string toAlign)
            /*
             * Returns the string with added alignment spaces.
             * Is not always 100% accurate due to different character sizes
             */
        {
            return string.Concat(Enumerable.Repeat(" ",
                (int) ((font.MeasureString(reference).X - font.MeasureString(toAlign).X)
                       / font.MeasureString(" ").X) + 1).ToArray());
        }

        private void UpdateEnemy()
        {
            if (!_user.HasChanged) return;
            // Tell server User has changed and broadcast it to enemy
            _client.Broadcast(new Message(_clientKey, "EnemyUpdate", _user));
            _user.HasChanged = false;
        }

        private void BetButtonSwitcher()
        {
            if (_enemy.Bets.Count > 0 && _user.Bets.Count > 0 && _enemy.Bets.Sum() < _user.Bets.Sum())
            {
                // Change BET to RAISE Button
                _betButton.Text = "RAISE";
                _betButton.TextPosition = _betButton.GetTextPosition();
            }
            else
            {
                _betButton.Text = "BET";
                _betButton.TextPosition = _betButton.GetTextPosition();
            }
        }

        private Texture2D[] Split(Texture2D original, int partWidth, int partHeight)
        {
            int yCount = original.Height / partHeight;
            int xCount = original.Width / partWidth;

            Texture2D[]
                r = new Texture2D[xCount * yCount];

            int dataPerPart = partWidth * partHeight;
            Color[] originalData = new Color[original.Width * original.Height];
            original.GetData(originalData);

            int index = 0;
            for (int y = 0;
                y < yCount * partHeight;
                y += partHeight)
            for (int x = 0;
                x < xCount * partWidth;
                x += partWidth)
            {
                Texture2D part = new Texture2D(original.GraphicsDevice, partWidth, partHeight);
                Color[] partData = new Color[dataPerPart];

                for (int py = 0; py < partHeight; py++)
                for (int px = 0; px < partWidth; px++)
                {
                    int partIndex = px + py * partWidth;
                    if (y + py >= original.Height || x + px >= original.Width)
                        partData[partIndex] = Color.Transparent;
                    else
                        partData[partIndex] = originalData[(x + px) + (y + py) * original.Width];
                }

                part.SetData(partData);
                r[index++] = part;
            }

            return r;
        }

        private Vector2 GetMouseCoords()
        {
            float x = Mouse.GetState().X;
            float y = Mouse.GetState().Y;

            var sceneCoords = new Vector2(x / _resolution.ScreenScale.X, y / _resolution.ScreenScale.Y);
            return sceneCoords;
        }

        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }

    public static class Logger
    {
        public static void WriteLine(string message, int level = 0)
        {
            var levelString = "";
            switch (level)
            {
                case 0:
                    levelString = "DEBUG: ";
                    break;
                case 1:
                    levelString = "INFO: ";
                    break;
                case 2:
                    levelString = "WARNING: ";
                    break;
                case 3:
                    levelString = "ERROR: ";
                    break;
                case 4:
                    levelString = "FATAL: ";
                    break;
            }

            Console.WriteLine(String.Concat(DateTime.Now.ToString("HH:mm:ss"), " ", levelString, message));
        }
    }

    public static class ExtensionMethods
    {
        public static T DeepClone<T>(this T a)
        {
            using MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, a);
            stream.Position = 0;
            return (T) formatter.Deserialize(stream);
        }
    }
}
