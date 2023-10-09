using Spectre.Console.Cli;

// TODO: Persist workspaces to disk on error or exit
var app = new CommandApp();
app.Configure(c =>
{
    c.PropagateExceptions();
    c.AddCommand<StartCommand>("start");
});

await app.RunAsync(args);