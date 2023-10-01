using Reason.Client;

public static class Session
{
	public static Workspace? CurrentWorkspace;
	
	public static Dictionary<string, Workspace> Workspaces = new();
	
	// TODO: Make this configurable/OS specific
	public static WorkspacePersister Persister = new WindowsWorkspacePersister();
	
	public static bool IsDefaultWorkspace => CurrentWorkspace?.Name == "default";

	public static Workspace SelectWorkspace(Workspace workspace)
	{
		CurrentWorkspace = workspace;
		return CurrentWorkspace;
	}
	
	public static async Task<Workspace> SelectWorkspace(string name)
	{
		var bCached = Workspaces.TryGetValue(name, out var ws);
		if (bCached) { return CurrentWorkspace = ws; }
		
		ws = await Persister.LoadAsync(name) 
		         ?? throw new ReasonException($"Workspace {name} does not exist");
		
		Workspaces.Add(ws.Name, ws);
		return CurrentWorkspace = ws;
	}
}
