using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Poker
{
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
}