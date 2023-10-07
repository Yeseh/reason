using Reason.Client;
using Spectre.Console.Advanced;

[System.Serializable]
public class WorkspaceData
{
	public string Name { get; set;  }
	public Dictionary<string, ReasonApiData> Apis { get; set; } = new();
	public Dictionary<string, Variable> Variables { get; set; }  = new();
	public Dictionary<string, Variable> Secrets { get; set; } = new();
}

public class Workspace
{
	public string Name { get; }
	public Dictionary<string, ReasonApi> Apis { get; set; } = new();
	public Dictionary<string, Variable> Variables { get; set; }  = new();
	public Dictionary<string, Variable> Secrets { get; set; } = new();
	
	private HttpClient httpClient;

	public Workspace(string name)
	{
		Name = name;
	}

	/// <summary>
	/// Creates a workspace from a workspace data object. Primarily meant for deserialization.
	/// </summary>
	/// <param name="workspace"></param>
	/// <exception cref="Exception"></exception>
	/// <exception cref="NotSupportedException"></exception>
	internal Workspace(WorkspaceData workspace)
	{
		Name = workspace.Name;
		foreach (var data in workspace.Apis)
		{
			switch (data.Value.Type)
			{
				case "builtin":
					Apis.Add(data.Key, new BuiltinReasonApi());
					break;
				
				case "openapi":
					var d = data.Value as OpenApiReasonApiData;
					if (d == null) { throw new ReasonException("Invalid api data"); }
					Apis.Add(data.Key, new OpenApiReasonApi(d.Name, d.CommandPrefix, d.DefinitionUri, d.BaseUri, d.Spec));
					break;
				
				default:
					throw new NotSupportedException("Unknown api type " + data.Value.Type);
			}
		}
	}
	
	public Workspace RegisterBuiltins()
	{
		Apis.Add("rsn", new BuiltinReasonApi());
		return this;
	}
    
	public Workspace RegisterApi(ReasonApi api)
	{
		Apis.Add(api.Data.CommandPrefix, api);
		return this;
	}
    
	public async Task Init()
	{
		var tasks = Apis.Select(a => a.Value.Init());
		await Task.WhenAll(tasks);
	}
    
	public ReasonApi? GetApi(string prefix)
	{
		return Apis.TryGetValue(prefix, out var value) ? value : null;
	}
    
	public Variable? GetVariable(string name)
	{
		return Variables.TryGetValue(name, out var value) ? value : null;
	}
    
	public Variable? GetSecret(string name)
	{
		return Secrets.TryGetValue(name, out var value) ? value : null;
	}
}
