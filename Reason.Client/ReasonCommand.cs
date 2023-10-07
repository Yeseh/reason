namespace Reason.Client;

[System.Serializable]
public class ReasonCommandData
{
	public string OperationPath { get; set; } = string.Empty;
	
	public string Type { get; set; } = string.Empty;
}

public abstract class ReasonCommand
{
	public virtual ReasonCommandData Data { get; }
	
	// TODO: Obj ugly, find right abstraction
	public abstract Task<object?> Call();
	public abstract Task Undo();

	protected ReasonCommand(string operationPath, string type)
	{
		Data = new ReasonCommandData()
		{
			OperationPath = operationPath,
			Type = type
		};
	}
}

