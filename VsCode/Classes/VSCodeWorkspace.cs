namespace CmdPalVsCode;

/// <summary>
/// Represents a Visual Studio Code workspace.
/// </summary>
internal class VSCodeWorkspace
{
    public VSCodeInstance Instance;
    public string Path;

    /// <summary>
    /// Initializes a new instance of the <see cref="VSCodeWorkspace"/> class.
    /// </summary>
    /// <param name="instance">The VS Code instance associated with the workspace.</param>
    /// <param name="path">The path to the workspace.</param>
    public VSCodeWorkspace(VSCodeInstance instance, string path)
    {
        this.Path = path;
        this.Instance = instance;
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

        // remove .code-workspace
        workspaceName = workspaceName.Replace(".code-workspace", "");

        if (workspaceName == "workspace" && nameParts.Length >= 2)
        {
            // use folder name instead
            workspaceName = nameParts[nameParts.Length - 2];
        }
        return workspaceName;
    }

    /// <summary>
    /// Determines the type of the workspace (e.g., Local, WSL, Remote).
    /// </summary>
    /// <returns>The type of the workspace as a string.</returns>
    public string GetVSType()
    {
        string tag = "Local";
        if (Path.StartsWith("vscode-remote://wsl", System.StringComparison.OrdinalIgnoreCase))
        {
            tag = "WSL";
        }
        else if (Path.StartsWith("vscode-remote://", System.StringComparison.OrdinalIgnoreCase))
        {
            tag = "Remote";
        }

        return tag;
    }
}
