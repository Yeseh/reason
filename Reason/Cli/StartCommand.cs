using System.Text.Json;
using Reason;
using Reason.Script;
using Reason.Sdk;
using Reason.Serialization;
using Spectre.Console;
using Spectre.Console.Cli;

class StartCommand : AsyncCommand<StartCommand.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandOption("-w|--workspace")]
		public string? Workspace { get; set;  }
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
        
        
		Console.Clear();

		var workspaceName = settings.Workspace ?? "default";
		Workspace? workspace;
        
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
        
		var builtins = workspace.GetApi("rsn");
		if (builtins == null)
		{
			Rsn.Print("Registering builtin commands");
			workspace.RegisterBuiltins();
		}

		Rsn.Print($"Initializing workspace {workspace.Name}");
		await workspace.Init();

		foreach (var api in workspace.Apis)
		{
			Rsn.Print(api.Value.Help());
		}

		Rsn.Print($"Selected workspace {workspace.Name}");
        
		async Task Exit()
		{
			Rsn.Print("Saving workspace...");
			await Session.SaveWorkspace(workspace);
			Rsn.Print("Exiting...");
		}
        
		Console.CancelKeyPress += async (_, _) => await Exit();
        
		// Autocomplete:  https://codereview.stackexchange.com/questions/139172/autocompleting-console-input
		while (true)
		{
			var input = AnsiConsole.Ask<string>($"[bold]([green]{workspace.Name}[/]) ~>[/] ");
			var bExit = input is "exit" or "quit";
            
			if (bExit) { await Exit(); break; }
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
            
			var bJson = result.Value != null && result.SerializationHint == SerializationHint.Json;
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