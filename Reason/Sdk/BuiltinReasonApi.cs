
namespace Reason.Sdk;

public class BuiltinReasonApi : ReasonApi
{
	public BuiltinReasonApi() : base("builtin", "rsn", "builtin") { }
	
	public override Task Init(bool force = false)
	{
		Console.WriteLine("Initializing api " + Name);
		Commands.Add(MakeOperationPath("quote"), new QuoteCommand());	
		return Task.CompletedTask;
	}
}
