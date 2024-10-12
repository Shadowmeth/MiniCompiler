namespace MiniCompiler
{
    public class ErrorHandler
    {
        public static void Error(string errorMsg)
        {
            ConsoleColor originalConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("ERROR: " + errorMsg);
            Console.ForegroundColor = originalConsoleColor;
            System.Environment.Exit(1);
        }
    }
}
