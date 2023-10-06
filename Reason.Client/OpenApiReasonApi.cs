using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Reason.Client;

public interface ReasonApi
{
	Task Init();
	string Help();
	ReasonCommand? GetCommand(OperationPath operationPath);
}

public interface ReasonCommand
{
	// TODO: Obj ugly, find right abstraction
	Task<object?> Call();
	Task<object?> Undo();
}

public interface UndoReasonCommand : ReasonCommand
{
	Task<object?> Undo();
}

[System.Serializable]
public class OpenApiReasonApi : ReasonApi
{
	public string Name { get; }
	public string CommandPrefix {get;}
	public Uri DefinitionUri { get; set; }
	public Uri BaseUri { get; set; }
	[JsonIgnore]
	public OpenApiDocument? Spec { get; set; }
	public Dictionary<string, ReasonCommand> Commands { get; set; } = new();
	
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
		Dictionary<string, ReasonCommand> commands)
	{
		Name = name;
		CommandPrefix = commandPrefix;
		DefinitionUri = definitionUri;
		BaseUri = baseUri;
		Commands = commands;
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

	public ReasonCommand? GetCommand(string path)
	{
		var opPath = new OperationPath(path);
		return Commands.TryGetValue(opPath.Value, out var operation) ? operation : null;
	}
	
	public ReasonCommand GetCommand(OperationPath path)
	{
		return Commands.TryGetValue(path.Value, out var operation) ? operation : null;
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
				var httpOp = new HttpCommand(opId, path.Key, operation.Key.ToString(), Client);
				Console.WriteLine($"Found operation {opPath} at {path.Key}");
				
				Commands.Add(opPath.Value, httpOp);
			}
		}
	}
}
