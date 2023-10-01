using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reason.Client;

public interface WorkspacePersister 
{
	Task SaveAsync(Workspace workspace);
	Task<Workspace?> LoadAsync(string workspace);
	bool Exists(string workspace);
}

public interface Serializer
{
	byte[] Serialize();
}

public class WindowsWorkspacePersister : WorkspacePersister 
{
	public string BasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Reason");
	public string WorkspacePath => Path.Combine(BasePath, "Workspaces");
	
	// TODO: Use Data Protection API to encrypt the workspace data
	public async Task SaveAsync(Workspace ws)
	{
		var workspaceBytes = JsonSerializer.SerializeToUtf8Bytes(ws);
		var savePath = Path.Combine(WorkspacePath, ws.Name);
		Console.WriteLine($"Persisting workspace {ws.Name} to {savePath}");
		Directory.CreateDirectory(savePath);
		
		var workspaceFile = Path.Combine(savePath, "workspace.json");
		File.WriteAllBytes(workspaceFile, workspaceBytes);
	}
	
	public async Task<Workspace?> LoadAsync(string workspace)
	{
		var loadPath = Path.Combine(WorkspacePath, workspace);
		var workspaceFile = Path.Combine(loadPath, "workspace.json");
		var workspaceBytes = await File.ReadAllBytesAsync(workspaceFile);
		var ws = JsonSerializer.Deserialize<Workspace>(workspaceBytes, JsonSerializerOptions.Default);
		
		return ws;
	}
	
	public bool Exists(string workspace)
	{
		var loadPath = Path.Combine(WorkspacePath, workspace);
		var workspaceFile = Path.Combine(loadPath, "workspace.json");
		return File.Exists(workspaceFile);
	}

	public async Task<List<Workspace>> LoadAll()
	{
		var workspacePath = Path.Combine(BasePath, "Workspaces");
		var workspaceDirs = Directory.EnumerateDirectories(workspacePath);
		
		return new();
	}
}
