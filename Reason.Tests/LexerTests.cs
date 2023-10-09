using Reason.Script;

namespace Reason.Client.Tests;

public class LexerTests
{
    private void AssertTests(IEnumerable<Token> tokens, string command, Token[] expected)
    {
        var i = 0;
        var tokenList = tokens.ToList();
        Assert.Equal(expected.Length, tokenList.Count);
        foreach (var token in tokenList)
        {
            var expectedToken = expected[i];
            Assert.Equal(expectedToken.Type, token.Type);
            if (expectedToken.Literal != null)
            {
                Assert.Equal(expectedToken.Literal, token.Literal);
            }
            if (expectedToken.Index > -1)
            {
                Assert.Equal(expectedToken.Index, token.Index);
            }
            i++;
        }
    }
    
    [Fact]
    public void Lexer_should_insert_tokens_for_whitespace()
    {
        var command = "  \n   \r\n";
        var expected = new Token[]
        {
            new Token(TokenType.Space),
            new Token(TokenType.LineBreak),
            new Token(TokenType.Space),
            new Token(TokenType.LineBreak),
            new Token(TokenType.EOF),
        };
        var tokens = new Lexer(command).Lex();
        AssertTests(tokens, command, expected);
    }
    
    [Fact]
    public void Lexer_should_insert_tokens_for_commands()
    {
        var command = "this.is.my.command @value $pw ( @another )";
        var expected = new Token[]
        {
            new Token(TokenType.Literal, "this"),
            new Token(TokenType.Dot),
            new Token(TokenType.Literal, "is"),
            new Token(TokenType.Dot),
            new Token(TokenType.Literal, "my" ),
            new Token(TokenType.Dot),
            new Token(TokenType.Literal, "command"),
            new Token(TokenType.Space),
            new Token(TokenType.At, "@"),
            new Token(TokenType.Literal, "value"),
            new Token(TokenType.Space),
            new Token(TokenType.Dollar, "$"),
            new Token(TokenType.Literal, "pw"),
            new Token(TokenType.Space),
            new Token(TokenType.LParen),
            new Token(TokenType.Space),
            new Token(TokenType.At),
            new Token(TokenType.Literal, "another"),
            new Token(TokenType.Space),
            new Token(TokenType.RParen),
            
            new Token(TokenType.EOF),
        };
        var tokens = new Lexer(command).Lex();
        AssertTests(tokens, command, expected);
    }
    
    
    [Fact]
    public void Lexer_should_insert_tokens_for_keywords()
    {
        var command = "var env secret";
        var expected = new Token[]
        {
            new Token(TokenType.Var, "var", 0),
            new Token(TokenType.Space, " ", 3),
            new Token(TokenType.Env, "env", 4),
            new Token(TokenType.Space, " ", 7),
            new Token(TokenType.Secret, "secret", 8),
            new Token(TokenType.EOF),
        };
        var tokens = new Lexer(command).Lex();
        AssertTests(tokens, command, expected);
    }
    
    [Fact]
    public void Lexer_should_output_the_correct_tokens()
    {
        var command = "$.@#-|--=()     var secret env blabla";
        var expected = new Token[]
        {
            new Token(TokenType.Dollar, "$", 0),
            new Token(TokenType.Dot, ".", 1),
            new Token(TokenType.At, "@", 2),
            new Token(TokenType.Pound, "#", 3),
            new Token(TokenType.Minus, "-", 4),
            new Token(TokenType.Pipe, "|", 5),
            new Token(TokenType.MinusMinus, "--", 6),
            new Token(TokenType.Equal, "=", 8),
            new Token(TokenType.LParen, "(", 9),
            new Token(TokenType.RParen, ")", 10),
            new Token(TokenType.Space, " ", 11),
            new Token(TokenType.Var, "var", 16),
            new Token(TokenType.Space, " ", 19),
            new Token(TokenType.Secret, "secret", 20),
            new Token(TokenType.Space, " ", 26),
            new Token(TokenType.Env, "env", 27),
            new Token(TokenType.Space, " ", 30),
            new Token(TokenType.Literal,"blabla", 31),
            new Token(TokenType.EOF, Index: 37),
        };

        var lexer = new Lexer(command);
        var tokens = lexer.Lex();
        AssertTests(tokens, command, expected);
    }
}