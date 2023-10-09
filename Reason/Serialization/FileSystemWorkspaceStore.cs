using System.Text;
using Newtonsoft.Json;
using Reason.Serialization;

namespace Reason;

public class FileSystemWorkspaceStore : WorkspaceStore 
{
	private string BasePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Reason");
	private string WorkspacePath => Path.Combine(BasePath, "Workspaces");
	
	// TODO: Use Data Protection API to encrypt the workspace data
	public Task SaveAsync(Workspace ws)
	{
		var workspaceJson = JsonConvert.SerializeObject(ws);
		if (workspaceJson == null)
		{
			throw new ReasonException($"Failed to serialize workspace {ws.Name}");
		}
		
		var workspaceBytes = Encoding.UTF8.GetBytes(workspaceJson);
		var workspaceFolder = Path.Combine(WorkspacePath, ws.Name);
		Directory.CreateDirectory(workspaceFolder);
		Rsn.Info($"Persisting workspace {ws.Name} to {workspaceFolder}");
		
		var workspaceFile = Path.Combine(workspaceFolder, "workspace.json");
		File.WriteAllBytes(workspaceFile, workspaceBytes);
		
		return Task.CompletedTask;
	}
	
	public async Task<Workspace?> LoadAsync(string workspaceName)
	{
		var loadPath = Path.Combine(WorkspacePath, workspaceName);
		var workspaceFile = Path.Combine(loadPath, "workspace.json");
		var workspaceJson = await File.ReadAllTextAsync(workspaceFile);
		
		var workspace = JsonConvert.DeserializeObject<Workspace>(workspaceJson, 
			new ReasonApiJsonConverter(),
			new ReasonCommandJsonConverter());

		if (workspace == null)
		{
			throw new ReasonException($"Failed to deserialize workspace {workspaceName} from {workspaceFile}");
		}
		
		return workspace;
	}
	
	public bool Exists(string workspace)
	{
		var loadPath = Path.Combine(WorkspacePath, workspace);
		var workspaceFile = Path.Combine(loadPath, "workspace.json");
		return File.Exists(workspaceFile);
	}
}
