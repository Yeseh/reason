namespace Reason.Sdk;

public class ReasonCommandFactory
{
	public static Command Create(string type)
	{
		return type switch
		{
			"builtin" => new Command(),
			"http" => new HttpCommand(),
			_ => throw new NotSupportedException($"Unknown command type " + type)
		};
	}
}
