using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Reason.Client;

public interface ReasonApi<TRes>
{
	Task Init();
	string Help();
	ReasonCommand<TRes>? GetCommand(OperationPath operationPath);
}

public interface ReasonCommand<TResult>
{
	public Task<TResult> Call();
	public Task<TResult> Undo();
}

[System.Serializable]
public class OpenApiReasonApi : ReasonApi<HttpResponseMessage>
{
	public string Name { get; }
	public string CommandPrefix {get;}
	public Uri DefinitionUri { get; set; }
	public Uri BaseUri { get; set; }
	[JsonIgnore]
	public OpenApiDocument? Spec { get; set; }
	public Dictionary<string, HttpCommand> Operations { get; set; } = new();
	
	[JsonIgnore]
	public HttpClient Client = new();

	[JsonIgnore]
	private static readonly string[] HttpMethods = new[]
	{
		"GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "TRACE", "CONNECT"
	};

	[JsonConstructor]
	public OpenApiReasonApi(
		string name, 
		string commandPrefix, 
		Uri definitionUri, 
		Uri baseUri, 
		Dictionary<string, HttpCommand> operations)
	{
		Name = name;
		CommandPrefix = commandPrefix;
		DefinitionUri = definitionUri;
		BaseUri = baseUri;
		Operations = operations;
	}

	public OpenApiReasonApi(
		string name, 
		Uri definitionUri, 
		string? prefix = null)
	{
		Name = name;
		CommandPrefix = prefix ?? name;
		DefinitionUri = definitionUri;
		BaseUri = null;
	}

	public string Help()
	{
		return $"This is the help info for {Name}\n";
	}

	public HttpCommand? GetOperation(string path)
	{
		var opPath = new OperationPath(path);
		return Operations.TryGetValue(opPath.Value, out var operation) ? operation : null;
	}
	
	public ReasonCommand<HttpResponseMessage>? GetCommand(OperationPath path)
	{
		return Operations.TryGetValue(path.Value, out var operation) ? operation : null;
	}

	public async Task Init()
	{
		Console.WriteLine("Initializing api " + Name);
		var stream = await Client.GetStreamAsync(DefinitionUri);
		Spec = new OpenApiStreamReader().Read(stream, out var diagnostic);
		BaseUri = new Uri(Spec.Servers[0].Url);
		Client = new HttpClient() { BaseAddress = BaseUri };

		foreach (var path in Spec.Paths)
		{
			foreach (var operation in path.Value.Operations)
			{
				var opId = operation.Value.OperationId;
				// TODO: OperationPath might not be needed if only using opId, but might be useful for de-duplication later 
				var opPath = new OperationPath(opId);
				var httpOp = new HttpCommand(opId, path.Key, operation.Key.ToString());
				Console.WriteLine($"Found operation {opPath} at {path.Key}");
				
				Operations.Add(opPath.Value, httpOp);
			}
		}
	}
}
