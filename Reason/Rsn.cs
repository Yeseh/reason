using System.Diagnostics;
using Spectre.Console;

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
