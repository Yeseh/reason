using System.Diagnostics;
using Spectre.Console;

namespace Reason.Client.Script;

public record AstNode();

public record Script(AstNode Root) : AstNode;
public record Ref(Token Type, Lit Lit) : AstNode;
public record Lit(Token Token) : AstNode;
public record Ass(AstNode rhs, AstNode lhs) : AstNode;

public record CommandPath(IEnumerable<Lit> Segments) : AstNode;

public record CommandParams(IEnumerable<AstNode> Params) : AstNode;
public record Invocation(CommandPath command, CommandParams? pars = null) : AstNode;

/*
 * Notes:
 * - Every non keyword that is not used a ref, is a literal
 * 
 * script: invocation | assignment | ref?
 * ref: secret_ref | variable_ref | env_ref
 * invocation: command_path param_tuple?
 * param_tuple: LPAREN (ref|param_tuple|LITERAL) RPAREN 
 * assignment: (VAR|SECRET|ENV) EQUALS (ref | LITERAL | invocation?)
 * command_path: LITERAL (DOT LITERAL)*
 * secret_ref: POUND LITERAL
 * variable_ref: DOLLAR LITERAL
 * env_ref: AT LITERAL
 */
public class Parser
{
    private Lexer _lex;
    private Token currentToken;
    private Exception[] Errors;

    private static TokenType[] RefTokens = new[] { TokenType.At, TokenType.Dollar, TokenType.Pound };
    private static TokenType[] AssignmentTokens = new[] { TokenType.Env, TokenType.Secret, TokenType.Var };
    
    public Parser(Lexer lexer)
    {
        _lex = lexer;
    }

    public Script Parse(Lexer lex)
    {
        var script = Script();
        return script;
    }
    
    private Token Eat(params TokenType[] types)
    {
        if (types.Contains(currentToken.Type))
        {
            currentToken = _lex.GetNextToken();
            return currentToken;
        }
        else
        {
            var expected = string.Join(" or ", types);
            throw new Exception($"ParserError: Expected one of {expected} got {currentToken.Type}");
        }
    }

    public Lit Lit()
    {
        var token = Eat(TokenType.Literal);
        return new Lit(token);
    }

    public Ref Ref()
    {
        var type = Eat(RefTokens);
        var lit = Lit(); 
        return new Ref(type, lit);
    }
    
    public Script Script()
    {
        AstNode? node = null;
        var token = currentToken;
        
        if (RefTokens.Contains(token.Type))
        {
            node = Ref();
        }
        
        if (AssignmentTokens.Contains(token.Type))
        {
            node = Assignment();
        }

        if (token.Type == TokenType.Literal)
        {
            node = Invocation();
        }

        if (node == null)
        {
            throw new UnreachableException();
        }
        
        Eat(TokenType.EOF);
        return new Script(node);
    }

    private Ass Assignment()
    {
        Eat(AssignmentTokens);
        var lhs = Lit(); 
        
        Eat(TokenType.Space);
        Eat(TokenType.Equal);
        Eat(TokenType.Space);

        AstNode rhs = RefTokens.Contains(currentToken.Type)
            ? Ref()
            : Lit();
        
        return new Ass(rhs, lhs);
    }

    private CommandPath CommandPath()
    {
        var id = Lit();
        var segments = new List<Lit>() { id };
        while (currentToken.Type == TokenType.Dot)
        {
            Eat(TokenType.Dot);
            var lit = Lit();
            segments.Add(lit);
        }
        
        return new CommandPath(segments);
    }

    private CommandParams CommandParams()
    {
        var nodes = new List<AstNode>();
        var accepted = RefTokens.Concat(new[] { TokenType.LParen, TokenType.Literal });
        while (currentToken.Type == TokenType.Space)
        {
            AstNode node;
            Eat(TokenType.Space);
            var ate = Eat(accepted.ToArray());
            
            if (RefTokens.Contains(ate.Type))
            {
                nodes.Add(Ref());
            }
            else if (ate.Type == TokenType.LParen)
            {
                nodes.Add(CommandParams());
            }
            else if (ate.Type == TokenType.Literal)
            {
                nodes.Add(Lit());
            }
        }
        
        return new CommandParams(nodes);
    }

    private Invocation Invocation()
    {
        var commandPath = CommandPath();
        var ate = Eat(TokenType.EOF, TokenType.Space);
        
        if (ate.Type == TokenType.EOF)
        {
            return new Invocation(commandPath, null);
        }

        var commandParams = CommandParams();
        return new Invocation(commandPath, null);
    }
}