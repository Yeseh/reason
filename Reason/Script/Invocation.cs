namespace Reason.Script;

public record Invocation(CommandPath Command, CommandParams? Params = null) : AstNode;