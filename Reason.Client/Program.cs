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

public static class RSN 
{
    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red bold]ERR:[/] {message}");
    }
    
    public static void Warn(string message)
    {
        AnsiConsole.MarkupLine($"[yellow bold]WARN:[/] {message}");
    }
    
    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"[blue bold]INFO:[/] {message}");
    }
    
    public static void Debug(string message)
    {
        AnsiConsole.MarkupLine($"[magenta bold]DBUG:[/] {message}");
    }
}

class StartCommand : AsyncCommand<StartCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-w|--workspace")]
        public string? Workspace { get; set; }
    }
    
    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            RSN.Error("This command is only available in interactive mode.");
            return 1;
        }
        
        const string swaggerUrl = "http://localhost:5225/swagger/v1/swagger.json";
        if (settings.Workspace == null)
        {
            RSN.Warn("No workspace selected, selecting default workspace. This is not auto-persisted.");
        }
        
        var workspaceName = settings.Workspace ?? "default";
        Workspace? workspace = null;
        if (Session.Persister.Exists(workspaceName))
        {
            AnsiConsole.MarkupLine("Loading existing workspace...");
            workspace = await Session.SelectWorkspace(workspaceName);
            // TODO: Always re-init workspace with option
        }
        else
        {
            AnsiConsole.MarkupLine("Creating new workspace...");
            workspace = new Workspace(workspaceName);
            
            // TODO: Register environment, builtins based on settings
            workspace.RegisterApi("weather", "w", swaggerUrl);
            
            await workspace.Init();
            Session.SelectWorkspace(workspace);
        }
        
        // Autocomplete:  https://codereview.stackexchange.com/questions/139172/autocompleting-console-input
        while (true)
        {
            var command = AnsiConsole.Ask<string>("~> ");
            var bExit = command is "exit" or "quit";
            if (bExit)
            {
                if (Session.CurrentWorkspace != null && !Session.IsDefaultWorkspace)
                {
                    await Session.Persister.SaveAsync(Session.CurrentWorkspace!);
                }
                break;
            }

            // TODO: Run command parser, for now only api command
            var split = command.Split('.');
            var prefix = split[0];
            var api = workspace.GetApi(prefix);
            if (api == null)
            {
                RSN.Error($"No api registered with prefix '{prefix}'");
                continue;
            }

            if (split.Length == 1)
            {
                AnsiConsole.MarkupLine(api.Help());
            }

            var operationPath = string.Join('.', split[1..]);
            var operation = api.GetOperation(operationPath);
            if (operation == null)
            {
                AnsiConsole.MarkupLine($"[red]ERR:[/] No operation found with path '{operationPath}'");
                AnsiConsole.MarkupLine(api.Help());
                continue;
            }

            var result = await operation.Call(api.Client);
            AnsiConsole.MarkupLine(result.IsSuccessStatusCode
                ? $"[green]Success:[/] {result.StatusCode}"
                : $"[red]Error:[/] {result.StatusCode}");
            if (result.IsSuccessStatusCode)
            {
                AnsiConsole.WriteLine(await result.Content.ReadAsStringAsync());
            }
        }

        return 0;
    }
}