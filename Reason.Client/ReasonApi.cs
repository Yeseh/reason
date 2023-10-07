namespace Reason.Client;

[System.Serializable]
public class ReasonApiData
{
	public string Type { get; set;  } = "builtin";
	public string Name { get; set; } = string.Empty;
	public string CommandPrefix { get; set;  } = string.Empty;
	public Dictionary<string, ReasonCommandData> Commands { get; set; } = new();
	
	public ReasonApiData(string name, string commandPrefix, string type)
	{
		Name = name;
		CommandPrefix = commandPrefix;
		Type = type;
	}
}

public abstract class ReasonApi : SerializeData
{
	public virtual ReasonApiData Data { get; }
	public string Name => Data.Name;
	public string CommandPrefix => Data.CommandPrefix;
	public Dictionary<string, ReasonCommand> Commands { get; set; } = new();
	
	public ReasonApi(string name, string prefix)
	{
		Data = new ReasonApiData(name, prefix, "builtin");
	}
	
	public abstract byte[] Serialize();

	public abstract Task Init(bool force = false);

	public string Help()
	{
		var msg =
			$@"Available commands for {Data.Name}: 
{string.Join($"{Environment.NewLine}", Commands.Values.Select(c => $"{CommandPrefix}.{c.Data.OperationPath}"))}";
	return msg;
	}
	
	public ReasonCommand? GetCommand(string path)
	{
		var opPath = new OperationPath(path);
		return Commands.TryGetValue(opPath.Value, out var operation) 
			? operation
			: null;
	}
	
	public ReasonCommand? GetCommand(OperationPath path)
	{
		return Commands.TryGetValue(path.Value, out var operation) 
			? operation 
			: null;
	}
	
	protected string MakeOperationPath(string path)
	{
		return $"{Data.CommandPrefix}.{path}"; 
	}
}
