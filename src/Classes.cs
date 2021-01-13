using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Xna.Framework.Content;

namespace Poker
{
    public enum Category
    {
        Clubs = 0,
        Spades = 1,
        Hearts = 2,
        Diamonds = 3,
        Reverse = 4,
        Joker = 5,
    }

    public enum Type
    {
        None = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14,
    }

    [Serializable]
    public class Card
    {
        public Category CardCategory { get; set; }
        public Type CardType { get; set; }

        [JsonIgnore] [NonSerialized] public Texture2D CardTexture;

        public Card(Category category, Type type, Texture2D texture)
        {
            CardCategory = category;
            CardType = type;
            CardTexture = texture;
        }

        [JsonConstructor]
        public Card()
        {
        }

        private static Color GetColor(Category category)
        {
            Color color = category switch
            {
                Category.Clubs => Color.Black,
                Category.Spades => Color.Black,
                Category.Hearts => Color.Red,
                Category.Diamonds => Color.Red,
                _ => new Color()
            };

            return color;
        }
    }

    public class Deck
    {
        public List<Card> Cards;
        public Card Reverse;

        public Deck(bool addJokers, Texture2D[] textures)
        {
            var cardAmount = addJokers ? 55 : 53;
            Cards = GetCards(cardAmount, textures);
        }

        private List<Card> GetCards(int cardAmount, Texture2D[] textures)
            /*
             * Creates the deck of cards from the loaded textures.
             */
        {
            List<Card> cards = new List<Card>();
            const int rows = 4;
            const int cols = 14;
            const int cardsWithoutJoker = 53; // Includes 1 reverse

            for (var i = 0; i < rows; i++)
            {
                for (int j = (i != 1) ? 0 : 1; j < cols; j++)
                {
                    if (j.Equals(0)) // First col (reverse, reverse, joker, joker)
                    {
                        if (i == 0)
                        {
                            Reverse = new Card(Category.Reverse, Type.None, textures[i * cols + j]);
                        }

                        if (cardAmount > cardsWithoutJoker && i > 1)
                        {
                            cards.Add(new Card(Category.Joker, Type.None, textures[i * cols + j]));
                        }
                    }
                    else
                    {
                        cards.Add(new Card((Category) i, (Type) j + 1, textures[i * cols + j]));
                    }
                }
            }

            return cards;
        }

        public Deck Shuffle()
        {
            Cards = Cards.OrderBy(a => Guid.NewGuid()).ToList();
            return this;
        }
    }

    class Chip
    {
        public int ChipWorth;
        public Texture2D ChipTexture;
        public Vector2 Position;
        public Rectangle Container;

        public Chip(int worth, Texture2D texture)
        {
            ChipWorth = worth;
            ChipTexture = texture;
        }
    }

    class Button
    {
        public Texture2D ButtonTexture;
        public Vector2 Position;
        public Rectangle Container;
        private float Scale;
        private SpriteFont ButtonFont;
        public string Text;
        public Vector2 TextPosition;

        public Button(Texture2D texture, Vector2 buttonPosition, string text, SpriteFont buttonFont,
            float buttonScale = 1.0f)
        {
            ButtonTexture = texture;
            Position = buttonPosition;
            Scale = buttonScale;
            Text = text;
            ButtonFont = buttonFont;
            TextPosition = GetTextPosition();
            Container = new Rectangle((int) Position.X, (int) Position.Y,
                (int) (ButtonTexture.Width * Scale), (int) (ButtonTexture.Height * Scale));
        }

        public Vector2 GetTextPosition()
        {
            return new Vector2(
                Position.X + (ButtonTexture.Width * Scale / 2 - ButtonFont.MeasureString(Text).X / 2),
                Position.Y + (ButtonTexture.Height * Scale / 2 - (float) ButtonFont.LineSpacing / 2));
        }
    }

    [Serializable]
    public class User
    {
        public int Cash { get; set; }
        public List<int> Bets { get; set; }
        public bool HasAddedBet { get; set; }
        public bool HasBetted { get; set; }

        public List<bool> BetIsProcessed { get; set; }
        public string Name { get; set; }
        public List<Card> Cards { get; set; }
        public bool HasChanged { get; set; }
        public bool HasFolded { get; set; }
        public string Key { get; set; }

        public bool HasLost { get; set; }
        public bool HasLostGame { get; set; }

        public User(string key, string name = "User")
        {
            Key = key;
            Name = name.ToUpper();
            Init();
        }

