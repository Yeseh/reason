using System.Diagnostics;
using Spectre.Console;

namespace Reason.Client.Script;

public enum SerializationHint
{
	None = default,
	Json,
}

public class InterpreterResult
{
	public string? Message;
	public string? Value;
	public OutputStatus Status;
	public SerializationHint SerializationHint;
	
	public InterpreterResult(OutputStatus status = OutputStatus.Success)
	{
		Status = status;
	}
	
	public InterpreterResult(string message, OutputStatus status)
	{
		Message = message;
		Status = status;
	}
	
	public InterpreterResult(
		string message, 
		string value, 
		OutputStatus status, 
		SerializationHint serializationHint)
	{
		Message = message;
		Status = status;
		SerializationHint = serializationHint;
		Value = value;
	}
}

public class Interpreter
{
	private Workspace _ws;
	
	public Interpreter(Workspace ws)
	{
		_ws = ws;
	}

	public Task<InterpreterResult> Run(string scriptText)
	{
		var lexer = new Lexer(scriptText);
		var parser = new Parser(lexer);
		var script = parser.Parse();
		
		return Run(script);
	}

	// TODO: Just big switch on type of AstNode for nw
	// Could be fancy with visitor pattern or something, but nah
	private Task<InterpreterResult> Run(Script script)
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

	private async Task<InterpreterResult> RunInvocation(Invocation invoc)
	{
		Debug.Assert(invoc.Command.Segments.Any());
		
		var result = new InterpreterResult();
		var commandPathLits = invoc.Command.Segments.Select(i => i.Token.Literal).ToList();
		var commandPath = string.Join('.', commandPathLits);
		
		var split = commandPath.Split('.');
		var prefix = commandPathLits.First();

		var api = _ws.GetApi(prefix!);
		if (api == null)
		{
			return new InterpreterResult($"No api registered with prefix '{prefix}'", OutputStatus.Error);
		}

		if (split.Length == 1)
		{
			return new InterpreterResult(api.Help(), OutputStatus.Info);
		}
		
		var command = api.GetCommand(commandPath);
		if (command == null)
		{
			result.Message = $"No command found with path {commandPath}\n {api.Help()}";
			result.Status = OutputStatus.Error;
			return result;
		}
		
		// TODO: Support other command types
		var commandResult = await command.Call() as HttpResponseMessage;
		var bSucces = commandResult is { IsSuccessStatusCode: true };
		if (bSucces)
		{
			var content = await commandResult.Content.ReadAsStringAsync();
			result.Message = $"{commandResult.StatusCode}";
			result.Value = content;
			result.SerializationHint = SerializationHint.Json;
		}
		else
		{
			var content = await commandResult.Content.ReadAsStringAsync();
			result.Message = $"{commandResult!.StatusCode} {content}";
			result.Status = OutputStatus.Error;
		}

		return result;
	}

	private InterpreterResult RunAssignment(Ass assgnment)
	{
		return new InterpreterResult("Assignment not implemented", OutputStatus.Warning);
	}
	
	public InterpreterResult RunReference(Ref r)
	{
		return new InterpreterResult("Reference not implemented", OutputStatus.Warning);
	}
}
