namespace MiniCompiler
{
    public enum TokenType
    {
        KEYWORD,
        STRING_LIT,
        IDENTIFIER,
        NUMBER,

        LEFT_PAREN,
        RIGHT_PAREN,
        LEFT_BRACE,
        RIGHT_BRACE,

        EQUAL,
        EQUAL_EQUAL,
        LESS_EQUAL,
        GREATER_EQUAL,
        NOT_EQUAL,
        LESS_THAN,
        GREATER_THAN,

        LOGICAL_AND,
        LOGICAL_OR,

        SEMICOLON,
        COLON,
        ADD,
        SUB,
        MUL,
        DIV,

        EOF, // only token with no span and text
    }

    public class Token
    {
        public string? text { get; set; }
        public int startPosition { get; set; }
        public int endPosition { get; set; }
        public int line { get; set; }
        public TokenType tokenType;

        public Token(string text, int startPosition, int endPosition, int line, TokenType tokenType)
        {
            this.text = text;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.line = line;
            this.tokenType = tokenType;
        }

        public override string ToString()
        {
            return $"at line: {line}, span: {startPosition}:{endPosition}, token text: \"{text}\"";
        }

    }

    // NOTE: A lot of indexes start at 0. Need to save them as +1
    // for proper error reporting
    public class Lexer
    {
        private HashSet<string>? m_keyWords = null;
        private bool m_writeToFile;
        private String m_fileName = ""; // this is without extension
        private String m_source = "";
        private int m_srcPtr = -1;
        private List<Token>? m_tokens = null;
        private bool m_inString = false;
        private int m_line = 0;

        public Lexer()
        {
            ErrorHandler.Error("Lexer default constructor cannot be called");
            // Console.Error.WriteLine("ERROR: Lexer default constructor cannot be caleld");
            // System.Environment.Exit(1);
        }

        private void registerKeyWord(string keyword)
        {
            if (m_keyWords != null && !m_keyWords.Contains(keyword))
            {
                m_keyWords.Add(keyword);
            }
        }

        private bool isKeyWord(string tokenText)
        {
            return m_keyWords != null && m_keyWords.Contains(tokenText);
        }

        public Lexer(String fileName, bool writeToFile)
        {
            m_fileName = fileName;
            m_writeToFile = writeToFile;
            m_tokens = new List<Token>();
            m_srcPtr = 0;
            readSourceFile();
            if (m_source.Length == 0)
            {
                ErrorHandler.Error("source file is empty!");
            }

            m_keyWords = new HashSet<string>();
            registerKeyWord("int");
            registerKeyWord("bool");
            registerKeyWord("string");
            registerKeyWord("struct");
            registerKeyWord("if");
            registerKeyWord("else");
            registerKeyWord("while");
            registerKeyWord("true");
            registerKeyWord("false");
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
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("DEBUG:");
            foreach (char c in m_source)
            {
                Console.Write(c);
            }
            Console.ForegroundColor = originalColor;
        }

        public void debugPrintTokens()
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("DEBUG:");

            foreach (Token t in m_tokens)
            {
                Console.WriteLine(t.ToString());
            }

            Console.ForegroundColor = originalColor;
        }

        public int debugTokenCount()
        {
            return m_tokens.Count;
        } 

        private char getChar()
        {
            if (m_srcPtr <= m_source.Length - 1)
            {
                return m_source.ElementAt(m_srcPtr);
            }
            else
            {
                return '\0';
            }
        }

        private char peekChar()
        {
            if (m_srcPtr <= m_source.Length - 2)
            {
                return m_source.ElementAt(m_srcPtr + 1);
            }
            else
            {
                return '\0';
            }
        }

        private void skipWhiteSpace()
        {
            char c = getChar();
            // only skip these chars when not inside a string literal
            while ((c == ' ' || c == '\t' || c == '\r') && !m_inString)
            {
                m_srcPtr++;
                c = getChar();
            }
        }