        public User(int cash)
        {
            Cash = cash;
            Init();
        }

        [JsonConstructor]
        public User()
        {
            Init();
        }

        private void Init()
        {
            if (Cash == 0)
            {
                Cash = 1000;
            }

            Key ??= "";
            Bets = new List<int>();
            HasAddedBet = false;
            HasFolded = false;
            HasLost = false;
            HasLostGame = false;
            HasChanged = true;
            HasBetted = false;
            Cards = new List<Card>();
            BetIsProcessed = new List<bool>();
        }
    }

    [Serializable]
    public class Table
    {
        public List<Card> Cards;
        public int Pot;
        public int RealCardsAmount;

        [JsonConstructor]
        public Table(int pot = 0, List<Card> cards = null)
        {
            Pot = pot;
            Cards = cards ?? new List<Card>();
            RealCardsAmount = 0;
        }
    }

    public class Resolution
    {
        private Vector3 _scalingFactor;
        private int _preferredBackBufferWidth;
        private int _preferredBackBufferHeight;

        public Vector2 VirtualScreen = new Vector2(1280, 800);
        private Vector2 ScreenAspectRatio = new Vector2(1, 1);
        public Matrix Scale;
        public Vector2 ScreenScale;

        public void Update(GraphicsDeviceManager device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            // Calculate ScalingFactor
            _preferredBackBufferWidth = device.PreferredBackBufferWidth;
            var widthScale = _preferredBackBufferWidth / VirtualScreen.X;

            _preferredBackBufferHeight = device.PreferredBackBufferHeight;
            var heightScale = _preferredBackBufferHeight / VirtualScreen.Y;

            ScreenScale = new Vector2(widthScale, heightScale);

            ScreenAspectRatio = new Vector2(widthScale / heightScale);
            _scalingFactor = new Vector3(widthScale, heightScale, 1);
            Scale = Matrix.CreateScale(_scalingFactor);
            device.ApplyChanges();
        }
    }

    public class Client
    {
        EventBasedNetListener _listener = new EventBasedNetListener();
        NetDataWriter _writer = new NetDataWriter();
        private NetManager _client;
        public Deck Deck;
        public User Enemy { get; set; }
        public string Username { get; set; }
        public User User { get; set; }
        public Table Table { get; set; }
        private Message _message;

        public Client(string clientKey, string password, string ip = "localhost", int port = 9050, string username="User")
        {
            Username = username;
            User = new User(clientKey, Username);
            Enemy = new User();
            Table = new Table(0);
            _client = new NetManager(_listener);
            _client.Start();
            _client.Connect(ip, port, password);
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                try
                {
                    _message = JsonConvert.DeserializeObject<Message>(dataReader.GetString());
                }
                catch (Exception e)
                {
                    Logger.WriteLine(string.Concat("Json conversion failed: ", e.ToString()));
                }

                if (_message.Key != clientKey)
                {
                    Logger.WriteLine($"Received {_message.Type} with Object: {_message.Object}");
                    switch (_message.Type)
                    {
                        // User (enemy) update from Enemy re-broadcasted by server
                        case "EnemyUpdate":
                        {
                            Enemy = JsonConvert.DeserializeObject<User>(_message.Object);
                            Enemy.Cards = ReplaceCardTextures(Enemy.Cards);
                            Logger.WriteLine("Enemy Update Successful");
                            break;
                        }

                        // User (user) update from Server
                        case "UserUpdate":
                        {
                            User = JsonConvert.DeserializeObject<User>(_message.Object);
                            User.Cards = ReplaceCardTextures(User.Cards);

                            Logger.WriteLine("User Update Successful");
                            break;
                        }

                        // Table update from Server
                        case "TableUpdate":
                        {
                            Table = JsonConvert.DeserializeObject<Table>(_message.Object);
                            Table.Cards = ReplaceCardTextures(Table.Cards);

                            Logger.WriteLine("Table Update Successful");
                            break;
                        }
                    }
                }
            };
        }

        private List<Card> ReplaceCardTextures(List<Card> cards)
        {
            foreach (var card in cards)
            {
                // Do-while loop because for some reason the texture assignment fails sometimes
                do
                {
                    if (card.CardCategory == Category.Reverse)
                    {
                        card.CardTexture = Deck.Reverse.CardTexture;
                    }
                    else
                    {
                        card.CardTexture = Deck.Cards.First(c =>
                            c.CardType == card.CardType && c.CardCategory == card.CardCategory).CardTexture;
                    }
                } while (card.CardTexture == null);
            }

            return cards;
        }

