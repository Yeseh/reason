using System.Diagnostics;
using Spectre.Console;

namespace Reason.Client.Script;

public record AstNode();

public record Script(AstNode Root) : AstNode;
public record Ref(Token Type, Lit Lit) : AstNode;
public record Lit(Token Token) : AstNode;
public record Tuple(IEnumerable<AstNode> Members) : AstNode;
public record Ass(Token Token, AstNode Rhs, AstNode Lhs) : AstNode;
public record CommandPath(IEnumerable<Lit> Segments) : AstNode;
public record CommandParams(IEnumerable<AstNode> Params) : AstNode;
public record Invocation(CommandPath Command, CommandParams? Params = null) : AstNode;

/*
 * Notes:
 * - Every non keyword that is not used a ref, is a literal
 * 
 * script: invocation | assignment | ref?
 * ref: secret_ref | variable_ref | env_ref
 * invocation: command_path SPACE command_params?
 * tuple: LPAREN ((ref|tuple|LITERAL)SPACE)* RPAREN 
 * command_params: ((ref|tuple|LITERAL)SPACE)*
 * assignment: (VAR|SECRET|ENV) SPACE EQUALS SPACE (ref | LITERAL | invocation?)
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

    private static TokenType[] RefTokens = { TokenType.At, TokenType.Dollar, TokenType.Pound };
    private static TokenType[] AssignmentTokens = { TokenType.Env, TokenType.Secret, TokenType.Var };
    
    public Parser(Lexer lexer)
    {
        _lex = lexer;
    }

    public Script Parse()
    {
        currentToken = _lex.GetNextToken();
        var script = Script();
        return script;
    }
    
    // TODO: For fun: make this more performant by avoiding the array allocation 
    private Token Eat(params TokenType[] types)
    {
        if (types.Contains(currentToken.Type))
        {
            var cur = currentToken;
            currentToken = _lex.GetNextToken();
            return cur;
        }
        
        var expected = string.Join(" or ", types);
        throw new Exception($"ParserError: Expected one of {expected} got {currentToken.Type}");
    }

    private Lit Lit()
    {
        var token = Eat(TokenType.Literal);
        return new Lit(token);
    }

    private Ref Ref()
    {
        var type = Eat(RefTokens);
        var lit = Lit(); 
        return new Ref(type, lit);
    }
    
    private Script Script()
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
        var ate = Eat(AssignmentTokens);
        Eat(TokenType.Space);
        
        var lhs = Lit(); 
        Eat(TokenType.Space);
        Eat(TokenType.Equal);
        Eat(TokenType.Space);

        AstNode rhs = RefTokens.Contains(currentToken.Type)
            ? Ref()
            : Lit();
        
        return new Ass(ate, rhs, lhs);
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
                var lit = Lit();
                var r = new Ref(ate, lit);
                nodes.Add(r);
            }
            else if (ate.Type == TokenType.LParen)
            {
                nodes.Add(Tuple());
            }
            else if (ate.Type == TokenType.Literal)
            {
                var lit = new Lit(ate);
                nodes.Add(lit);
            }
        }
        
        return new CommandParams(nodes);
    }

    private Tuple Tuple()
    {
        var nodes = new List<AstNode>();
        var accepted = RefTokens.Concat(new[] { TokenType.LParen, TokenType.RParen, TokenType.Literal });
        while (currentToken.Type == TokenType.Space)
        {
            AstNode node;
            Eat(TokenType.Space);
            var ate = Eat(accepted.ToArray());

            if (ate.Type == TokenType.RParen)
            {
                break;
            }
            if (RefTokens.Contains(ate.Type))
            {
                var lit = Lit();
                var r = new Ref(ate, lit);
                nodes.Add(r);
            }
            else if (ate.Type == TokenType.LParen)
            {
                nodes.Add(Tuple());
                Eat(TokenType.Space);
                Eat(TokenType.RParen);
            }
            else if (ate.Type == TokenType.Literal)
            {
                var lit = new Lit(ate);
                nodes.Add(lit);
            }
        }
        
        return new Tuple(nodes);
    }

    private Invocation Invocation()
    {
        var commandPath = CommandPath();
        var commandParams = CommandParams();
        return new Invocation(commandPath, commandParams);
    }
}