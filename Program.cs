namespace MiniCompiler
{
    public class Program
    {
        private static String m_fileName = "";

        private static void printUsage()
        {
            Console.Error.WriteLine("ERROR: try dotnet run <file name>");
            Console.Error.WriteLine("or try: MiniCompiler <file name>");
            Console.Error.WriteLine("WARN: expected file to compile but got nothing");
            System.Environment.Exit(1);
        }

        private static void verifyFileExtension()
        {
            if (!m_fileName.EndsWith(".min"))
            {
                Console.Error.WriteLine("ERROR: .min file extension required");
                System.Environment.Exit(1);
            }
        }

        private static void extractFileName()
        {
            // ".min" is 4 chars
            m_fileName = m_fileName.Remove(m_fileName.Length - 4);
        }

        public static void Main(String[] args)
        {
            if (args.Length != 1)
            {
                printUsage();
            }
            m_fileName = args[0].Trim();

            verifyFileExtension();
            // don't reach here unless file extension is correct
            extractFileName();

            Lexer lexer = new Lexer(m_fileName, true);
            lexer.lex();
        }
    }
}
