public static class Session
{
	public static Workspace? CurrentWorkspace;
    
	public static Workspace SelectWorkspace(string name)
	{
		CurrentWorkspace = new Workspace(name);
		return CurrentWorkspace;
	}
}
