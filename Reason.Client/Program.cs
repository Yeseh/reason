using System.Diagnostics;
using System.Text.Json;
using Reason.Client;
using Reason.Client.Script;
using Spectre.Console;
using Spectre.Console.Cli;

// TODO: Persist workspaces to disk on error or exit
var app = new CommandApp();
app.Configure(c =>
{
    c.PropagateExceptions();
    c.AddCommand<StartCommand>("start");
});

await app.RunAsync(args);

public enum OutputStatus
{
    Success = default,
	Warning,
	Error,
    Info,
    Debug
} 

public static class Rsn 
{
    public static void Print(string message, OutputStatus status = OutputStatus.Info)
    {
        var msg = status switch
        {
            OutputStatus.Debug => Debug(message),
            OutputStatus.Warning => Warn(message),
            OutputStatus.Info => Info(message),
            OutputStatus.Error => Error (message),
            OutputStatus.Success => Success(message),
            _ => throw new UnreachableException() 
        };
        AnsiConsole.MarkupLine(msg);
    }
    public static string Msg(string message, OutputStatus status = OutputStatus.Info)
    {
        return status switch
        {
            OutputStatus.Debug => Debug(message),
            OutputStatus.Warning => Warn(message),
            OutputStatus.Info => Info(message),
            OutputStatus.Error => Error (message),
            OutputStatus.Success => Success(message),
            _ => throw new UnreachableException() 
        };
    }
    
    public static string Error(string message) => $"[red bold]ERR:[/] {message}";
    
    public static string Success(string message) => $"[green bold]SUC6:[/] {message}";
    
    public static string Warn(string message) => $"[yellow bold]WARN:[/] {message}";
    
    public static string Info(string message) => $"[blue bold]INFO:[/] {message}";
    
    public static string Debug(string message) => $"[magenta bold]DBUG:[/] {message}";
}

class StartCommand : AsyncCommand<StartCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-w|--workspace")]
        public string? Workspace { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            Rsn.Print("This command is only available in interactive mode.", OutputStatus.Error);
            return 1;
        }

        const string swaggerUrl = "http://localhost:5225/swagger/v1/swagger.json";
        if (settings.Workspace == null)
        {
            Rsn.Print("No workspace selected, selecting default workspace. This is not auto-persisted.", OutputStatus.Warning);
        }

        var workspaceName = settings.Workspace ?? "default";
        Workspace? workspace = null;
        
        // TODO: Load workspace from disk
        if (Session.WorkspaceExists(workspaceName))
        {
            Rsn.Print($"Loading workspace {workspaceName}");
            workspace = await Session.LoadWorkspace(workspaceName);
        }
        else
        {
            Rsn.Print("Creating new workspace...");
            workspace = new Workspace(workspaceName);
            var weatherApi = new OpenApiReasonApi("weather", "w", swaggerUrl);

            // TODO: Register environment, builtins based on settings
            Rsn.Print($"Registering api {weatherApi.Name}");
            workspace.RegisterApi(weatherApi);
            Session.SetActiveWorkspace(workspace);
        }

        Rsn.Print($"Registering builtin commands");
        workspace.RegisterBuiltins();
        Rsn.Print($"Initializing workspace {workspace.Name}");
        await workspace.Init();

        foreach (var api in workspace.Apis)
        {
            Rsn.Print(api.Value.Help());
        }

        Rsn.Print($"Selected workspace {workspace.Name}");

        // Autocomplete:  https://codereview.stackexchange.com/questions/139172/autocompleting-console-input
        while (true)
        {
            var input = AnsiConsole.Ask<string>($"[bold]([green]{workspace.Name}[/]) ~>[/] ");
            var bExit = input is "exit" or "quit";
            
            // TODO: Save workspace on exit
            if (bExit)
            {
                await Session.SaveWorkspace(workspace);
                break;
            }
            await RunCommand(input, workspace);
        }

        return 0;
    }
    
    private string PrettifyJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        return JsonSerializer.Serialize(doc, options);
    }

    private async Task RunCommand(string input, Workspace workspace)
    {
        try
        {
            var rsn = new Interpreter(workspace);
            var result =  await rsn.Run(input);
            
            var bMessage = result.Message != null;
            if (bMessage)
            {
                var outputMessage = Rsn.Msg(result.Message!, result.Status);
                AnsiConsole.MarkupLine(outputMessage);
            }
            
            var bJson = result.Value != null && result.SerializationHint == SerializationHint.Json;; 
            var message = bJson ? PrettifyJson(result.Value!) : result.Value;
            if (message != null)
            {
                AnsiConsole.WriteLine(message);
            }
        }
        catch (ReasonException rex)
        {
            Rsn.Error(rex.Message);
        }
    }
}