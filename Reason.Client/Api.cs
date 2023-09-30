using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Reason.Client;

public class Api
{
	public readonly string Name;
	public readonly string CommandPrefix;
	public HttpClient Client = new();
	
	private Uri _definitionUri;
	private Uri _baseUri;
	private OpenApiDocument? Spec = null;

	private static readonly string[] HttpMethods = new[]
	{
		"GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "TRACE", "CONNECT"
	};
	private Dictionary<OperationPath, HttpOperation> _operations = new();

	public Api(string name, string definitionUri, string? prefix = null)
	{
		Name = name;
		CommandPrefix = prefix ?? name;
		_definitionUri = new (definitionUri);
		_baseUri = null;
	}

	public string Help()
	{
		return $"This is the help info for {Name}\n";
	}

	public HttpOperation? GetOperation(string path)
	{
		var opPath = new OperationPath(path);
		return _operations.TryGetValue(opPath, out var operation) ? operation : null;
	}

	public async Task Init()
	{
		Console.WriteLine("Initializing api " + Name);
		var stream = await Client.GetStreamAsync(_definitionUri);
		Spec = new OpenApiStreamReader().Read(stream, out var diagnostic);
		_baseUri = new Uri(Spec.Servers[0].Url);
		Client = new HttpClient() { BaseAddress = _baseUri };

		foreach (var path in Spec.Paths)
		{
			foreach (var operation in path.Value.Operations)
			{
				var opId = operation.Value.OperationId;
				var opPath = new OperationPath(opId);
				var httpOp = new HttpOperation(opPath, path.Key, operation.Key.ToString());
				Console.WriteLine($"Found operation {opPath} at {path.Key}");
				
				_operations.Add(opPath, httpOp);
			}
		}
	}
}
