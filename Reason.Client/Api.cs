using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Reason.Client;

[System.Serializable]
public class Api
{
	public string Name { get; }
	public string CommandPrefix {get;}
	public Uri DefinitionUri { get; private set; }
	public Uri BaseUri { get; private set; }
	[JsonIgnore]
	public OpenApiDocument? Spec { get; private set; }
	public Dictionary<string, HttpOperation> Operations { get; private set; } = new();
	
	[JsonIgnore]
	public HttpClient Client = new();

	[JsonIgnore]
	private static readonly string[] HttpMethods = new[]
	{
		"GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "TRACE", "CONNECT"
	};

	public Api(string name, string definitionUri, string? prefix = null)
	{
		Name = name;
		CommandPrefix = prefix ?? name;
		DefinitionUri = new (definitionUri);
		BaseUri = null;
	}

	public string Help()
	{
		return $"This is the help info for {Name}\n";
	}

	public HttpOperation? GetOperation(string path)
	{
		var opPath = new OperationPath(path);
		return Operations.TryGetValue(opPath.Value, out var operation) ? operation : null;
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
				var opPath = new OperationPath(opId);
				var httpOp = new HttpOperation(opPath, path.Key, operation.Key.ToString());
				Console.WriteLine($"Found operation {opPath} at {path.Key}");
				
				Operations.Add(opPath.Value, httpOp);
			}
		}
	}
}
