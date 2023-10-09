namespace Reason.Sdk;

public class QuoteCommand : Command
{
	public override Task<Result> Call()
	{
		return Task.FromResult(Result.Success("This is a super interesting quote"));
	}
	
	public override Task<Result> Undo()
	{
		return Task.FromResult(Result.Error("Undo not implemented"));
	}
	
	public QuoteCommand() : base("quote", "builtin") { }
}