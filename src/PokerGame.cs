using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Threading;

namespace Poker
{
    public class PokerGame : Game
    {
        private int _userBetAmount;
        private int _opponentBetAmount;

        // Input
        private KeyboardState _previousKeyBoardState;
        private KeyboardState _keyBoardState;
        private MouseState _previousMouseState;
        private MouseState _mouseState;

        // General
        private GraphicsDeviceManager _graphics;
        private GameServer _gameServer;
        private Client _client;
        private Thread ServerThread;
        private Thread ClientThread;
        private static Random Random = new Random();
        private readonly string _clientKey = GenerateRandomKey(16);
        private const string ServerPassword = "test1234";
        private bool isHost;
        public GameUI UserInterface;
        public Resolution WindowResolution = new Resolution();
        public Drawer GameDrawer;
        private bool _firstRound = true;

        public PokerGame(bool isHost)
        {
            this.isHost = isHost;
            if (isHost)
            {
                _gameServer = new GameServer(key: ServerPassword);
                ServerThread = new Thread(_gameServer.PollEvents);
            }

            _client = new Client(_clientKey, ServerPassword) {User = new User(_clientKey, isHost ? "HOST" : "CLIENT")};
            ClientThread = new Thread(_client.PollEvents);

            _graphics = new GraphicsDeviceManager(this);
            WindowResolution.Update(_graphics);
            TouchPanel.EnableMouseTouchPoint = true;
            _graphics.ApplyChanges();
            IsMouseVisible = true;
        }

        private void StartServer()
        {
            ServerThread.Start();
            _gameServer.CreateDeck(UserInterface.ContentLoader.CardSprites);
            _gameServer.AddCardsToTable();
        }

        protected override void LoadContent()
        {
            UserInterface = new GameUI(WindowResolution, new ContentLoader(Content));
            GameDrawer = new Drawer(UserInterface, new SpriteBatch(GraphicsDevice));
            
            UserInterface.ContentLoader.LoadTextures();
            UserInterface.ContentLoader.SplitCardSprites();
            UserInterface.ContentLoader.SplitChipSprites();
            
            UserInterface.CreateChipElements();
            UserInterface.CreateButtonElements();
            UserInterface.UpdateOpponentInfoElements(_client.Opponent.Name, 0, 0);
            UserInterface.UpdateUserInfoElements(0, 0, 0);
            
            if (isHost) StartServer();

            _client.Deck = new Deck(UserInterface.ContentLoader.CardSprites);
            ClientThread.Start();
            _client.BroadcastClientUpdate();
            Thread.Sleep(1000);
            UserInterface.UpdateUserCardElements(_client.User.Cards);
        }

