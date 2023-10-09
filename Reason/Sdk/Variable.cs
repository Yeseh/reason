
namespace Reason;

[System.Serializable]
public class Variable
{
	public enum VariableType
	{
		String,
		Number,
		Boolean,
		Object,
		Array
	}

	public object? Value = null;
    
	public bool IsSecret; 
}