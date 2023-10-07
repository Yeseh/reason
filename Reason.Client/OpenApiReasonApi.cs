using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Text.Json;

namespace Reason.Client;

[System.Serializable]
public class OpenApiReasonApiData : ReasonApiData
{
	public string DefinitionUri { get; set; }

	public string? BaseUri { get; set; }
	
	public string? Spec { get; set; }
	
	public OpenApiReasonApiData(
		string name, 
		string prefix, 
		string definitionUri, 
		string? baseUri = null, 
		string? spec = null) : base(name, prefix, "openapi")
	{
		DefinitionUri = definitionUri;
	}
}

public class OpenApiReasonApi : ReasonApi
{
	public sealed override OpenApiReasonApiData Data { get; }
	public OpenApiDocument? Spec { get; set; }
	
	public HttpClient Client = new();
	
	private static readonly string[] HttpMethods = new[]
	{
		"GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS", "TRACE", "CONNECT"
	};
	
	public OpenApiReasonApi(
		string name, 
		string prefix, 
		string definitionUri, 
		string? baseUri = null, 
		string? spec = null) : base(name, prefix)
	{
		Data = new OpenApiReasonApiData(name, prefix, definitionUri);
		Data.BaseUri = baseUri;
		Data.Spec = spec;
	}

	public override byte[] Serialize()
	{
		foreach (var command in Commands)
		{
			Data.Commands.Add(command.Key, command.Value.Data);
		}
		var serializedSpec = Spec.SerializeAsJson(OpenApiSpecVersion.OpenApi3_0);
		Data.Spec = serializedSpec;
		
		var serializedData = JsonSerializer.SerializeToUtf8Bytes(Data);
		return serializedData;
	}
	
	public override async Task Init(bool force = false)
	{
		var bRefetch = Data.Spec == null || force;
		if (bRefetch)
		{
			var stream = await Client.GetStreamAsync(Data.DefinitionUri);
			Spec = new OpenApiStreamReader().Read(stream, out var diagnostic);
		}
		else if (Data.Spec != null)
		{
			Spec = new OpenApiStringReader().Read(Data.Spec, out var diagnostic);
		}
		
		// TODO: Add all servers
		Data.BaseUri = Spec.Servers[0].Url;
		Client = new HttpClient() { BaseAddress = new Uri(Data.BaseUri) };

		foreach (var path in Spec.Paths)
		{
			foreach (var operation in path.Value.Operations)
			{
				var opId = operation.Value.OperationId;
				var httpOp = new HttpCommand(opId.ToLower(), path.Key, operation.Key.ToString(), Client);
				var opPath = $"{Data.CommandPrefix}.{opId}".ToLower();
				
				Commands.Add(opPath, httpOp);
			}
		}
	}
}
