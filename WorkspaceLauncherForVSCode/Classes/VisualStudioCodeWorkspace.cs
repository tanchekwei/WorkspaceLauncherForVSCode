using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Helpers;
using WorkspaceLauncherForVSCode.Properties;

namespace WorkspaceLauncherForVSCode;

/// <summary>
/// Represents a Visual Studio Code workspace.
/// </summary>
public class VisualStudioCodeWorkspace
{
    public string? Path { get; set; }
    public string? Name { get; set; }
    public VisualStudioCodeInstance? Instance { get; set; }
    public string? WindowsPath { get; set; }
    public WorkspaceType WorkspaceType { get; set; }
    public VisualStudioCodeWorkspaceSource Source { get; set; }
    public List<string> SourcePath { get; set; } = new();
    [JsonIgnore]
    public string WorkspaceName { get; set; } = "";
    [JsonIgnore]
    public string VSTypeString { get; set; } = "";
    [JsonIgnore]
    public string WorkspaceTypeString { get; set; } = "";
    [JsonIgnore]
    public DetailsElement[] Details { get; set; } = [];
    [JsonIgnore]
    public int Frequency { get; set; } = 0;
    public DateTime LastAccessed { get; set; }
    public VisualStudioCodeWorkspace() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="VisualStudioCodeWorkspace"/> class.
    /// </summary>
    /// <param name="instance">The Visual Studio Code instance associated with the workspace.</param>
    /// <param name="path">The path to the workspace.</param>
    internal VisualStudioCodeWorkspace(VisualStudioCodeInstance instance, string path, WorkspaceType visualStudioCodeWorkspaceType, VisualStudioCodeWorkspaceSource source, string sourcePath)
    {
        this.Path = path;
        if (FileUriParser.TryConvertToWindowsPath(path, out string? windowsPath) && windowsPath != null)
        {
            this.WindowsPath = windowsPath;
        }
        else
        {
            this.WindowsPath = path;
        }
        this.Instance = instance;
        this.WorkspaceType = visualStudioCodeWorkspaceType;
        this.Source = source;
        SourcePath.Add(sourcePath);

        SetName();
        SetVSType();
        SetWorkspaceType();
        SetMetadata();
        Name = WorkspaceName;
    }

    /// <summary>
    /// Sets the name of the workspace.
    /// </summary>
    /// <returns>The name of the workspace.</returns>
    public void SetName()
    {
        if (Path == null) return;
        WorkspaceName = "";

        // split name by / and get last part
        var nameParts = Uri.UnescapeDataString(Path).Split('/');
        if (nameParts.Length == 0)
        {
            return;
        }

        WorkspaceName = nameParts[nameParts.Length - 1];

        if (WorkspaceType == WorkspaceType.Workspace)
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
        if (Path == null) return;
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
        switch (WorkspaceType)
        {
            case WorkspaceType.Workspace:
                WorkspaceTypeString = "Workspace";
                break;
            case WorkspaceType.Folder:
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
        if (Instance == null || Path == null) return;
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
