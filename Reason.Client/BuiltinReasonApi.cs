namespace Reason.Client;

public class QuoteCommand : ReasonCommand
{
	public override Task<object?> Call()
	{
		return Task.FromResult((object?)"This is a super interesting quote");
	}
	
	public override Task Undo()
	{
		throw new NotImplementedException();
	}
	
	public QuoteCommand() : base("quote", "builtin") { }
}

public class BuiltinReasonApi : ReasonApi
{
	public BuiltinReasonApi() : base("builtin", "rsn") { }

	public override byte[] Serialize()
	{
		foreach (var command in Commands)
		{
			Data.Commands.Add(command.Key, command.Value.Data);
		}
		return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(Data);
	}
	
	public override Task Init(bool force = false)
	{
		Console.WriteLine("Initializing api " + Data.Name);
		Commands.Add(MakeOperationPath("quote"), new QuoteCommand());	
		return Task.CompletedTask;
	}
}
