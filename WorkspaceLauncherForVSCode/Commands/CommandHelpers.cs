// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Commands
{
    internal static class CommandHelpers
    {
        public static CommandResult? IsPathNotFound(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
            {
                new ToastStatusMessage($"Path does not exist").Show();
                return CommandResult.KeepOpen();
            }
            return null;
        }
    }
}