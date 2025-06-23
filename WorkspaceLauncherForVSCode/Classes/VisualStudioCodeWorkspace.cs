﻿// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Helpers;
using WorkspaceLauncherForVSCode.Properties;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models;

namespace WorkspaceLauncherForVSCode;

/// <summary>
/// Represents a Visual Studio Code workspace.
/// </summary>
public class VisualStudioCodeWorkspace
{
    public string? Path { get; set; }
    public string? Name { get; set; }
    public VisualStudioCodeInstance? VSCodeInstance { get; set; }
    public VisualStudioInstance? VSInstance { get; set; }
    public string? WindowsPath { get; set; }
    public WorkspaceType WorkspaceType { get; set; }
    public VisualStudioCodeWorkspaceSource Source { get; set; }
    public List<string> SourcePath { get; set; } = new();
    public string WorkspaceName { get; set; } = "";
    public string VSTypeString { get; set; } = "";
    public string WorkspaceTypeString { get; set; } = "";
    public DetailsElement[] Details { get; set; } = [];
    public int Frequency { get; set; }
    public DateTime LastAccessed { get; set; }
    public DateTime? PinDateTime { get; set; }

    public VisualStudioCodeWorkspace() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="VisualStudioCodeWorkspace"/> class.
    /// </summary>
    /// <param name="instance">The Visual Studio Code instance associated with the workspace.</param>
    /// <param name="path">The path to the workspace.</param>
    internal VisualStudioCodeWorkspace(VisualStudioCodeInstance instance, string path, WorkspaceType visualStudioCodeWorkspaceType, VisualStudioCodeWorkspaceSource source, string sourcePath)
    {
        Path = path;
        if (FileUriParser.TryConvertToWindowsPath(path, out string? windowsPath) && windowsPath != null)
        {
            WindowsPath = windowsPath;
        }
        else
        {
            WindowsPath = path;
        }
        VSCodeInstance = instance;
        WorkspaceType = visualStudioCodeWorkspaceType;
        Source = source;
        SourcePath.Add(sourcePath);

        SetName();
        SetVSCodeType();
        SetWorkspaceType();
        SetVSCodeMetadata();
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
    public void SetVSCodeType()
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
            case WorkspaceType.Solution:
                WorkspaceTypeString = "Solution";
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
    public void SetVSCodeMetadata()
    {
        if (Path == null) return;
        var typeTags = new List<Tag>() { new Tag(WorkspaceTypeString) };
        if (VSTypeString != "")
        {
            typeTags.Add(new Tag(VSTypeString));
        }

        var detailsElements = new List<DetailsElement>();

        if (VSCodeInstance != null)
        {
            detailsElements.Add(new DetailsElement()
            {
                Key = Resource.item_details_target,
                Data = new DetailsTags() { Tags = new List<Tag>() { new Tag(VSCodeInstance.Name) }.ToArray() }
            });
        }

        detailsElements.Add(new DetailsElement()
        {
            Key = Resource.item_details_type,
            Data = new DetailsTags() { Tags = typeTags.ToArray() }
        });

        detailsElements.Add(new DetailsElement()
        {
            Key = Resource.item_details_path,
            Data = new DetailsLink() { Text = Uri.UnescapeDataString(Path) },
        });

        Details = detailsElements.ToArray();
    }

    /// <summary>
    /// Gets the details of the workspace.
    /// </summary>
    /// <returns>An array of details elements containing information about the workspace.</returns>
    public void SetVSMetadata()
    {
        if (Path == null) return;
        var typeTags = new List<Tag>() { new Tag(WorkspaceTypeString) };
        if (VSTypeString != "")
        {
            typeTags.Add(new Tag(VSTypeString));
        }

        var detailsElements = new List<DetailsElement>();

        if (VSInstance != null)
        {
            detailsElements.Add(new DetailsElement()
            {
                Key = Resource.item_details_target,
                Data = new DetailsTags() { Tags = new List<Tag>() { new Tag(VSInstance.Name) }.ToArray() }
            });
        }

        detailsElements.Add(new DetailsElement()
        {
            Key = Resource.item_details_type,
            Data = new DetailsTags() { Tags = typeTags.ToArray() }
        });

        detailsElements.Add(new DetailsElement()
        {
            Key = Resource.item_details_path,
            Data = new DetailsLink() { Text = Uri.UnescapeDataString(Path) },
        });

        Details = detailsElements.ToArray();
    }
}
