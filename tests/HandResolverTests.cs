using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Poker;

namespace Tests
{
    public class HandResolverTests
    {
        public HandResolver Resolver;
        public List<Card> Hand1;
        public List<Card> Hand2;
        public List<Card> Hand3;
        
        [SetUp]
        public void Setup()
        {
            Hand1 = new List<Card>
            {
                new Card(Category.Clubs, Type.Ace, null),
                new Card(Category.Clubs, Type.King, null),
                new Card(Category.Clubs, Type.Queen, null),
                new Card(Category.Clubs, Type.Jack, null),
                new Card(Category.Clubs, Type.Ten, null),
                new Card(Category.Clubs, Type.Nine, null),
                new Card(Category.Clubs, Type.Eight, null),
            };

            Hand2 = new List<Card>
            {
                new Card(Category.Clubs, Type.Ace, null),
                new Card(Category.Diamonds, Type.Ace, null),
                new Card(Category.Hearts, Type.Ace, null),
                new Card(Category.Spades, Type.Ace, null),
                new Card(Category.Clubs, Type.Two, null),
                new Card(Category.Clubs, Type.Three, null),
                new Card(Category.Clubs, Type.Four, null),
            };

            Hand3 = new List<Card>
            {
                new Card(Category.Clubs, Type.Ace, null),
                new Card(Category.Diamonds, Type.Ace, null),
                new Card(Category.Hearts, Type.King, null),
                new Card(Category.Spades, Type.King, null),
                new Card(Category.Clubs, Type.King, null),
                new Card(Category.Hearts, Type.Ace, null),
                new Card(Category.Clubs, Type.Four, null),
            };
        }
        
        [Test]
        public void HighCardTest1()
        {
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForHighCard();
            
            Assert.AreEqual(Hand.HighCard, Resolver.BestHand);
            Assert.AreEqual(1, Resolver.BestHandCards.Count);
            Assert.AreEqual(4, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void PairTest1()
        {
            // No pair expected
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForPair();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }

        [Test]
        public void PairTest2()
        {
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.CheckForPair();
            
            Assert.AreEqual(Hand.Pair, Resolver.BestHand);
            Assert.AreEqual(2, Resolver.BestHandCards.Count);
            Assert.AreEqual(3, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void TwoPairTest1()
        {
            // No two pair expected
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForTwoPair();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }

        [Test]
        public void TwoPairTest2()
        {
            Resolver = new HandResolver(Hand3.Take(5).ToList(), Hand3.Skip(5).ToList());
            Resolver.CheckForTwoPair();
            
            Assert.AreEqual(Hand.TwoPair, Resolver.BestHand);
            Assert.AreEqual(4, Resolver.BestHandCards.Count);
            Assert.AreEqual(1, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void ThreeOfAKindTest1()
        {
            // No three of a kind expected
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForThreeOfAKind();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }
        
        [Test]
        public void ThreeOfAKindTest2()
        {
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.CheckForThreeOfAKind();
            
            Assert.AreEqual(Hand.ThreeOfAKind, Resolver.BestHand);
            Assert.AreEqual(3, Resolver.BestHandCards.Count);
            Assert.AreEqual(2, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void StraightTest1()
        {
            // No straight expected
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.CheckForStraight();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }
        
        [Test]
        public void StraightTest2()
        {
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForStraight();
            
            Assert.AreEqual(Hand.Straight, Resolver.BestHand);
            Assert.AreEqual(5, Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void FlushTest1()
        {
            // No flush expected
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.CheckForFlush();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }
        
        [Test]
        public void FlushTest2()
        {
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForFlush();
            
            Assert.AreEqual(Hand.Flush, Resolver.BestHand);
            Assert.AreEqual(5, Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void FullHouseTest1()
        {
            // No full house expected
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.CheckForFullHouse();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }
        
        [Test]
        public void FullHouseTest2()
        {
            Resolver = new HandResolver(Hand3.Take(5).ToList(), Hand3.Skip(5).ToList());
            Resolver.CheckForFullHouse();
            
            Assert.AreEqual(Hand.FullHouse, Resolver.BestHand);
            Assert.AreEqual(5, Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void FourOfAKindTest1()
        {
            // No four of a kind expected
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForFourOfAKind();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }
        
        [Test]
        public void FourOfAKindTest2()
        {
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.CheckForFourOfAKind();
            
            Assert.AreEqual(Hand.FourOfAKind, Resolver.BestHand);
            Assert.AreEqual(4, Resolver.BestHandCards.Count);
            Assert.AreEqual(1, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void StraightFlushTest1()
        {
            // No straight flush expected
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.CheckForStraightFlush();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }
        
        [Test]
        public void StraightFlushTest2()
        {
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForStraightFlush();
            
            Assert.AreEqual(Hand.StraightFlush, Resolver.BestHand);
            Assert.AreEqual(5, Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void RoyalFlushTest1()
        {
            // No royal flush expected
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.CheckForRoyalFlush();
            
            Assert.AreEqual(Hand.None, Resolver.BestHand);
            Assert.AreEqual(0,Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
        }
        
        [Test]
        public void RoyalFlushTest2()
        {
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.CheckForRoyalFlush();
            
            Assert.AreEqual(Hand.RoyalFlush, Resolver.BestHand);
            Assert.AreEqual(5, Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }

        [Test]
        public void DetermineBestHandTest1()
        {
            Resolver = new HandResolver(Hand1.Take(5).ToList(), Hand1.Skip(5).ToList());
            Resolver.DetermineBestHand();
            
            Assert.AreEqual(Hand.RoyalFlush, Resolver.BestHand);
            Assert.AreEqual(5, Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void DetermineBestHandTest2()
        {
            Resolver = new HandResolver(Hand2.Take(5).ToList(), Hand2.Skip(5).ToList());
            Resolver.DetermineBestHand();
            
            Assert.AreEqual(Hand.FourOfAKind, Resolver.BestHand);
            Assert.AreEqual(4, Resolver.BestHandCards.Count);
            Assert.AreEqual(1, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
        
        [Test]
        public void DetermineBestHandTest3()
        {
            Resolver = new HandResolver(Hand3.Take(5).ToList(), Hand3.Skip(5).ToList());
            Resolver.DetermineBestHand();
            
            Assert.AreEqual(Hand.FullHouse, Resolver.BestHand);
            Assert.AreEqual(5, Resolver.BestHandCards.Count);
            Assert.AreEqual(0, Resolver.Kickers.Count);
            Assert.AreEqual(Type.Ace, Resolver.BestHandCards[0].CardType);
        }
    }
}
