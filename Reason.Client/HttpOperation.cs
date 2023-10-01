using System.Text.Json.Serialization;
using Reason.Client;

[System.Serializable]
public record HttpOperation
{
	public OperationPath OperationPath { get; set; }
	public string Uri { get; set; }
	public HttpMethod Method { get; set; }
	
	[JsonConstructor]
	public HttpOperation(string operationPath, string uri, string method)
	{
		OperationPath = new OperationPath(operationPath);
		Uri = uri;
		Method = new(method);
	}
    
	public async Task<HttpResponseMessage> Call(HttpClient client)
	{
		var method = Method.Method.ToUpper() switch
		{
			"GET" => client.GetAsync(new Uri(client.BaseAddress!, Uri)),
			_ => throw new NotImplementedException()
		};
        
		var response = await method;
		return response;
	}
}