        public void PollEvents()
        {
            while (true)
            {
                _client.PollEvents();
                Thread.Sleep(15);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        public void Stop()
        {
            _client.Stop();
        }

        public void Broadcast(Message message)
        {
            Logger.WriteLine($"Broadcasting: {message.Type}");
            _writer.Put(message.Json());
            _client.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }
    }

    public class GameServer
    {
        private EventBasedNetListener _listener = new EventBasedNetListener();
        private NetDataWriter _writer;
        private NetManager _server;
        private string _password;
        private string _key = PokerGame.GetRandomString(16);
        private int _maxPlayers;
        private Table _table = new Table();
        private Table _broadCastTable = new Table();
        private List<User> _users;
        public Deck Deck;
        public int UserCardsAmount;
        private int _tableCardsAmount;
        private User _newUser;

        public GameServer(int port = 9050, string key = "", int maxPlayers = 2)
        {
            try
            {
                _writer = new NetDataWriter();
                _server = new NetManager(_listener);
                _password = key;
                _maxPlayers = maxPlayers;
                _users = new List<User>(_maxPlayers);
                _server.Start(port);
            }
            catch (Exception)
            {
                Logger.WriteLine("Exception occurred, server might already be running on the same port.", 3);
            }

            _listener.ConnectionRequestEvent += request =>
            {
                if (_server.GetPeersCount(ConnectionState.Connected) < _maxPlayers)
                    request.AcceptIfKey(_password);
                else
                    request.Reject();
            };

            _listener.PeerConnectedEvent += peer =>
            {
                Logger.WriteLine($"New Connection: {peer.EndPoint}");
                _writer.Put(new Message(_key, "ConnectionSuccessful").Json());
                peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                _writer.Reset();
                Logger.WriteLine($"Connected Clients: {_server.GetPeersCount(ConnectionState.Connected)}");
                if (_users.Count >= 1)
                {
                    ProcessRound(true);
                }
            };

            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var message = dataReader.GetString();
                Broadcast(JsonConvert.DeserializeObject<Message>(message), fromPeer.Id);
                dataReader.Recycle();
            };
        }

        public void CreateDeck(Texture2D[] textures, bool addJokers = false, bool shuffle = true)
        {
            Deck = new Deck(addJokers, textures);
            ResetDeck(shuffle);
        }

        private void ResetDeck(bool shuffle = true)
        {
            if (shuffle)
            {
                Deck.Shuffle();
            }

            // Add Cards to Table
            _table = new Table(0);
            for (int j = 0; j < 5; j++)
            {
                _table.Cards.Add(Deck.Cards[_maxPlayers * UserCardsAmount + j]);
            }
        }

        public void PollEvents()
        {
            while (true)
            {
                _server.PollEvents();
                ProcessRound();
                Thread.Sleep(15);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        public void Stop()
        {
            _server.Stop();
        }

        private void Broadcast(Message message, int fromPeerId = -1)
        {
            if (Deck == null) return;
            if (!message.Type.Equals("UserUpdate"))
            {
                try
                {
                    // Deserialize broadcasted User and check if it already exists in _users, then replace the saved User
                    // with the broadcasted user but keep the Cards/Cash. This way we always have the latest Users cached.
                    _newUser = JsonConvert.DeserializeObject<User>(message.Object);
                    if (_users.Any(user => user.Key.Equals(_newUser.Key)))
                    {
                        var user = _users.First(u => u.Key.Equals(_newUser.Key));
                        _newUser.Cards = user.Cards;
                        _newUser.Cash = user.Cash;
                        _newUser.HasLost = user.HasLost;
                        _newUser.HasLostGame = user.HasLostGame;
                        _users[_users.FindIndex(u => u.Key == user.Key)] = _newUser.DeepClone();
                    }
                    else
                    {
                        _newUser.Cards = new List<Card>();
                        for (var j = 0; j < UserCardsAmount; j++)
                        {
                            _newUser.Cards.Add(Deck.Cards[_users.Count * UserCardsAmount + j]);
                        }

                        _users.Add(_newUser.DeepClone());
                        Broadcast(new Message(_key, "UserUpdate", _users[^1]),
                            _users.Count > 1 ? _server.ConnectedPeerList.First(peer => peer.Id != fromPeerId).Id : -1);
                    }

                    for (int i = 0; i < _newUser.Cards.Count; i++)
                    {
                        _newUser.Cards[i] = Deck.Reverse;
                    }

                    message.Object = JsonConvert.SerializeObject(_newUser);
                    _newUser = null;
                }
                catch (Exception e)
                {
                    // Message was not a User
                    Logger.WriteLine(e.ToString());
                }
            }

            _writer.Put(message.Json());
            if (fromPeerId >= 0)
            {
                _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered, _server.GetPeerById(fromPeerId));
            }
            else
            {
                _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
            }

            _writer.Reset();
        }

        private void BroadCastUsers()
        {
            for (int i = 0; i < _users.Count; i++)
            {
                var message = new Message(_key, "UserUpdate", _users[i]);
                _writer.Put(message.Json());
                _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered,
                    _users.Count > 1
                        ? i == 0 ? _server.FirstPeer :
                        _server.ConnectedPeerList.First(peer => peer != _server.FirstPeer)
                        : null);
                _writer.Reset();
            }
        }

        private void BroadCastTable()
        {
            _table.RealCardsAmount = _tableCardsAmount;
            _broadCastTable = _table.DeepClone();
            for (int i = 0; i < 5 - _tableCardsAmount; i++)
            {
                _broadCastTable.Cards[4 - i] = Deck.Reverse;
            }

            var message = new Message(_key, "TableUpdate", _broadCastTable);
            _writer.Put(message.Json());
            _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }

        public void ProcessRound(bool first = false)
        {
            var usersChanged = false;
            var tableChanged = false;

            // Process Bets
            foreach (var user in _users)
            {
                if (user.HasBetted && !user.BetIsProcessed[^1])
                {
                    user.HasAddedBet = false;
                    _table.Pot += user.Bets[^1];
                    user.Cash -= user.Bets[^1];
                    user.BetIsProcessed[^1] = true;

                    usersChanged = true;
                    tableChanged = true;
                }

                if (!user.HasFolded) continue;
                
                user.HasLost = true;
                if (user.Cash <= 0)
                {
                    user.HasLostGame = true;
                }
                else
                {
                    foreach (var u in _users.Where(u => u.Key != user.Key))
                    {
                        u.Cash += _table.Pot / _users.Count(us => us.Key != user.Key);
                    }

                    ResetRound();
                }

                usersChanged = true;
                tableChanged = true;
            }
            
            // Process End of Bets (Table cards, reset bets)
            if (_users.Count >= 2 && _users.All(user => user.HasBetted)
                                  && _users.All(user => user.BetIsProcessed.All(bet => bet))
                                  && _users[0].Bets.Sum() == _users[1].Bets.Sum())
            {
                if (_tableCardsAmount < 6)
                {
                    _tableCardsAmount = _tableCardsAmount == 0 ? 3 : ++_tableCardsAmount;
                    tableChanged = true;
                }

                foreach (var user in _users)
                {
                    user.HasBetted = false;
                    user.BetIsProcessed = new List<bool>();
                    user.Bets = new List<int>();
                }
            }
            
            // End of Game
            if (_users.Any(user => user.HasLostGame))
            {
                ResetGame();
                BroadCastTable();
                BroadCastUsers();

                tableChanged = false;
                usersChanged = false;
            }

            // Broadcast Table if changed
            if ((tableChanged || first) && Deck != null)
            {
                BroadCastTable();
            }

            // Broadcast UserUpdates if changed
            if (usersChanged || first)
            {
                BroadCastUsers();
            }
        }

        private void ResetRound()
        {
            _tableCardsAmount = 0;
            ResetDeck();
            foreach (var user in _users)
            {
                user.Bets = new List<int>();
                user.HasAddedBet = false;
                user.HasBetted = false;
                user.HasFolded = false;
                user.HasLost = false;
                user.HasLostGame = false;
                user.HasChanged = true;
                user.Cards = new List<Card>();
            }

            for (var i = 0; i < _users.Count; i++)
            {
                for (var j = 0; j < UserCardsAmount; j++)
                {
                    _users[i].Cards.Add(Deck.Cards[i * UserCardsAmount + j]);
                }
            }
        }

        private void ResetGame()
        {
            ResetRound();
        }
    }

    public class Message
    {
        public string Key { get; }
        public string Type { get; }
        public string Object { get; set; }

        public Message(string key, string type, object obj = null)
        {
            Key = key;
            Type = type;
            Object = JsonConvert.SerializeObject(obj);
        }

        public string Json()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
