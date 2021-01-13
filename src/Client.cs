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
                        card.Texture = Deck.Reverse.Texture;
                    }
                    else
                    {
                        card.Texture = Deck.Cards.First(c =>
                            c.CardType == card.CardType && c.CardCategory == card.CardCategory).Texture;
                    }
                } while (card.Texture == null);
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
}