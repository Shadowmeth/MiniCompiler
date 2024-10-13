import os


def passed_test_msg(msg):
    print(f"\033[1;32m{msg}\033[0m")


def failed_test_msg(msg):
    print(f"\033[1;31m{msg}\033[0m")


def read_tokens():
    os.system("dotnet run Tests/lexer_test.min")
    # by default we will create lexer_test.lex file for this
    with open("./Tests/lexer_test.lex", "r") as tokens_file:
        tokens = [token_line[:-1] for token_line in tokens_file.readlines()]
        return tokens


def testLexer():
    tokens = read_tokens()

    if (
        tokens[0] == "1 1:3 KEYWORD int"
        and tokens[1] == "1 5:7 IDENTIFIER age"
        and tokens[2] == "1 9:9 EQUAL ="
        and tokens[3] == "1 11:12 NUMBER 10"
        and tokens[4] == "1 13:13 SEMICOLON ;"
        and tokens[5] == "2 1:2 KEYWORD if"
        and tokens[6] == "2 4:4 LEFT_PAREN ("
        and tokens[7] == "2 5:7 IDENTIFIER age"
        and tokens[8] == "2 9:10 GREATER_EQUAL >="
        and tokens[9] == "2 12:13 NUMBER 18"
        and tokens[10] == "2 14:14 RIGHT_PAREN )"
        and tokens[11] == "3 1:1 LEFT_BRACE {"
        and tokens[12] == "4 5:9 IDENTIFIER print"
        and tokens[13] == "4 10:10 LEFT_PAREN ("
        and tokens[14] == "4 12:39 STRING_LIT you are eligible to vote at "
        and tokens[15] == "4 42:42 ADD +"
        and tokens[16] == "4 44:46 IDENTIFIER age"
        and tokens[17] == "4 48:48 ADD +"
        and tokens[18] == "4 51:56 STRING_LIT  years"
        and tokens[19] == "4 58:58 RIGHT_PAREN )"
        and tokens[20] == "4 59:59 SEMICOLON ;"
        and tokens[21] == "5 1:1 RIGHT_BRACE }"
        and tokens[22] == "5 3:6 KEYWORD else"
        and tokens[23] == "6 1:1 LEFT_BRACE {"
        and tokens[24] == "7 5:9 IDENTIFIER print"
        and tokens[25] == "7 10:10 LEFT_PAREN ("
        and tokens[26] == "7 12:42 STRING_LIT you can't vote currently sorry!"
        and tokens[27] == "7 44:44 RIGHT_PAREN )"
        and tokens[28] == "7 45:45 SEMICOLON ;"
        and tokens[29] == "8 1:1 RIGHT_BRACE }"
        and tokens[30] == "-1 -1:-1 EOF EOF"
    ):
        passed_test_msg("Lexer tests passed: 1/1 passed")
    else:
        failed_test_msg("Lexer tests failed: 0/1 passed")


if __name__ == "__main__":
    testLexer()
