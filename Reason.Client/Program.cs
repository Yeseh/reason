using Spectre.Console;
using Spectre.Console.Cli;

// TODO: Persist workspaces to disk on error or exit

var app = new CommandApp();
app.Configure(c =>
{
    c.AddCommand<DefaultCommand>("start");
});

await app.RunAsync(args);

class DefaultCommand : AsyncCommand
{
    public async override Task<int> ExecuteAsync(CommandContext context)
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] This command is only available in interactive mode.");
            return 1;
        }
        
        // TEMP SETUP
        const string swaggerUrl = "http://localhost:5225/swagger/v1/swagger.json";
        var workspace = Session.SelectWorkspace("default");
        workspace.RegisterApi("weather", "w", swaggerUrl);

        await workspace.Init();
        
        // Autocomplete:  https://codereview.stackexchange.com/questions/139172/autocompleting-console-input
        while (true)
        {
            var command = AnsiConsole.Ask<string>("~> ");
            if (command == "exit") { break; }
            
            // TODO: Run command parser, for now only api command

            var split = command.Split('.');
            var prefix = split[0];
            var api = workspace.GetApi(prefix);
            
            if (api == null)
            {
                AnsiConsole.MarkupLine($"[red]ERR:[/] No api registered with prefix '{prefix}'");
                continue;
            }
            
            if (split.Length == 1)
            {
                AnsiConsole.MarkupLine(api.Help());
                continue;
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
            AnsiConsole.MarkupLine(result.IsSuccessStatusCode ? $"[green]Success:[/] {result.StatusCode}" : $"[red]Error:[/] {result.StatusCode}");
            if (result.IsSuccessStatusCode)
            {
                AnsiConsole.WriteLine(await result.Content.ReadAsStringAsync());
            }
        }

        return 0;
    }
}