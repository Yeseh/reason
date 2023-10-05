using System.Text.Json.Serialization;

[System.Serializable]
public class Workspace
{
	public string Name { get; }
	public Dictionary<string, OpenApiReasonApi> Apis { get; set; } = new();
	public Dictionary<string, Variable> Variables { get; set; }  = new();
	public Dictionary<string, Variable> Secrets { get; set; } = new();
	
	[JsonIgnore]
	private HttpClient httpClient;

	[JsonConstructor]
	public Workspace(
		string name, 
		Dictionary<string, OpenApiReasonApi> apis, 
		Dictionary<string, Variable> variables,
		Dictionary<string, Variable> secrets)
	{
		Name = name;
		Apis = apis;
		Variables = variables;
		Secrets = secrets;	
	}

	public Workspace(string name)
	{
		Name = name;
	}
    
	public Workspace RegisterApi(string name, string prefix, string url)
	{
		Console.WriteLine($"Registering api {name}");
		var api = new OpenApiReasonApi(name, new Uri(url), prefix);
		Apis.Add(api.CommandPrefix, api);
		return this;
	}
    
	public async Task Init()
	{
		Console.WriteLine("Initializing workspace " + Name);
		var tasks = Apis.Select<KeyValuePair<string, OpenApiReasonApi>,Task>(a => a.Value.Init());
		await Task.WhenAll(tasks);
	}
    
	public OpenApiReasonApi? GetApi(string prefix)
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
