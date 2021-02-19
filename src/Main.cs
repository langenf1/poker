using System;


namespace Poker
{
    
    public class Program
    {
        
        [STAThread]
        private static void Main(string[] args)
        {
            using var game = new PokerGame(args[0] == "true");
            game.Run();
        }
    }
}
