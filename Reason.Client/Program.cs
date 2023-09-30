using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

const string swaggerUrl = "http://localhost:5225/swagger/v1/swagger.json";

var defaultWorkspace = new Workspace("default");
defaultWorkspace.RegisterApi("weather", "w", swaggerUrl);

class Variable
{
    public enum VariableType
    {
        String,
        Number,
        Boolean,
        Object,
        Array
    }

    public object Value;
    
    public 
}

class Api
{
    public string Name;
    public string CommandPrefix;
    public string Path;
    public OpenApiDocument? Spec = null;

    public Api(string name, string path, string prefix = null 
    {
        Name = name;
        CommandPrefix = ;
        Path = path;
    }
}

class Workspace
{
    public string Name;
    public List<Api> Apis;
    public Dictionary<string, Variable> Variables;
    public Dictionary<string, Variable> Secrets;
    private HttpClient httpClient;

    public Workspace(string name)
    {
        Name = name;
    }
    
    public Workspace RegisterApi(string name, string prefix, string url)
    {
        var stream = httpClient.GetStreamAsync(url).Result;
        var document = new OpenApiStreamReader().Read(stream, out var diagnostic);
        var api = new Api(name, prefix, url);
        api.Spec = document;
        
        Apis.Add(api);
        return this;
    }
}