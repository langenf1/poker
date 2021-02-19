using System;
using System.Collections.Generic;
using System.Linq;

namespace Poker
{
    public enum Hand
    {
        None = 0,
        HighCard = 1,
        Pair = 2,
        TwoPair = 3,
        ThreeOfAKind = 4,
        Straight = 5,
        Flush = 6,
        FullHouse = 7,
        FourOfAKind = 8,
        StraightFlush = 9,
        RoyalFlush = 10
    }
    
    public class HandResolver
    {
        private List<Card> _allCards;
        public Hand BestHand = Hand.None;
        public List<Card> BestHandCards = new List<Card>();
        public List<Card> Kickers = new List<Card>();
        private Dictionary<Type, int> _allCardsTypes;
        private Dictionary<Category, int> _allCardsCategories;
        
        public HandResolver(List<Card> userCards, IEnumerable<Card> tableCards)
        {
            userCards.AddRange(tableCards);
            _allCards = userCards.OrderByDescending(card => (int) card.CardType).ToList();
            _allCardsTypes = new Dictionary<Type, int>();
            _allCardsCategories = new Dictionary<Category, int>();
            FillCardDicts();
        }
        
        public void DetermineBestHand()
        {
            CheckForHighCard();
            CheckForPair();
            CheckForTwoPair();
            CheckForThreeOfAKind();
            CheckForStraight();
            CheckForFlush();
            CheckForFullHouse();
            CheckForFourOfAKind();
            CheckForStraightFlush();
            CheckForRoyalFlush();
            
            BestHandCards = BestHandCards.OrderByDescending(card => card.CardType).ToList();
            Kickers = Kickers.OrderByDescending(card => card.CardType).ToList();
        }

        private void FillCardDicts()
        {
            foreach (var type in Enum.GetNames(typeof(Type)))
            {
                _allCardsTypes.Add((Type) Enum.Parse(typeof(Type), type, true), 0);
            } 
            
            foreach (var category in Enum.GetNames(typeof(Category)))
            {
                _allCardsCategories.Add((Category) Enum.Parse(typeof(Category), category, true), 0);
            } 
            
            foreach (var card in _allCards)
            {
                _allCardsTypes[card.CardType]++;
                _allCardsCategories[card.CardCategory]++;
            }
        }
        
        public void CheckForHighCard()
        {
            BestHand = Hand.HighCard;
            BestHandCards = _allCards.Take(1).ToList();
            Kickers = _allCards.Skip(1).Take(4).ToList();
        }

        public void CheckForPair()
        {
            if (_allCardsTypes.Count(typeCount => typeCount.Value >= 2) < 1) return;
            
            BestHand = Hand.Pair;
            BestHandCards = _allCards.Where(card => card.CardType == _allCardsTypes.Last(type => type.Value >= 2).Key).Take(2).ToList();
            Kickers = _allCards.Except(BestHandCards).Take(3).ToList();
        }
        
        public void CheckForTwoPair()
        {
            if (_allCardsTypes.Count(typeCount => typeCount.Value >= 2) < 2) return;
            
            BestHand = Hand.TwoPair;
            BestHandCards = _allCards.Where(card => card.CardType == _allCardsTypes.Last(type => type.Value >= 2).Key).Take(2).ToList();
            BestHandCards.AddRange(_allCards.Where(card => card.CardType == 
                                                          _allCardsTypes.Last(type => type.Value >= 2 && type.Key != BestHandCards[0].CardType).Key).Take(2).ToList());
            Kickers = _allCards.Except(BestHandCards).Take(1).ToList();
        }
        
        public void CheckForThreeOfAKind()
        {
            if (_allCardsTypes.All(typeCount => typeCount.Value < 3)) return;
            
            BestHand = Hand.ThreeOfAKind;
            BestHandCards = _allCards.Where(card => card.CardType == _allCardsTypes.Last(type => type.Value >= 3).Key).Take(3).ToList();
            Kickers = _allCards.Except(BestHandCards).Take(2).ToList();
        }
        
        public void CheckForStraight(Category flushCategory = Category.Reverse)
        {
            var streaks = new List<List<Card>> {new List<Card>()};
            var straight = false;

            // Check for any straights
            foreach (Type type in Enum.GetValues(typeof(Type)))
            {
                if (_allCardsTypes[type] >= 1)
                    streaks[^1].Add(flushCategory == Category.Reverse
                        ? _allCards.First(card => card.CardType == type)
                        : _allCards.First(card => card.CardType == type && card.CardCategory == flushCategory));
                else
                    streaks.Add(new List<Card>());

                // Check for straight starting with Ace as 1
                if (streaks.Count == 4 && type == Type.Five && _allCardsTypes[Type.Ace] > 0)
                {
                    streaks[^1].Insert(0, _allCards.First(card => card.CardType == Type.Ace));
                }

                if (streaks.All(streak => streak.Count < 5)) continue;
                straight = true;
            }
            
            if (!straight) return;
            
            // Take highest straight
            BestHand = Hand.Straight;
            BestHandCards = streaks.Where(streak => streak.Count > 0)
                .OrderByDescending(streak => streak[^1].CardType).First()
                .OrderByDescending(card => card.CardType).Take(5).ToList();
            Kickers.Clear();
        }
        
        public void CheckForFlush()
        {
            if (_allCardsCategories.All(categoryCount => categoryCount.Value < 5)) return;
            
            BestHand = Hand.Flush;
            BestHandCards = _allCards.Where(card => card.CardCategory == _allCardsCategories.First(category => category.Value >= 5).Key).ToList();
            BestHandCards = BestHandCards.OrderByDescending(card => (int) card.CardType).Take(5).ToList();
            Kickers.Clear();
        }

        public void CheckForFullHouse()
        {
            var trips = _allCardsTypes.Where(typeCount => typeCount.Value >= 3).Take(1).ToList();
            if (trips.Count == 0) return;
            
            var dubsOrTrips = _allCardsTypes.Where(typeCount => typeCount.Value >= 2 && typeCount.Key != trips[0].Key).ToList();
            if (dubsOrTrips.Count == 0) return;
            
            BestHand = Hand.FullHouse;
            BestHandCards = _allCards.FindAll(card => card.CardType == trips[0].Key);
            BestHandCards.AddRange(_allCards.FindAll(card => card.CardType == dubsOrTrips[0].Key).Take(2));
            BestHandCards = BestHandCards.OrderByDescending(card => card.CardType).ToList();
            Kickers.Clear();
        }

        public void CheckForFourOfAKind()
        {
            if (_allCardsTypes.All(typeCount => typeCount.Value < 4)) return;
            
            BestHand = Hand.FourOfAKind;
            BestHandCards = _allCards.Where(card => card.CardType == _allCardsTypes.First(type => type.Value == 4).Key).ToList();
            Kickers = _allCards.Except(BestHandCards).Take(1).ToList();
        }

        public void CheckForStraightFlush()
        {
            CheckForFlush();
            if (BestHand != Hand.Flush) return;
            
            CheckForStraight(BestHandCards[0].CardCategory);
            if (BestHand != Hand.Straight) return;
            
            BestHand = Hand.StraightFlush;
        }

        public void CheckForRoyalFlush()
        {
            CheckForStraightFlush();
            if (BestHand != Hand.StraightFlush) return;
            
            if (BestHandCards.All(card => card.CardType != Type.Ace) || 
                BestHandCards.All(card => card.CardType != Type.Ten)) return;

            BestHand = Hand.RoyalFlush;
        }
    }
}