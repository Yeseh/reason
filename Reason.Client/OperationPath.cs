namespace Reason.Client;

[System.Serializable]
public readonly struct OperationPath
{
	public string Value {get;}
	
	public OperationPath(string operationId)
	{
		this.Value = operationId.ToLower(); 
	}
    
	public OperationPath(string path, string operationId)
	{
		var substr = path.Substring(1);
		this.Value = $"{substr.Replace("/", ".")}.{operationId}".ToLower(); 
	}
        
	public override string ToString() => Value;
}
