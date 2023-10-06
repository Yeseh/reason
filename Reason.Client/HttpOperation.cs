using System.Text.Json;
using System.Text.Json.Serialization;
using Reason.Client;

[System.Serializable]
public struct ReasonCommandData
{
	public string OperationPath { get; set; }
	public string Uri { get; set; }
	public string Method { get; set; }
}

public record HttpCommand : ReasonCommand, SerializeData
{
	private ReasonCommandData Data { get; }

	private HttpClient _httpClient = new();
	
	public HttpCommand(string operationPath, string uri, string method, HttpClient client)
	{
		_httpClient = client;
		Data = new ReasonCommandData()
		{
			OperationPath = operationPath,
			Uri = uri,
			Method = method
		};
	}

	public async Task<object?> Call()
	{
		var method = Data.Method.ToUpper() switch
		{
			"GET" => _httpClient.GetAsync(new Uri(_httpClient.BaseAddress!, Data.Uri)),
			_ => throw new NotImplementedException()
		};
        
		var response = await method;
		return response;
	}

	public Task<object?> Undo()
	{
		throw new NotImplementedException();
	}

	public byte[] SerializeData()
	{
		var data = JsonSerializer.SerializeToUtf8Bytes(Data);
		return data;
	}
}
