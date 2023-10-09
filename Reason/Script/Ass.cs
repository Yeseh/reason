namespace Reason.Script;

public record Ass(Token Token, AstNode Rhs, AstNode Lhs) : AstNode;