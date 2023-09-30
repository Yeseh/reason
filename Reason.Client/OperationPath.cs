namespace Reason.Client;

public readonly struct OperationPath
{
	private readonly string _path;
	
	public OperationPath(string operationId)
	{
		this._path = operationId.ToLower(); 
	}
    
	public OperationPath(string path, string operationId)
	{
		var substr = path.Substring(1);
		this._path = $"{substr.Replace("/", ".")}.{operationId}".ToLower(); 
	}
        
	public override string ToString() => _path;
}
