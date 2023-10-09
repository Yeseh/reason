namespace Reason.Script;

public record struct Token(TokenType Type, string? Literal = null, int? Index = -1);