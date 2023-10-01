using Reason.Client;

public static class Session
{
	public static Workspace? CurrentWorkspace;
	// TODO: Make this configurable/OS specific
	public static WorkspacePersister Persister = new WindowsWorkspacePersister();
    
	public static Workspace SelectWorkspace(string name)
	{
		CurrentWorkspace = new Workspace(name);
		return CurrentWorkspace;
	}
}
