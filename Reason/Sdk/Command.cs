using Reason.Script;

namespace Reason.Sdk;

[System.Serializable]
public class Command
{
	public string OperationPath { get; set; } = string.Empty;
	
	public string Type { get; set; } = string.Empty;
	
	public virtual Task<Result> Call()
	{
		return Task.FromResult(Result.Success());
	}
	
	public virtual Task<Result> Undo()
	{
		return Task.FromResult(Result.Success());
	}
	
	public Command()
	{
	}

	protected Command(string operationPath, string type)
	{
		OperationPath = operationPath;
		Type = type;
	}
}

