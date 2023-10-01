using Reason.Client;

[System.Serializable]
public record HttpOperation
{
	public OperationPath OperationPath;
	private string _uri;
	private HttpMethod _method;
    
	public HttpOperation(OperationPath path, string uri, string method)
	{
		OperationPath = path;
		_uri = uri;
		_method = new(method);
	}
    
	public async Task<HttpResponseMessage> Call(HttpClient client)
	{
		var method = _method.Method.ToUpper() switch
		{
			"GET" => client.GetAsync(new Uri(client.BaseAddress!, _uri)),
			_ => throw new NotImplementedException()
		};
        
		var response = await method;
		return response;
	}
}
