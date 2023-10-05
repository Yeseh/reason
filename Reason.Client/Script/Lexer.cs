using System.Collections;
using System.Text;
using System.Xml.Schema;

namespace Reason.Client.Script;

/*
 * var var = bla
 * $test
 * secret test = bla
 * @test
 * env test = bla
 * #test
 *
 * b.bla.bla $var @secret
 */
public enum TokenType
{
    Var,
    Secret,
    Env,
    Equal,
    Break,
    Dot,
    At,
    Pound,
    Dollar,
    Literal,
    Minus,
    MinusMinus,
    Pipe,
}

public record struct Token(TokenType Type, string? Literal = null, int? index = -1);

public class Lexer 
{
    private static Dictionary<string, Token> KeyWords = new()
    {
        { "var", new Token(TokenType.Var, "var") },
        { "env", new Token(TokenType.Env, "env") },
        { "secret", new Token(TokenType.Secret, "secret") },
    };
    private static char[] WhiteSpace = { ' ', '\t' };

    private string _text = string.Empty;
    private int _pos = 0;
    private char? currentChar;
    
    public Lexer(string command)
    {
        _text = command;
    }

    public IEnumerable<Token> Lex()
    {
        while (currentChar != null)
        {
            yield return GetNextToken();
        }
    }
    
    private Token GetNextToken()
    {
        // We know this is not null due to check in Lex()
        Token token;
        var chr = currentChar!.Value;
        
        // Handle BREAK first as the token doesnt have a literal
        // Add a token for all combined tabs/spaces between other tokens
        // Might be semantically important later
        if (WhiteSpace.Contains(chr)) { return Break(); }

        token = chr switch
        {
            '-' when Peek() == '-' => new Token(TokenType.MinusMinus, "--", _pos),
            '-' => new Token(TokenType.Minus, "-", _pos),
            '=' => new Token(TokenType.Equal, "=", _pos),
            '$' => new Token(TokenType.Dollar, "$", _pos),
            '@' => new Token(TokenType.At, "@", _pos),
            '#' => new Token(TokenType.Pound, "#", _pos),
            '.' => new Token(TokenType.Dot, ".", _pos),
            '|' => new Token(TokenType.Pipe, "|", _pos),
            
            // This could be, keyword, identifier, or literal value in case of var/secret/env assignment
            // Semantics decide if its ID or Literal so put it in Identitifier token for now
            _ => KeywordOrID()
        };
        
        Advance(token.Literal!.Length);
        return token;
    }

    private void Advance(int count = 1)
    {
        this._pos += count;
        if (_pos > _text.Length)
        {
            currentChar = null;
            return;
        }
        currentChar = _text[_pos];
    }

    private char? Peek()
    {
        var localPos = _pos;
        var textSpan = _text.AsSpan();
        var bEnd = localPos > _text.Length - 1;
        return bEnd ? null : _text[localPos];
    }

    private string PeekN(int num)
    {
        var localPos = _pos;
        var textSpan = _text.AsSpan();
        var result = new StringBuilder(); 
        while (localPos - _pos < num)
        {
            result.Append(textSpan[localPos]);
            localPos++;
        }
        return result.ToString();
    }
    
    private Token Break()
    {
        while (WhiteSpace.Contains(currentChar!.Value)) { Advance(); }
        return new Token(TokenType.Break);
    }

    public Token KeywordOrID()
    {
        var result = new StringBuilder();
        result.Append(currentChar!.Value);

        while (currentChar != ' ' && currentChar != '.')
        {
            _pos++;  
            result.Append(_text[_pos]);
        }
        
        var resultStr = result.ToString();
        var bKeyword = KeyWords.TryGetValue(resultStr, out var token);
        if (!bKeyword) {  token = new Token(TokenType.Literal, resultStr); }
        
        return token;
    }
}
