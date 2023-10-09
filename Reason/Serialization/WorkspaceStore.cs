namespace Reason;

public interface WorkspaceStore 
{
	Task SaveAsync(Workspace workspace);
	Task<Workspace?> LoadAsync(string workspaceName);
	bool Exists(string workspace);
}