        protected override void Update(GameTime gameTime)
        {
            BetButtonSwitcher();
            
            // Update User UI
            if (_client.User.IsChangedByClient)
            {
                UserInterface.UpdateUserInfoElements(_client.User.Cash, _client.User.Bets.Sum(), _client.Table.Pot);
                _client.BroadcastClientUpdate();
            }

            if (_client.User.IsChangedByServer)
            {
                UserInterface.UpdateUserInfoElements(_client.User.Cash, _client.User.Bets.Sum(), _client.Table.Pot);
                UserInterface.UpdateUserCardElements(_client.User.Cards);
                _client.User.IsChangedByServer = false;
            }
            
            // Update Table UI
            if (!_client.Opponent.Key.Equals("") && _client.Table.HasChanged)
            {
                UserInterface.UpdateTableCardElements(_client.Table.Cards);
                _client.Table.HasChanged = false;
            }
            
            // Update Opponent UI
            if (!_client.Opponent.Key.Equals("") && (_client.Opponent.IsChangedByServer || _client.Opponent.IsChangedByClient))
            {
                UserInterface.UpdateOpponentCardElements(_client.Opponent.Cards);
                UserInterface.UpdateOpponentInfoElements(_client.Opponent.Name, _client.Opponent.Cash, _client.Opponent.Bets.Sum());
                if (isHost && _firstRound)
                {
                    _gameServer.ProcessGameLogic(true);
                    _firstRound = false;
                }
            }
            
            if (_client.Opponent.Bets != null && _client.User.Bets.Count > 0)
            {
                if (_client.Opponent.Bets.Sum() > _client.User.Bets.Sum())
                {
                    _client.User.HasBetted = false;
                }
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
                var mousePos = GetMouseCoords();
                if (!_client.User.HasBetted && _client.User.Cash > 0 && !_client.User.HasLostRound && !_client.Opponent.HasLostRound)
                {
                    foreach (var chipElement in UserInterface.ChipElements
                        .Where(chipElement => chipElement.Container.Contains(mousePos)))
                    {
                        if (_client.User.HasAddedBet)
                        {
                            if (_client.User.Cash - _client.User.Bets[^1] >= chipElement.Chip.Value)
                            {
                                // User has 0 or more money left
                                _client.User.Bets[^1] += chipElement.Chip.Value;
                            }
                            else
                            {
                                // User has less than the chips worth so add max money to bet
                                _client.User.Bets[^1] += _client.User.Cash - _client.User.Bets[^1];
                            }
                        }
                        else
                        {
                            _client.User.Bets.Add(_client.User.Cash >= chipElement.Chip.Value ? chipElement.Chip.Value : _client.User.Cash);
                            _client.User.BetIsProcessed.Add(false);
                            _client.User.HasAddedBet = true;
                        }

                        _client.User.IsChangedByClient = true;
                    }
                }

                for (var i = 0; i < UserInterface.ButtonElements.Count; i++)
                {
                    if (UserInterface.ButtonElements[i].Container.Contains(mousePos) 
                        && !_client.User.HasLostRound && !_client.Opponent.HasLostRound)
                    {
                        switch (i)
                        {
                            // Clear Bet
                            case 0:
                                if (_client.User.HasAddedBet)
                                {
                                    _client.User.Bets.Remove(_client.User.Bets[^1]);
                                    _client.User.HasAddedBet = false;
                                    _client.User.BetIsProcessed.Remove(_client.User.BetIsProcessed[^1]);
                                }

                                _client.User.IsChangedByClient = true;
                                break;

                            // Confirm Bet
                            case 1:
                                if (_client.User.HasAddedBet)
                                {
                                    if (!(_client.Opponent.HasBetted && _client.User.Bets.Sum() < _client.Opponent.Bets?.Sum()))
                                    {
                                        _client.User.HasBetted = true;
                                        _client.User.HasAddedBet = false;
                                        _client.User.IsChangedByClient = true;
                                    }
                                }

                                break;

                            // Fold
                            case 2:
                                _client.User.HasFolded = true;
                                _client.User.IsChangedByClient = true;
                                break;

                            // Call
                            case 3:
                                if (!_client.User.HasBetted && _client.Opponent.HasBetted && _client.Opponent.Bets != null)
                                {
                                    if (_client.User.HasAddedBet)
                                    {
                                        _client.User.Bets[^1] = Math.Min(_client.Opponent.Bets.Sum() - _client.User.Bets.Sum() + _client.User.Bets[^1],
                                            _client.User.Cash);
                                    }
                                    else
                                    {
                                        _client.User.Bets.Add(Math.Min(_client.Opponent.Bets.Sum() - _client.User.Bets.Sum(), _client.User.Cash));
                                        _client.User.BetIsProcessed.Add(false);
                                    }

                                    _client.User.HasAddedBet = false;
                                    _client.User.HasBetted = true;
                                    _client.User.IsChangedByClient = true;
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
            GameDrawer.DrawBackground();
            
            GameDrawer.SpriteBatch.Begin(
                SpriteSortMode.Immediate,
                null,
                null,
                null,
                null,
                null,
                WindowResolution.Scale);
            
            // Draw Cards
            if (UserInterface.UserCardElements?.Count == GameUI.UserCardsAmount)
            {
                foreach (var userCardElement in UserInterface.UserCardElements)
                {
                    GameDrawer.DrawCardElement(userCardElement);
                }
            }
            
            if (UserInterface.OpponentCardElements?.Count == GameUI.UserCardsAmount)
            {
                foreach (var opponentCardElement in UserInterface.OpponentCardElements)
                {
                    GameDrawer.DrawCardElement(opponentCardElement);
                }
            }

            if (UserInterface.TableCardElements != null)
            {
                foreach (var tableCardElement in UserInterface.TableCardElements)
                {
                    GameDrawer.DrawCardElement(tableCardElement);
                }
            }

            // Draw Chips
            foreach (var chipElement in UserInterface.ChipElements)
            {
                GameDrawer.DrawChipElement(chipElement);
            }

            // Draw Info
            foreach (var userInfoElement in UserInterface.UserInfoElements)
            {
                GameDrawer.DrawInfoElement(userInfoElement);
            }
            
            foreach (var opponentInfoElement in UserInterface.OpponentInfoElements)
            {
                GameDrawer.DrawInfoElement(opponentInfoElement);
            }

            // Draw Buttons
            foreach (var buttonElement in UserInterface.ButtonElements)
            {
                GameDrawer.DrawButtonElement(buttonElement);
            }

            GameDrawer.SpriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Changes the Bet button element to Raise or vice versa when necessary
        /// </summary>
        private void BetButtonSwitcher()
        {
            if (_client.Opponent.Bets.Count > 0 && _client.User.Bets.Count > 0 && _client.Opponent.Bets.Sum() < _client.User.Bets.Sum())
                UserInterface.ButtonElements[1].TextElement.Text = "RAISE";
            else
                UserInterface.ButtonElements[1].TextElement.Text = "BET";
            UserInterface.ButtonElements[1].TextElement.Position = UserInterface.ButtonElements[1].CalculateTextPosition();
        }

        /// <summary>
        /// Gets the coordinates of the current mouse position
        /// </summary>
        /// <returns>A scaled Vector2 with the mouse coordinates</returns>
        private Vector2 GetMouseCoords()
        {
            float x = Mouse.GetState().X;
            float y = Mouse.GetState().Y;

            var sceneCoords = new Vector2(x / WindowResolution.ScreenScale.X, y / WindowResolution.ScreenScale.Y);
            return sceneCoords;
        }

        /// <summary>
        /// Generates a random string as client/server key.
        /// </summary>
        /// <param name="length"></param>
        /// <returns>A random string with the specified length</returns>
        public static string GenerateRandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
    
    /// <summary>
    /// Deep clones an object.
    /// </summary>
    public static class ExtensionMethods
    {
        public static T DeepClone<T>(this T a)
        {
            using var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, a);
            stream.Position = 0;
            return (T) formatter.Deserialize(stream);
        }
    }
}
