using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker
{
    public class GameServerLogic
    {
        private Table _table;
        private Deck _deck;
        private List<User> _users;
        private HandResolver _handResolver1;
        private HandResolver _handResolver2;
        private bool _decidedUsingKickers;

        public int ActiveTableCardsAmount;
        public bool RoundEnded;
        public bool UsersHaveChanged;
        public bool TableHasChanged;
        private int _defaultCash;

        // If you change these, things will break (so don't)
        private const int MaxPlayers = 2;
        private const int UserCardsAmount = 2;

        public GameServerLogic(ref Table table, ref Deck deck, ref List<User> users, int defaultCash)
        {
            _table = table;
            _deck = deck;
            _users = users;
            _defaultCash = defaultCash;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ProcessRound()
        {
            TableHasChanged = false;
            UsersHaveChanged = false;
            if (RoundEnded) return;
            
            ProcessBets();

            if (AnyUserHasFolded())
            {
                RoundEnded = true;
                ActiveTableCardsAmount = 5;
            }
            else
            {
                if (_users.Count != MaxPlayers || !_users.All(user => user.HasBetted) ||
                    !_users.All(user => user.BetIsProcessed.All(bet => bet)) ||
                    _users[0].Bets.Sum() != _users[1].Bets.Sum()) return;
            }

            AddActiveTableCards();
            

            if (RoundEnded)
            {
                TableHasChanged = true;
                UsersHaveChanged = true;

                if (_users.All(user => !user.HasLostRound))
                {
                    DetermineRoundLoser();
                    if (_users.All(user => !user.HasLostRound))
                    {
                        Logger.Info("The round was a tie, splitting the pot.");
                        _users[0].Cash += _table.Pot / 2;
                        _users[1].Cash += _table.Pot / 2;
                        _table.Pot = 0;
                    }
                    else
                    {
                        if (_users[0].HasLostRound)
                        {
                            Logger.Info($"{_users[1].Name} won the round with a {_handResolver2.BestHand} against " +
                                        $"{_users[0].Name}'s {_handResolver1.BestHand}" +
                                        (_decidedUsingKickers ? " (decided with kickers)" : ""));
                            _users[1].Cash += _table.Pot;
                        }
                        else
                        {
                            Logger.Info($"{_users[0].Name} won the round with a {_handResolver1.BestHand} against " +
                                        $"{_users[1].Name}'s {_handResolver2.BestHand}" +
                                        (_decidedUsingKickers ? " (decided with kickers)" : ""));
                            _users[0].Cash += _table.Pot;
                        }
                    }
                }
                else
                {
                    if (_users[0].HasLostRound)
                    {
                        Logger.Info($"{_users[0].Name} has folded so {_users[1].Name} wins the round");
                        _users[1].Cash += _table.Pot;
                    }
                    else
                    {
                        Logger.Info($"{_users[1].Name} has folded so {_users[0].Name} wins the round");
                        _users[0].Cash += _table.Pot;
                    }
                }
                _table.Pot = 0;
            }

            if (AnyUserHasLostGame())
            {
                ResetGame();
                TableHasChanged = true;
                UsersHaveChanged = true;
            }
        }
        
        /// <summary>
        /// Processes the bets from each user.
        /// </summary>
        private void ProcessBets()
        {
            foreach (var user in _users.Where(user => user.HasBetted && !user.BetIsProcessed[^1]))
            {
                user.HasAddedBet = false;
                _table.Pot += user.Bets[^1];
                user.Cash -= user.Bets[^1];
                user.BetIsProcessed[^1] = true;
                UsersHaveChanged = true;
                TableHasChanged = true;
            }
        }

        /// <summary>
        /// Processes the changes to the table (e.g. add cards)
        /// </summary>
        private void AddActiveTableCards()
        {
            if (ActiveTableCardsAmount < 5)
                ActiveTableCardsAmount = ActiveTableCardsAmount == 0 ? 3 : ++ActiveTableCardsAmount;

            else if (ActiveTableCardsAmount == 5)
            {
                RoundEnded = true;
            }

            TableHasChanged = true;

            ResetUserBets();
        }

        /// <summary>
        /// Resets the user bets.
        /// </summary>
        private void ResetUserBets()
        {
            foreach (var user in _users)
            {
                user.HasBetted = false;
                user.BetIsProcessed = new List<bool>();
                user.Bets = new List<int>();
            }
        }

        private void DetermineRoundLoser()
        {
            _decidedUsingKickers = false;
            _handResolver1 = new HandResolver(_users[0].Cards, _table.Cards);
            _handResolver2 = new HandResolver(_users[1].Cards, _table.Cards);
            _handResolver1.DetermineBestHand();
            _handResolver2.DetermineBestHand();

            // Determine better hand
            _users[0].HasLostRound = _handResolver1.BestHand < _handResolver2.BestHand;
            _users[1].HasLostRound = _handResolver1.BestHand > _handResolver2.BestHand;

            if (_users.Any(user => user.HasLostRound)) return;

            // If hands are equal determine which hand had the higher card(s)
            switch (_handResolver1.BestHand)
            {
                case Hand.TwoPair:
                    _users[0].HasLostRound = _handResolver1.BestHandCards[0].CardType <
                                             _handResolver2.BestHandCards[0].CardType;
                    _users[1].HasLostRound = _handResolver1.BestHandCards[0].CardType >
                                             _handResolver2.BestHandCards[0].CardType;
                    if (_users.All(user => !user.HasLostRound))
                    {
                        _users[0].HasLostRound = _handResolver1.BestHandCards[2].CardType <
                                                 _handResolver2.BestHandCards[2].CardType;
                        _users[1].HasLostRound = _handResolver1.BestHandCards[2].CardType >
                                                 _handResolver2.BestHandCards[2].CardType;
                    }

                    break;

                case Hand.RoyalFlush:
                    break;

                default:
                    _users[0].HasLostRound = _handResolver1.BestHandCards[0].CardType <
                                             _handResolver2.BestHandCards[0].CardType;
                    _users[1].HasLostRound = _handResolver1.BestHandCards[0].CardType >
                                             _handResolver2.BestHandCards[0].CardType;
                    break;
            }

            if (_users.Any(user => user.HasLostRound) && _handResolver1.BestHand != Hand.RoyalFlush) return;

            // If hands are still equal decide it using kicker cards
            DecideWithKickers(ref _handResolver1, ref _handResolver2);
            _decidedUsingKickers = true;
        }

        private void DecideWithKickers(ref HandResolver handResolver1, ref HandResolver handResolver2)
        {
            for (var i = 0; i < handResolver1.Kickers.Count; i++)
            {
                _users[0].HasLostRound = handResolver1.Kickers[i].CardType < handResolver2.Kickers[i].CardType;
                _users[1].HasLostRound = handResolver1.Kickers[i].CardType > handResolver2.Kickers[i].CardType;

                if (_users.Any(user => user.HasLostRound))
                    return;
            }
        }

        /// <summary>
        /// Checks if any users have lost the game.
        /// </summary>
        private bool AnyUserHasLostGame()
        {
            foreach (var user in _users.Where(user => user.HasLostRound && user.Cash == 0))
                user.HasLostGame = true;

            return _users.Any(user => user.HasLostGame);
        }

        private bool AnyUserHasFolded()
        {
            if (!_users.Any(user => user.HasFolded)) return false;
            {
                _users.First(user => user.HasFolded).HasLostRound = true;
                return true;
            }
        }

        /// <summary>
        /// Resets the round and gives the players new cards.
        /// </summary>
        private void ResetRound()
        {
            ActiveTableCardsAmount = 0;
            _deck.Shuffle();
            foreach (var user in _users)
            {
                user.Bets = new List<int>();
                user.HasAddedBet = false;
                user.HasBetted = false;
                user.HasFolded = false;
                user.HasLostRound = false;
                user.HasLostGame = false;
                user.IsChangedByServer = true;
                user.IsChangedByClient = false;
                user.Cards = new List<Card>();
            }

            for (var i = 0; i < _users.Count; i++)
            {
                for (var j = 0; j < UserCardsAmount; j++)
                {
                    _users[i].Cards.Add(_deck.Cards[i * UserCardsAmount + j]);
                }
            }

            RoundEnded = false;
        }

        /// <summary>
        /// Resets the entire game.
        /// </summary>
        private void ResetGame()
        {
            ResetRound();
            foreach (var user in _users)
            {
                user.Cash = _defaultCash;
            }
        }
    }
}