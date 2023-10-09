namespace Reason;

public static class Session
{
	public static Workspace? CurrentWorkspace;
	
	public static Dictionary<string, Workspace> Workspaces = new();

	public static LinkedList<string> CommandStack = new();
	
	// TODO: Make this configurable/OS specific
	private static WorkspaceStore _store = new FileSystemWorkspaceStore();
	
	public static bool IsDefaultWorkspace => CurrentWorkspace?.Name == "default";

	public static Workspace SetActiveWorkspace(Workspace workspace)
	{
		CurrentWorkspace = workspace;
		return CurrentWorkspace;
	}

	public static bool WorkspaceExists(string name)
	{
		return _store.Exists(name);
	}
	
	public static async Task<Workspace> LoadWorkspace(string name)
	{
		var bCached = Workspaces.TryGetValue(name, out var ws);
		if (bCached && ws != null) { return CurrentWorkspace = ws; }
		
		ws = await _store.LoadAsync(name) 
		         ?? throw new ReasonException($"Workspace {name} does not exist");
		
		Workspaces.Add(ws.Name, ws);
		return CurrentWorkspace = ws;
	}
	
	public static async Task SaveWorkspace(Workspace ws)
	{
		await _store.SaveAsync(ws);
	}
}
