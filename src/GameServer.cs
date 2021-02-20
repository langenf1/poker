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
        private string _key = PokerGame.GenerateRandomKey(16);

        private Table _table = new Table();
        private List<User> _users;
        private Deck _deck;
        private User _newUser;
        private GameServerLogic GameLogic;
        private const int DefaultCash = 1000;

        // If you change these, things will break (so don't)
        private const int MaxPlayers = 2;
        private const int UserCardsAmount = 2;

        public GameServer(int port = 9050, string key = "")
        {
            try
            {
                _writer = new NetDataWriter();
                _server = new NetManager(_listener);
                _password = key;
                _users = new List<User>(MaxPlayers);
                _server.Start(port);
                AddEventListeners();
                GameLogic = new GameServerLogic(ref _table, ref _deck, ref _users, DefaultCash);
            }
            catch (Exception)
            {
                Logger.Fatal("SERVER: Exception occurred during server startup, " + 
                             "server might already be running on the same port.");
                throw new Exception("SERVER: Exception occurred during server startup, " +
                                    "server might already be running on the same port.");
            }
        }

        private void AddEventListeners()
        {
            // Request to connect to server
            _listener.ConnectionRequestEvent += request =>
            {
                if (_server.GetPeersCount(ConnectionState.Connected) < MaxPlayers)
                    request.AcceptIfKey(_password);
                else
                    request.Reject();
            };

            // Approved new connection to server
            _listener.PeerConnectedEvent += peer =>
            {
                Logger.Info($"SERVER: New Connection: {peer.EndPoint}");
                _writer.Put(new Message(_key, "ConnectionSuccessful").Json());
                peer.Send(_writer, DeliveryMethod.ReliableOrdered);
                _writer.Reset();
                Logger.Info($"SERVER: Connected Clients: {_server.GetPeersCount(ConnectionState.Connected)}");
            };

            // Receive broadcast from client
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                // Rebroadcast so that other client can see.
                var message = JsonConvert.DeserializeObject<Message>(dataReader.GetString());
                Logger.Info(string.Concat("SERVER (78): Received ", message.Type, " from: ", fromPeer.Id));
                Broadcast(message, fromPeer.Id);
                dataReader.Recycle();
            };
        }

        /// <summary>
        /// Creates the server cards deck.
        /// </summary>
        /// <param name="textures">Card textures</param>
        /// <param name="addJokers">Whether to add jokers to the deck or not</param>
        /// <param name="shuffle">Shuffle the deck?</param>
        public void CreateDeck(List<Texture2D> textures, bool addJokers = false, bool shuffle = true)
        {
            _deck = new Deck(textures, addJokers);
            if (shuffle) _deck.Shuffle();
        }

        /// <summary>
        /// Adds 5 cards from the deck to the table.
        /// </summary>
        public void AddCardsToTable()
        {
            for (var j = 0; j < 5; j++)
            {
                _table.Cards.Add(_deck.Cards[MaxPlayers * UserCardsAmount + j]);
            }
        }

        /// <summary>
        /// Polls updates from all connections.
        /// </summary>
        public void PollEvents()
        {
            while (true)
            {
                _server.PollEvents();
                ProcessGameLogic();
                Thread.Sleep(15);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        /// Stops the server and closes all connections.
        /// </summary>
        public void Stop()
        {
            _server.Stop();
        }

        private void Broadcast(Message message, int fromPeerId = -1)
        {
            if (_deck == null) return;
            if (!message.Type.Equals("ServerClientUpdate"))
            {
                try
                {
                    // Deserialize broadcasted User and check if it already exists in _users, then replace the saved User
                    // with the broadcasted user but keep the Cards/Cash. This way we always have the latest Users cached.
                    _newUser = JsonConvert.DeserializeObject<User>(message.Object);
                    _newUser.IsChangedByServer = true;
                    _newUser.IsChangedByClient = false;

                    if (_users.Any(user => user.Key.Equals(_newUser.Key)))
                    {
                        var user = _users.First(u => u.Key.Equals(_newUser.Key));
                        _newUser.Cards = user.Cards;
                        _newUser.Cash = user.Cash;
                        _newUser.HasLostRound = user.HasLostRound;
                        _newUser.HasLostGame = user.HasLostGame;
                        _users[_users.FindIndex(u => u.Key == user.Key)] = _newUser.DeepClone();
                    }
                    else
                    {
                        // New user joined
                        _newUser = new User(_newUser.Key, _newUser.Name, DefaultCash) {Cards = new List<Card>()};
                        for (var j = 0; j < UserCardsAmount; j++)
                            _newUser.Cards.Add(_deck.Cards[_users.Count * UserCardsAmount + j]);

                        _users.Add(_newUser.DeepClone());
                        message = new Message(_key, "ServerClientUpdate");
                        if (_users.Count == 2)
                            BroadCastServerOpponentUpdates();
                    }

                    message.Object = JsonConvert.SerializeObject(_newUser);
                    _newUser = null;
                }
                catch (Exception e)
                {
                    // Message was not a User
                    Logger.Error(e.ToString());
                }
            }
            
            if (_server.ConnectedPeerList != null)
            {
                message.Type = "ServerClientUpdate";
                _writer.Put(message.Json());
                Logger.Info(string.Concat("SERVER (182): Broadcasting ", message.Type, " to: ", fromPeerId));
                
                _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered,
                    _users.Count == 2 ? _server.ConnectedPeerList.First(peer => peer.Id != fromPeerId) : null);
            }

            _writer.Reset();
        }

        private void BroadCastServerClientUpdates()
        {
            Logger.Info($"SERVER: Broadcasting ServerClientUpdates to {_users.Count} user(s).");
            for (var i = 0; i < _users.Count; i++)
            {
                _users[i].IsChangedByServer = true;
                _users[i].IsChangedByClient = false;
                var message = new Message(_key, "ServerClientUpdate", _users[i]);
                _writer.Put(message.Json());
                _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered, i == 0 ? _server.GetPeerById(1) : _server.GetPeerById(0));
                _writer.Reset();
            }
        }

        private void BroadCastServerOpponentUpdates(bool makeCardsReverse = true)
        {
            Logger.Info($"SERVER: Broadcasting ServerOpponentUpdates to {_users.Count} user(s).");
            for (var i = 0; i < _users.Count; i++)
            {
                _users[i].IsChangedByServer = true;
                _users[i].IsChangedByClient = false;
                if (makeCardsReverse)
                {
                    _newUser = _users[i].DeepClone();
                    for (var j = 0; j < _newUser.Cards.Count; j++)
                        _newUser.Cards[j] = _deck.Reverse;
                }

                var message = new Message(_key, "ServerOpponentUpdate", makeCardsReverse ? _newUser : _users[i]);
                _writer.Put(message.Json());
                _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered, _server.GetPeerById(i));
                _writer.Reset();
            }
        }

        private void BroadCastTableUpdate()
        {
            Logger.Info($"SERVER: Broadcasting TableUpdate to {_users.Count} user(s).");
            _table.HasChanged = true;
            var broadCastTable = _table.DeepClone();
            for (var i = 0; i < 5 - GameLogic.ActiveTableCardsAmount; i++)
            {
                broadCastTable.Cards[4 - i] = _deck.Reverse;
            }

            var message = new Message(_key, "ServerTableUpdate", broadCastTable);
            _writer.Put(message.Json());
            _server.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }

        public void ProcessGameLogic(bool first = false)
        {
            GameLogic.ProcessRound();
            
            // Broadcast Table if changed
            if ((GameLogic.TableHasChanged || first) && _deck != null)
            {
                BroadCastTableUpdate();
            }

            // Broadcast Client/Opponent Updates if changed
            if (!GameLogic.UsersHaveChanged && !first) return;
            BroadCastServerClientUpdates();
            BroadCastServerOpponentUpdates(!GameLogic.RoundEnded);
        }
    }
}