using Reason.Serialization;

namespace Reason.Sdk;

public class Result
{
	public static Result Error(params string[] errors)
	{
		var msg = string.Join("\n", errors);
		return new() { Message = msg, Status = OutputStatus.Error };
	}
	
	public static Result Success(
		string? value = null, 
		string? message = null, 
		OutputStatus status = OutputStatus.Success)
	{
		return new() { Value = value, Message = message, Status = status};
	}

	public string? Value { get; set; }
	public string? Message { get; set; }

	public bool IsError => Status == OutputStatus.Error;

	public SerializationHint SerializationHint = SerializationHint.None;
	public OutputStatus Status = OutputStatus.Success;
}
