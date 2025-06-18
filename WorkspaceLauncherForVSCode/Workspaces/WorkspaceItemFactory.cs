using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Workspaces
{
    public static class WorkspaceItemFactory
    {
        public static ListItem Create(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page, SettingsManager settingsManager, CommandContextItem refreshCommandContextItem)
        {
            var command = new OpenVisualStudioCodeCommand(workspace, page, settingsManager.CommandResult);
            var details = new Details
            {
                Title = workspace.WorkspaceName,
                HeroImage = VisualStudioCode.IconInfo,
                Metadata = workspace.Details,
            };

            var tags = new List<Tag>();
            if (settingsManager.TagTypes.HasFlag(TagType.Type))
            {
                tags.Add(new Tag(workspace.WorkspaceTypeString));
                if (!string.IsNullOrEmpty(workspace.VSTypeString))
                {
                    tags.Add(new Tag(workspace.VSTypeString));
                }
            }

            //if (settingsManager.TagTypes.HasFlag(TagType.Target))
            //{
            //    tags.Add(new Tag(workspace.Instance.Name));
            //}
            return new ListItem(command)
            {
                Title = details.Title,
                Subtitle = !string.IsNullOrEmpty(workspace.WindowsPath) ? Uri.UnescapeDataString(workspace.WindowsPath) : workspace.WindowsPath ?? string.Empty,
                Details = details,
                Icon = VisualStudioCode.IconInfo,
                Tags = tags.ToArray(),
                MoreCommands = new IContextItem[]
                {
                    new CommandContextItem(new OpenInExplorerCommand(workspace.WindowsPath ?? string.Empty, workspace, page)),
                    new CommandContextItem(new CopyPathCommand(workspace.WindowsPath ?? string.Empty)),
                    new CommandContextItem(new RemoveWorkspaceCommandConfirmation(workspace, page)),
                    refreshCommandContextItem,
                }
            };
        }
    }
}