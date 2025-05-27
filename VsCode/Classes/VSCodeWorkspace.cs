using CmdPalVsCode.Properties;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;

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
    public string WorkspaceName = "";
    public string VSTypeString = "";
    public string WorkspaceTypeString = "";
    public DetailsElement[] Details = [];

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

        SetName();
        SetVSType();
        SetWorkspaceType();
        SetMetadata();
    }

    /// <summary>
    /// Sets the name of the workspace.
    /// </summary>
    /// <returns>The name of the workspace.</returns>
    public void SetName()
    {
        WorkspaceName = "";

        // split name by / and get last part
        var nameParts = Uri.UnescapeDataString(Path).Split('/');
        if (nameParts.Length == 0)
        {
            return;
        }

        WorkspaceName = nameParts[nameParts.Length - 1];

        if (VSCodeWorkspaceType == VSCodeWorkspaceType.Workspace)
        {
            // remove .code-workspace
            WorkspaceName = WorkspaceName.Replace(".code-workspace", "");

            // if the workspace name is "workspace", use the folder name instead
            if (WorkspaceName == "workspace" && nameParts.Length >= 2)
            {
                WorkspaceName = nameParts[nameParts.Length - 2];
            }
        }
    }

    /// <summary>
    /// Determines the type of the workspace (e.g., Local, WSL, Remote).
    /// </summary>
    /// <returns>The type of the workspace as a string.</returns>
    public void SetVSType()
    {
        if (Path.StartsWith("vscode-remote://wsl", System.StringComparison.OrdinalIgnoreCase))
        {
            VSTypeString = "WSL";
        }
        else if (Path.StartsWith("vscode-remote://", System.StringComparison.OrdinalIgnoreCase))
        {
            VSTypeString = "Remote";
        }
    }

    /// <summary>
    /// Sets the workspace type (e.g., Workspace, Folder).
    /// </summary>
    /// <returns>The type of the workspace as a string.</returns>
    public void SetWorkspaceType()
    {
        switch (VSCodeWorkspaceType)
        {
            case VSCodeWorkspaceType.Workspace:
                WorkspaceTypeString = "Workspace";
                break;
            case VSCodeWorkspaceType.Folder:
                WorkspaceTypeString = "Folder";
                break;
            default:
                WorkspaceTypeString = "Unknown Type";
                break;
        }
    }


    /// <summary>
    /// Gets the details of the workspace.
    /// </summary>
    /// <returns>An array of details elements containing information about the workspace.</returns>
    public void SetMetadata()
    {
        var typeTags = new List<Tag>() { new Tag(WorkspaceTypeString) };
        if (VSTypeString != "")
        {
            typeTags.Add(new Tag(VSTypeString));
        }

        Details = new List<DetailsElement>(){
            new DetailsElement()
            {
                Key = Resource.item_details_target,
                Data = new DetailsTags() { Tags = new List<Tag>() { new Tag(Instance.Name) }.ToArray() }
            },
            new DetailsElement()
            {
                Key = Resource.item_details_type,
                Data = new DetailsTags() { Tags = typeTags.ToArray() }
            },
            new DetailsElement()
            {
                Key = Resource.item_details_path,
                Data = new DetailsLink() { Text = Uri.UnescapeDataString(Path) },
            }
        }.ToArray();
    }
}
