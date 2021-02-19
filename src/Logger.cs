using System;

namespace Poker
{
    public static class Logger
    {
        public static void WriteLine(string message, int level = 0)
        {
            var levelString = "";
            switch (level)
            {
                case 0:
                    levelString = "DEBUG: ";
                    break;
                case 1:
                    levelString = "INFO: ";
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case 2:
                    levelString = "WARNING: ";
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case 3:
                    levelString = "ERROR: ";
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case 4:
                    levelString = "FATAL: ";
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine(String.Concat(DateTime.Now.ToString("HH:mm:ss"), " ", levelString, message));
            Console.ResetColor();
        }
        
        public static void Debug(string message)
        {
            WriteLine(message);
        }
        
        public static void Info(string message)
        {
            WriteLine(message, 1);
        }
        
        public static void Warning(string message)
        {
            WriteLine(message, 2);
        }
        
        public static void Error(string message)
        {
            WriteLine(message, 3);
        }
        
        public static void Fatal(string message)
        {
            WriteLine(message, 4);
        }
    }
}