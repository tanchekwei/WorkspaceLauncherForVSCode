using System.IO;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Commands
{
    internal static class CommandHelpers
    {
        public static CommandResult? GetPathNotFoundResult(string path, VisualStudioCodeWorkspace workspace, VisualStudioCodePage page)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
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

            return null;
        }
    }
}