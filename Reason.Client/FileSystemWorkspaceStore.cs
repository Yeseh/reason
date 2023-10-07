using System.Text.Json;

namespace Reason.Client;

public interface WorkspaceStore 
{
	Task SaveAsync(Workspace workspace);
	Task<Workspace?> LoadAsync(string workspaceName);
	bool Exists(string workspace);
}

public interface SerializeData 
{
	byte[] Serialize();
}

public class FileSystemWorkspaceStore : WorkspaceStore 
{
	public string BasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Reason");
	public string WorkspacePath => Path.Combine(BasePath, "Workspaces");
	
	// TODO: Use Data Protection API to encrypt the workspace data
	public Task SaveAsync(Workspace ws)
	{
		var data = new WorkspaceData()
		{
			Name = ws.Name,
			Variables = ws.Variables,
			Secrets = ws.Secrets,
			Apis = ws.Apis.AsEnumerable().ToDictionary(x => x.Key, x => x.Value.Data)
		};
		
		var workspaceBytes = JsonSerializer.SerializeToUtf8Bytes(data);
		var workspaceFolder = Path.Combine(WorkspacePath, ws.Name);
		Directory.CreateDirectory(workspaceFolder);
		Console.WriteLine($"Persisting workspace {ws.Name} to {workspaceFolder}");
		
		var workspaceFile = Path.Combine(workspaceFolder, "workspace.json");
		File.WriteAllBytes(workspaceFile, workspaceBytes);
		
		return Task.CompletedTask;
	}
	
	public async Task<Workspace?> LoadAsync(string workspaceName)
	{
		var loadPath = Path.Combine(WorkspacePath, workspaceName);
		var workspaceFile = Path.Combine(loadPath, "workspace.json");
		var workspaceBytes = await File.ReadAllBytesAsync(workspaceFile);
		
		var data = JsonSerializer.Deserialize<WorkspaceData>(
			workspaceBytes, 
			JsonSerializerOptions.Default);

		if (data == null)
		{
			throw new ReasonException($"Failed to deserialize workspace {workspaceName} from {workspaceFile}");
		}
		
		var workspace = new Workspace(data);
		
		return workspace;
	}
	
	public bool Exists(string workspace)
	{
		var loadPath = Path.Combine(WorkspacePath, workspace);
		var workspaceFile = Path.Combine(loadPath, "workspace.json");
		return File.Exists(workspaceFile);
	}
}
