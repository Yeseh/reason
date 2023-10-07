namespace Reason.Client;

[System.Serializable]
public class HttpCommandData : ReasonCommandData
{
	public string Uri { get; set; } = string.Empty;
	public string Method { get; set; } = string.Empty;
}

public class HttpCommand : ReasonCommand
{
	public sealed override HttpCommandData Data  { get; }

	private HttpClient _httpClient = new();
	
	public HttpCommand(string operationPath, string uri, string method, HttpClient client) : base(operationPath, "http")
	{
		_httpClient = client;
		Data = new HttpCommandData()
		{
			OperationPath = base.Data.OperationPath,
			Type = base.Data.Type,
			Uri = uri,
			Method = method
		};
	}

	public override async Task<object?> Call()
	{
		var method = Data.Method.ToUpper() switch
		{
			"GET" => _httpClient.GetAsync(new Uri(_httpClient.BaseAddress!, Data.Uri)),
			_ => throw new NotImplementedException()
		};
        
		var response = await method;
		return response;
	}

	public override Task<object?> Undo()
	{
		throw new NotImplementedException();
	}
}
