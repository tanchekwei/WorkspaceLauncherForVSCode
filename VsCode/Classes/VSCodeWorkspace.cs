namespace CmdPalVsCode;

/// <summary>
/// Enum for type of Visual Studio Code.
/// </summary>
enum VSCodeWorkspaceType
{
    Workspace,
    Folder
}


/// <summary>
/// Represents a Visual Studio Code workspace.
/// </summary>
internal class VSCodeWorkspace
{
    public VSCodeInstance Instance;
    public string Path;
    public VSCodeWorkspaceType VSCodeWorkspaceType;

    /// <summary>
    /// Initializes a new instance of the <see cref="VSCodeWorkspace"/> class.
    /// </summary>
    /// <param name="instance">The VS Code instance associated with the workspace.</param>
    /// <param name="path">The path to the workspace.</param>
    public VSCodeWorkspace(VSCodeInstance instance, string path, VSCodeWorkspaceType vsCodeWorkspaceType)
    {
        this.Path = path;
        this.Instance = instance;
        this.VSCodeWorkspaceType = vsCodeWorkspaceType;
    }

    /// <summary>
    /// Gets the name of the workspace.
    /// </summary>
    /// <returns>The name of the workspace.</returns>
    public string GetName()
    {
        string workspaceName = "";

        // split name by / and get last part
        var nameParts = Path.Split('/');
        if (nameParts.Length == 0)
        {
            return workspaceName;
        }

        workspaceName = nameParts[nameParts.Length - 1];

        if (VSCodeWorkspaceType == VSCodeWorkspaceType.Workspace)
        {
            // remove .code-workspace
            workspaceName = workspaceName.Replace(".code-workspace", "");

            // if the workspace name is "workspace", use the folder name instead
            if (workspaceName == "workspace" && nameParts.Length >= 2)
            {
                workspaceName = nameParts[nameParts.Length - 2];
            }
        }

        return workspaceName;
    }

    /// <summary>
    /// Determines the type of the workspace (e.g., Local, WSL, Remote).
    /// </summary>
    /// <returns>The type of the workspace as a string.</returns>
    public string GetVSType()
    {
        if (Path.StartsWith("vscode-remote://wsl", System.StringComparison.OrdinalIgnoreCase))
        {
            return "WSL";
        }
        else if (Path.StartsWith("vscode-remote://", System.StringComparison.OrdinalIgnoreCase))
        {
            return "Remote";
        }

        return "";
    }

    /// <summary>
    /// Gets the workspace type (e.g., Workspace, Folder).
    /// </summary>
    /// <returns>The type of the workspace as a string.</returns>
    public string GetWorkspaceType()
    {
        switch (VSCodeWorkspaceType)
        {
            case VSCodeWorkspaceType.Workspace:
                return "Workspace";
            case VSCodeWorkspaceType.Folder:
                return "Folder";
            default:
                return "Unknown Type";
        }
    }
}
