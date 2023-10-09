using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json;

namespace Reason.Sdk;

[System.Serializable]
public class OpenApiReasonApi : ReasonApi
{
	public string DefinitionUri { get; set; } = string.Empty;

	public string? BaseUri { get; set; } = string.Empty;
	
	public string? Spec { get; set; }
	
	[JsonIgnore]
	public HttpClient Client = new();
	
	private static readonly string[] HttpMethods = new[]
	{
		"GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "TRACE", "CONNECT"
	};
	
	public OpenApiReasonApi() : base()
	{
	}
	
	public OpenApiReasonApi(
		string name, 
		string prefix, 
		string definitionUri, 
		string? baseUri = null, 
		OpenApiDocument? spec = null) : base(name, prefix, "openapi")
	{
		DefinitionUri = definitionUri;
		BaseUri = baseUri;
	}
	
	public override async Task Init(bool force = false)
	{
		OpenApiDocument? document = null;
		var bRefetch = Spec == null || force;
		if (bRefetch)
		{
			var stream = await Client.GetStreamAsync(DefinitionUri);
			document = new OpenApiStreamReader().Read(stream, out var _);
		}
		else if (Spec == null)
		{
			document = new OpenApiStringReader().Read(Spec, out var _);
		}
		
		if (document == null)
		{
			throw new Exception("Failed to parse OpenAPI document");
		}
		
		// TODO: Add all servers
		BaseUri = document.Servers[0].Url;
		Client = new HttpClient() { BaseAddress = new Uri(BaseUri) };

		foreach (var path in document.Paths)
		{
			foreach (var operation in path.Value.Operations)
			{
				var opId = operation.Value.OperationId;
				var httpOp = new HttpCommand(opId.ToLower(), path.Key, operation.Key.ToString(), Client);
				var opPath = $"{CommandPrefix}.{opId}".ToLower();
				
				Commands.Add(opPath, httpOp);
			}
		}
	}
}