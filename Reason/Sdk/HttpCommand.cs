
namespace Reason.Sdk;

using Reason.Serialization;
using Newtonsoft.Json;

[System.Serializable]
public class HttpCommand : Command
{
	public string Uri { get; set; } = string.Empty;
	public string Method { get; set; } = string.Empty;

	[JsonIgnore]
	private HttpClient _httpClient = new();
	
	public HttpCommand() : base()
	{
	}
	
	public HttpCommand(string operationPath, string uri, string method, HttpClient client) : base(operationPath, "http")
	{
		_httpClient = client;
		Uri = uri;
		Method = method;
	}

	public override async Task<Result> Call()
	{
		Result result;
		var method = Method.ToUpper() switch
		{
			"GET" => _httpClient.GetAsync(new Uri(_httpClient.BaseAddress!, Uri)),
			_ => throw new NotImplementedException()
		};
        
		var commandResult = await method;
		
		var bSucces = commandResult is { IsSuccessStatusCode: true };
		if (bSucces)
		{
			var content = await commandResult.Content.ReadAsStringAsync();
			result = new Result()
			{
				Value = content,
				SerializationHint = SerializationHint.Json
			};
		}
		else
		{
			var content = await commandResult.Content.ReadAsStringAsync();
			result = Result.Error($"{commandResult.StatusCode} {content}");
		}
		
		return result;
	}

	public override Task<Result> Undo()
	{
		throw new NotImplementedException();
	}
}
