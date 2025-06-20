// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Workspaces
{
    public static class WorkspaceItemFactory
    {
        public static ListItem Create(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page, SettingsManager settingsManager, CommandContextItem refreshCommandContextItem, CommandContextItem openExtensionSettingsLogsCommandContextItem)
        {
            ICommand command;
            IconInfo icon;
            Details details;
            var tags = new List<Tag>();
            IContextItem[] moreCommands;

            switch (workspace.WorkspaceType)
            {
                case WorkspaceType.Solution:
                    command = new OpenSolutionCommand(workspace, page);
                    icon = Classes.Icon.VisualStudio;
                    workspace.WindowsPath = workspace.Path;
                    details = new Details
                    {
                        Title = workspace.Name ?? string.Empty,
                        HeroImage = icon,
                        Metadata = workspace.Details,
                    };
                    if (settingsManager.TagTypes.HasFlag(TagType.Target))
                    {
                        if (workspace.VSInstance?.Name is string name)
                        {
                            tags.Add(new Tag(name));
                        }
                    }
                    moreCommands = [
                        new CommandContextItem(new OpenInExplorerCommand(workspace.WindowsPath ?? string.Empty, workspace, page)),
                        new CommandContextItem(new CopyPathCommand(workspace.WindowsPath ?? string.Empty)),
                        refreshCommandContextItem,
#if DEBUG
                        openExtensionSettingsLogsCommandContextItem,
#endif
                    ];
                    break;
                default:
                    command = new OpenVisualStudioCodeCommand(workspace, page, settingsManager.CommandResult);
                    icon = Classes.Icon.VisualStudioCode;
                    details = new Details
                    {
                        Title = workspace.WorkspaceName,
                        HeroImage = icon,
                        Metadata = workspace.Details,
                    };
                    if (settingsManager.TagTypes.HasFlag(TagType.Type))
                    {
                        tags.Add(new Tag(workspace.WorkspaceTypeString));
                        if (!string.IsNullOrEmpty(workspace.VSTypeString))
                        {
                            tags.Add(new Tag(workspace.VSTypeString));
                        }
                    }
                    if (settingsManager.TagTypes.HasFlag(TagType.Target))
                    {
                        if (workspace.VSCodeInstance?.Name is string name)
                        {
                            tags.Add(new Tag(name));
                        }
                    }
                    moreCommands = [
                        new CommandContextItem(new OpenInExplorerCommand(workspace.WindowsPath ?? string.Empty, workspace, page)),
                        new CommandContextItem(new CopyPathCommand(workspace.WindowsPath ?? string.Empty)),
                        new CommandContextItem(new RemoveWorkspaceCommandConfirmation(workspace, page)),
                        refreshCommandContextItem,
#if DEBUG
                        openExtensionSettingsLogsCommandContextItem,
#endif

                    ];
                    break;
            }

            var item = new ListItem(command)
            {
                Title = details.Title ?? "(no title)",
                Subtitle = !string.IsNullOrEmpty(workspace.WindowsPath) ? Uri.UnescapeDataString(workspace.WindowsPath) : workspace.WindowsPath ?? string.Empty,
                Details = details,
                Icon = icon,
                Tags = tags.ToArray(),
                MoreCommands = moreCommands
            };
            return item;
        }
    }
}