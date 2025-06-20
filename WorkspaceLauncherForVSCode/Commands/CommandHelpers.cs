// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Commands
{
    internal static class CommandHelpers
    {
        public static CommandResult? GetPathNotFoundResult(string path, VisualStudioCodeWorkspace? workspace, VisualStudioCodePage page)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                if (workspace?.WorkspaceType == Enums.WorkspaceType.Solution)
                {
                    return CommandResult.ShowToast("Path does not exist");
                }
                else if (workspace != null)
                {
                    var confirmArgs = new ConfirmationArgs()
                    {
                        Title = $"Path does not exist, do you want to remove from list?",
                        Description = $"{path}",
                        PrimaryCommand = new RemoveWorkspaceCommandConfirmation(workspace, page),
                        IsPrimaryCommandCritical = true,
                    };
                    return CommandResult.Confirm(confirmArgs);
                }
            }

            return null;
        }
    }
}