using Reason.Sdk;

namespace Reason.Sdk;

public class ReasonApi : Init
{
	public string Type { get; set;  } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string CommandPrefix { get; set;  } = string.Empty;
	
	public Dictionary<string, Command> Commands { get; set; } = new();
	
	public ReasonApi()
	{
	}
	
	public ReasonApi(string name, string prefix, string type)
	{
		Name = name;
		CommandPrefix = prefix;
		Type = type;
	}

	public virtual Task Init(bool force = false)
	{
		return Task.CompletedTask;
	}

	public string Help()
	{
		var msg =
			$@"Available commands for api '{Name}': 
{string.Join($"{Environment.NewLine}", Commands.Values.Select(c => $"{CommandPrefix}.{c.OperationPath}"))}";
	return msg;
	}

	public Command? GetCommand(string path)
	{
		var bExists = Commands.TryGetValue(path, out var command);
		if (!bExists || command == null) return null;
		
		return command;
	}
	
	protected string MakeOperationPath(string path)
	{
		return $"{CommandPrefix}.{path}"; 
	}
}
