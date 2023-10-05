using System.Text.Json.Serialization;
using Reason.Client;

[System.Serializable]
public record HttpCommand : ReasonCommand<HttpResponseMessage>
{
	public string OperationPath { get; set; }
	public string Uri { get; set; }
	public  string Method { get; set; }
	
	[JsonConstructor]
	public HttpCommand(string operationPath, string uri, string method)
	{
		OperationPath = operationPath;
		Uri = uri;
		Method = new(method);
	}
	
	public HttpCommand(string operationPath, string uri, string method)
    
	public async Task<HttpResponseMessage> Call(HttpClient client)
	{
		var method = Method.ToUpper() switch
		{
			"GET" => client.GetAsync(new Uri(client.BaseAddress!, Uri)),
			_ => throw new NotImplementedException()
		};
        
		var response = await method;
		return response;
	}
}
