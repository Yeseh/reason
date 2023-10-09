using Newtonsoft.Json;
using Reason.Sdk;

namespace Reason;

[System.Serializable]
public class Workspace
{
	public string Name { get; } = string.Empty;
	public Dictionary<string, ReasonApi> Apis { get; set; } = new();
	public Dictionary<string, Variable> Variables { get; set; }  = new();
	public Dictionary<string, Variable> Secrets { get; set; } = new();
	
	public Workspace(string name)
	{
		Name = name;
	}

	public Workspace RegisterBuiltins()
	{
		Apis.Add("rsn", new BuiltinReasonApi());
		return this;
	}
    
	public Workspace RegisterApi(ReasonApi api)
	{
		Apis.Add(api.CommandPrefix, api);
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
