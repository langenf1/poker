using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;

namespace Poker
{
    public class Client
    {
        EventBasedNetListener _listener = new EventBasedNetListener();
        NetDataWriter _writer = new NetDataWriter();
        private NetManager _client;
        public Deck Deck;
        public User Opponent { get; set; }
        public User User { get; set; }
        public Table Table { get; set; }
        private Message _message;

        public Client(string clientKey, string password, string ip = "localhost", int port = 9050, string username="User")
        {
            User = new User(clientKey, username);
            Opponent = new User();
            Table = new Table();
            _client = new NetManager(_listener);
            _client.Start();
            _client.Connect(ip, port, password);
            AddNewNetworkConnectionEventListener();
        }

        /// <summary>
        /// Adds an event listener for new network connections to the listener instance.
        /// </summary>
        private void AddNewNetworkConnectionEventListener()
        {
            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                try
                {
                    _message = JsonConvert.DeserializeObject<Message>(dataReader.GetString());
                }
                catch (Exception e)
                {
                    Logger.Debug(string.Concat("CLIENT: Json conversion failed: ", e.ToString()));
                }

                if (_message.Type == "ClientUpdate") return;
                
                Logger.Debug($"CLIENT: Received {_message.Type} with Object: {_message.Object}");

                switch (_message.Type)
                {
                    // Client Update from Opponent re-broadcasted by Server
                    case "ServerOpponentUpdate":
                    {
                        Opponent = JsonConvert.DeserializeObject<User>(_message.Object);
                        Opponent.Cards = InsertCardTextures(Opponent.Cards);
                        Logger.Debug("CLIENT: Opponent Update Successful.");
                        
                        break;
                    }

                    // User Client update from Server
                    case "ServerClientUpdate":
                    {
                        User = JsonConvert.DeserializeObject<User>(_message.Object);
                        User.Cards = InsertCardTextures(User.Cards);

                        Logger.Debug("CLIENT: Client User Update Successful.");
                        break;
                    }

                    // Table update from Server
                    case "ServerTableUpdate":
                    {
                        Table = JsonConvert.DeserializeObject<Table>(_message.Object);
                        Table.Cards = InsertCardTextures(Table.Cards);

                        Logger.Debug("CLIENT: Table Update Successful.");
                        break;
                    }
                }
            };
        }

        /// <summary>
        /// Inserts textures into cards that don't have textures assigned (e.g. because of JSON serialization).
        /// </summary>
        /// <param name="cards">List of cards without textures.</param>
        /// <returns>The list of cards with the textures taken from Client.Deck.</returns>
        private List<Card> InsertCardTextures(List<Card> cards)
        {
            foreach (var card in cards)
            {
                // Do-while loop because for some reason the texture assignment fails sometimes.
                do
                {
                    if (card.CardCategory == Category.Reverse) 
                        card.Texture = Deck.Reverse.Texture;
                    else 
                        card.Texture = Deck.Cards.First(c =>
                            c.CardType == card.CardType && c.CardCategory == card.CardCategory).Texture;
                    
                } while (card.Texture == null);
            }
            return cards;
        }

        /// <summary>
        /// Polls updates from server/opponent.
        /// </summary>
        public void PollEvents()
        {
            while (true)
            {
                _client.PollEvents();
                Thread.Sleep(15);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        /// Stops the client connection to the server.
        /// </summary>
        public void Stop()
        {
            _client.Stop();
        }

        /// <summary>
        /// Broadcasts a Message object to the Server & Opponent.
        /// </summary>
        /// <param name="message">Message to broadcast.</param>
        private void Broadcast(Message message)
        {
            Logger.Debug($"CLIENT: Broadcasting: {message.Type}");
            _writer.Put(message.Json());
            _client.SendToAll(_writer, DeliveryMethod.ReliableOrdered);
            _writer.Reset();
        }

        /// <summary>
        /// Inform the server about changes to the client.
        /// </summary>
        public void BroadcastClientUpdate()
        {
            Broadcast(new Message(User.Key, "ClientUpdate", User));
            User.IsChangedByClient = false;
        }
    }
}