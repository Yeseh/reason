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

	public object Value;
    
	public bool IsSecret; 
}
