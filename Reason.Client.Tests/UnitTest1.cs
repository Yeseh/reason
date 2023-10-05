using Reason.Client.Script;
using Spectre.Console.Cli;

namespace Reason.Client.Tests;

public class LexerTests
{
    [Fact]
    public void Lexer_should_output_the_correct_tokens()
    {
        var command = "$.@#-|--     var secret env blabla";
        var expected = new Token[]
        {
            new Token(TokenType.Dollar, "$"), 
            new Token(TokenType.Dot, "."),
            new Token(TokenType.At, "@"),
            new Token(TokenType.Pound, "#"),
            new Token(TokenType.Minus, "-"),
            new Token(TokenType.Pipe, "|"),
            new Token(TokenType.MinusMinus, "--"),
            new Token(TokenType.Break),
            new Token(TokenType.Var, "var"),
            new Token(TokenType.Break),
            new Token(TokenType.Secret, "secret"),
            new Token(TokenType.Break),
            new Token(TokenType.Env, "env"),
            new Token(TokenType.Break),
            new Token(TokenType.Literal, "blabla"),
        };

        var lexer = new Lexer(command);
        for (int i = 0; i < UPPER; i++)
        {
            
        }
        
        


    }
}