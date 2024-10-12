namespace MiniCompiler
{
    public enum TokenType { }

    public class Token
    {
        public string? text { get; set; }
        public int startPosition { get; set; }
        public int endPosition { get; set; }
        public TokenType tokenType;
    }

    public class Lexer
    {
        private bool m_writeToFile;
        private String m_fileName = "";
        private String m_source = "";
        private int m_srcPtr = -1;
        private List<Token>? m_tokens = null;

        public Lexer()
        {
            Console.Error.WriteLine("ERROR: Lexer default constructor cannot be caleld");
            System.Environment.Exit(1);
        }

        public Lexer(String fileName, bool writeToFile)
        {
            m_fileName = fileName;
            m_writeToFile = writeToFile;
            m_tokens = new List<Token>();
            m_srcPtr = 1;
            readSourceFile();
        }

        private void readSourceFile()
        {
            try
            {
                m_source = File.ReadAllText(m_fileName + ".min");
            }
            catch (FileNotFoundException e)
            {
                Console.Error.Write(e.ToString());
            }
        }

        public void debugPrintSource()
        {
            Console.WriteLine("DEBUG:");
            foreach (char c in m_source)
            {
                Console.Write(c);
            }
        }

        private char getChar() { }

        public void lex() { }
    }
}
