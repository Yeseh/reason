
namespace Reason.Script;

using Reason.Sdk;
using System.Diagnostics;
using Reason;

public class Interpreter
{
	private Workspace _ws;
	
	public Interpreter(Workspace ws)
	{
		_ws = ws;
	}

	public Task<Result> Run(string scriptText)
	{
		var lexer = new Lexer(scriptText);
		var parser = new Parser(lexer);
		var script = parser.Parse();
		
		return Run(script);
	}

	// TODO: Just big switch on type of AstNode for nw
	// Could be fancy with visitor pattern or something, but nah
	private Task<Result> Run(Script script)
	{
		if (script.Root is Invocation invoc)
		{
			return RunInvocation(invoc);
		}
		
		if (script.Root is Ass ass)
		{
			return Task.FromResult(RunAssignment(ass));
		}
		
		if (script.Root is Ref r)
		{
			return Task.FromResult(RunReference(r));
		}

		throw new NotSupportedException("Script root node is of unimplemented type");
	}

	private async Task<Result> RunInvocation(Invocation invoc)
	{
		Debug.Assert(invoc.Command.Segments.Any());
		
		var result = new Result();
		var commandPathLits = invoc.Command.Segments.Select(i => i.Token.Literal).ToList();
		var commandPath = string.Join('.', commandPathLits);
		
		var split = commandPath.Split('.');
		var prefix = commandPathLits.First();

		var api = _ws.GetApi(prefix!);
		if (api == null)
		{
			return Result.Error($"No api registered with prefix '{prefix}'");
		}

		if (split.Length == 1)
		{
			return Result.Success(api.Help(), status: OutputStatus.Info);
		}
		
		var command = api.GetCommand(commandPath);
		if (command == null)
		{
			result.Message = $"No command found with path {commandPath}\n {api.Help()}";
			result.Status = OutputStatus.Error;
			return result;
		}
		
		result = await command.Call();
		return result;
	}

	private Result RunAssignment(Ass assgnment)
	{
		return Result.Error("Assignment not implemented");
	}
	
	public Result RunReference(Ref r)
	{
		return Result.Error("Reference not implemented");
	}
}