        private bool isAlphaNumeric(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c == '_');
        }

        private bool isDigit(char c)
        {
            return (c >= '0') && (c <= '9');
        }

        private void registerCharToken(string text, TokenType type)
        {
            Token t = new Token(text, m_srcPtr + 1, m_srcPtr + 1, m_line + 1, type);
            m_tokens.Add(t);
        }

        public void lex()
        {
            while (true)
            {
                skipWhiteSpace();
                char c = getChar();
                
                if (c == '\0')
                {
                    Token t = new Token("$", -1, -1, -1, TokenType.EOF);
                    m_tokens.Add(t);
                    break;
                }

                if (c == '\n')
                {
                    m_line++;
                    m_srcPtr++;
                    continue;
                }
                else if (isAlphaNumeric(c))
                {
                    // possible identifier or a keyword
                    int tokenStart = m_srcPtr + 1;
                    string tokenText = "";
                    while (isAlphaNumeric(c))
                    {
                        tokenText += c;
                        m_srcPtr++;
                        c = getChar();
                    }
                    int tokenEnd = m_srcPtr;
                    tokenText = tokenText.Trim();
                    if (isKeyWord(tokenText))
                    {
                        Token t = new Token(
                            tokenText,
                            tokenStart,
                            tokenEnd,
                            m_line + 1,
                            TokenType.KEYWORD
                        );
                        m_tokens.Add(t);
                    }
                    else
                    {
                        Token t = new Token(
                            tokenText,
                            tokenStart,
                            tokenEnd,
                            m_line + 1,
                            TokenType.IDENTIFIER
                        );
                        m_tokens.Add(t);
                    }
                }
                else if (isDigit(c))
                {
                    string number = "";
                    int tokenStart = m_srcPtr + 1;
                    while (isDigit(c)) 
                    {
                        number += c;
                        m_srcPtr++;
                        c = getChar();
                    }
                    int tokenEnd = m_srcPtr;
                    Token t = new Token(number, tokenStart, tokenEnd, m_line + 1, TokenType.NUMBER);
                    m_tokens.Add(t);
                }
                else if (c == '"')
                {
                    // string literal
                    string stringLit = "";
                    m_inString = true;
                    int tokenStart = m_srcPtr + 1;
                    m_srcPtr++;
                    c = getChar();
                    while (c != '"')
                    {
                        stringLit += c;
                        m_srcPtr++;
                        c = getChar();
                        if (c == '\n')
                        {
                            int line = m_line + 1;
                            string errorMsg =
                                "In file: "
                                + m_fileName
                                + ".min string literal started at ("
                                + line.ToString()
                                + ":"
                                + tokenStart.ToString()
                                + ") but did't finish";
                            ErrorHandler.Error(errorMsg);
                        }
                    }
                    m_inString = false;
                    int tokenEnd = m_srcPtr + 1;
                    Token t = new Token(
                        stringLit,
                        tokenStart,
                        tokenEnd,
                        m_line + 1,
                        TokenType.STRING_LIT
                    );
                    m_srcPtr++;
                }
                else if (c == '/')
                {
                    // this can be DIV or single line comment
                    char peek = peekChar();
                    if (peek == '/')
                    {
                        // single line comment, ignore text until next '\n'
                        while (peek != '\n')
                        {
                            m_srcPtr++;
                            peek = getChar();
                        }
                    }
                    else
                    {
                        registerCharToken("/", TokenType.DIV);
                        m_srcPtr++;
                    }
                }
                else if (c == '+')
                {
                    registerCharToken("+", TokenType.ADD);
                    m_srcPtr++;
                }
                else if (c == '*')
                {
                    registerCharToken("*", TokenType.MUL);
                    m_srcPtr++;
                }
                else if (c == '-')
                {
                    registerCharToken("-", TokenType.SUB);
                    m_srcPtr++;
                }
                else if (c == '(')
                {
                    registerCharToken("(", TokenType.LEFT_PAREN);
                    m_srcPtr++;
                }
                else if (c == ')')
                {
                    registerCharToken(")", TokenType.RIGHT_PAREN);
                    m_srcPtr++;
                }
                else if (c == '{')
                {
                    registerCharToken("{", TokenType.LEFT_BRACE);
                    m_srcPtr++;
                }
                else if (c == '}')
                {
                    registerCharToken("}", TokenType.RIGHT_BRACE);
                    m_srcPtr++;
                }
                else if (c == ';')
                {
                    registerCharToken(";", TokenType.SEMICOLON);
                    m_srcPtr++;
                }
                else if (c == ',')
                {
                    registerCharToken(",", TokenType.COLON);
                    m_srcPtr++;
                }
                else if (c == '>')
                {
                    char peek = peekChar();
                    if (peek == '=')
                    {
                        int tokenStart = m_srcPtr + 1;
                        int tokenEnd = m_srcPtr + 2;
                        Token t = new Token(
                            ">=",
                            tokenStart,
                            tokenEnd,
                            m_line + 1,
                            TokenType.GREATER_EQUAL
                        );
                        m_tokens.Add(t);
                        m_srcPtr += 2;
                    }
                    else
                    {
                        registerCharToken(">", TokenType.GREATER_THAN);
                        m_srcPtr++;
                    }
                }
                else if (c == '<')
                {
                    char peek = peekChar();
                    if (peek == '=')
                    {
                        int tokenStart = m_srcPtr + 1;
                        int tokenEnd = m_srcPtr + 2;
                        Token t = new Token(
                            "<=",
                            tokenStart,
                            tokenEnd,
                            m_line + 1,
                            TokenType.LESS_EQUAL
                        );
                        m_tokens.Add(t);
                        m_srcPtr += 2;
                    }
                    else
                    {
                        registerCharToken("<", TokenType.LESS_THAN);
                        m_srcPtr++;
                    }
                }
                else if (c == '=')
                {
                    char peek = peekChar();
                    if (peek == '=')
                    {
                        int tokenStart = m_srcPtr + 1;
                        int tokenEnd = m_srcPtr + 2;
                        Token t = new Token(
                            "==",
                            tokenStart,
                            tokenEnd,
                            m_line + 1,
                            TokenType.EQUAL_EQUAL
                        );
                        m_tokens.Add(t);
                        m_srcPtr += 2;
                    }
                    else
                    {
                        registerCharToken("=", TokenType.EQUAL);
                        m_srcPtr++;
                    }
                }
                else if (c == '&')
                {
                    char peek = peekChar();
                    if (peek == '&')
                    {
                        int tokenStart = m_srcPtr + 1;
                        int tokenEnd = m_srcPtr + 2;
                        Token t = new Token(
                            "&&",
                            tokenStart,
                            tokenEnd,
                            m_line + 1,
                            TokenType.LOGICAL_AND
                        );
                        m_tokens.Add(t);
                        m_srcPtr += 2;
                    }
                    else
                    {
                        int tokenStart = m_srcPtr + 1;
                        int line = m_line + 1;
                        string errorMsg =
                            "Invalid token: '&' in "
                            + m_fileName
                            + ".min ("
                            + line.ToString()
                            + ":"
                            + tokenStart.ToString()
                            + ")";
                        ErrorHandler.Error(errorMsg);
                    }
                }
                else if (c == '|')
                {
                    char peek = peekChar();
                    if (peek == '|')
                    {
                        int tokenStart = m_srcPtr + 1;
                        int tokenEnd = m_srcPtr + 2;
                        Token t = new Token(
                            "||",
                            tokenStart,
                            tokenEnd,
                            m_line + 1,
                            TokenType.LOGICAL_OR
                        );
                        m_tokens.Add(t);
                        m_srcPtr += 2;
                    }
                    else
                    {
                        int tokenStart = m_srcPtr + 1;
                        int line = m_line + 1;
                        string errorMsg =
                            "Invalid token: '|' in "
                            + m_fileName
                            + ".min ("
                            + line.ToString()
                            + ":"
                            + tokenStart.ToString()
                            + ")";
                        ErrorHandler.Error(errorMsg);
                    }
                }
                else
                {
                    int tokenStart = m_srcPtr + 1;
                    int line = m_line + 1;
                    string errorMsg =
                        "Invalid character: '"
                        + c
                        + "' in "
                        + m_fileName
                        + ".min ("
                        + line.ToString()
                        + ":"
                        + tokenStart.ToString()
                        + ")";
                    ErrorHandler.Error(errorMsg);
                }
                

            }
        }
    }
}
