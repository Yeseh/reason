using System.Text;
using System.Text.Json;

namespace Reason.Client;

public interface WorkspacePersister 
{
	void Persist(Workspace workspace);
	void Load(string workspace);
}

public interface Serializer
{
	byte[] Serialize();
}

public class WindowsWorkspacePersister: WorkspacePersister 
{
	public string BasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Reason");
	
	// TODO: Use Data Protection API to encrypt the workspace data
	public void Persist(Workspace ws)
	{
		var workspaceBytes = JsonSerializer.SerializeToUtf8Bytes(ws);
		var workspacePath = Path.Combine(BasePath, "Workspaces", ws.Name);
		Console.WriteLine($"Persisting workspace {ws.Name} to {workspacePath}");
		Directory.CreateDirectory(workspacePath);
		
		var workspaceFile = Path.Combine(workspacePath, "workspace.json");
		File.WriteAllBytes(workspaceFile, workspaceBytes);
	}
	
	public void Load(string workspace)
	{
		throw new NotImplementedException();
	}
}
