using System.Text;

namespace Reason.Script;

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
    private char currentChar;
    
    public Lexer(string command)
    {
        _text = command;
        currentChar = _text[0];
    }

    public IEnumerable<Token> Lex()
    {
        var token = GetNextToken();
        yield return token; 
        
        while (token.Type != TokenType.EOF)
        {
            token = GetNextToken();
            yield return token;
        }
    }
    
    public Token GetNextToken()
    {
        if (currentChar == char.MinValue)
        {
            return new Token(TokenType.EOF, Index: _pos);
        }
        // We know this is not null due to check in Lex()
        var chr = currentChar;
        Token token;
        
        // Handle BREAK first as the token doesnt have a literal
        // Add a token for all combined tabs/spaces between other tokens
        // Might be semantically important later
        if (WhiteSpace.Contains(chr)) { return Space(); }

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
            ')' => new Token(TokenType.RParen, ")", _pos),
            '(' => new Token(TokenType.LParen, "(", _pos),
            '\n' => new Token(TokenType.LineBreak, "\n", _pos),
            '\r' when Peek() == '\n' => new Token(TokenType.LineBreak, "\r\n", _pos),
            
            // This could be, keyword, identifier, or literal value in case of var/secret/env assignment
            // Semantics decide if its ID or Literal so put it in Identitifier token for now
            _ => KeywordOrID()
        };
        
        Advance(token.Literal?.Length ?? 1);
        return token;
    }

    private void Advance(int count = 1)
    {
        _pos += count;
        if (_pos > _text.Length - 1)
        {
            currentChar = char.MinValue;
            return;
        }
        currentChar = _text[_pos];
    }

    private char Peek()
    {
        var bEnd = _pos > _text.Length - 1;
        return bEnd ? char.MinValue : _text[_pos + 1];
    }

    private string PeekN(int num)
    {
        var localPos = _pos;
        var textSpan = _text.AsSpan();
        var result = new StringBuilder(num); 
        
        while (localPos - _pos < num)
        {
            result.Append(textSpan[localPos]);
            localPos++;
        }
        return result.ToString();
    }
    
    private Token Space()
    {
        var token = new Token(TokenType.Space, " ", _pos);
        while (WhiteSpace.Contains(currentChar)) { Advance(); }

        return token;
    }

    private int NextIndexOf(char chr)
    {
        var localPos = _pos;
        var slice = _text.AsSpan().Slice(localPos);
        var idx = slice.IndexOf(chr);
        var ret = idx == -1 ? -1 : idx + localPos;
        return ret;
    }

    public Token KeywordOrID()
    {
        var localPos = _pos;
        var cur = _text[localPos];
        var result = new StringBuilder();
        // Spaces indicate a new semantic
        // Dots seperate identifiers of command paths
        while (cur != ' ' && cur != '.' && cur > char.MinValue)
        {
            result.Append(cur);
            cur = localPos >= _text.Length -1 
                ? char.MinValue 
                : _text[++localPos];
        }
        
        var resultStr = result.ToString();
        var bKeyword = KeyWords.TryGetValue(resultStr, out var token);
        if (!bKeyword) {  token = new Token(TokenType.Literal, resultStr); }
        var retToken = token with {  Index = _pos };
        
        return retToken;
    }
}
