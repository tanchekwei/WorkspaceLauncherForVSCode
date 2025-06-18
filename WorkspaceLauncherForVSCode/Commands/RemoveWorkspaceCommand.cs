using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Workspaces.Readers;

namespace WorkspaceLauncherForVSCode.Commands
{
    internal sealed partial class RemoveWorkspaceCommand : InvokableCommand
    {
        private readonly VisualStudioCodeWorkspace workspace;
        private readonly VisualStudioCodePage page;

        public RemoveWorkspaceCommand(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page)
        {
            this.workspace = workspace;
            this.page = page;
        }

        public override string Name => "Remove from List";

        public override CommandResult Invoke()
        {
            _ = Task.Run(async () =>
            {
                var tasks = new List<Task<int>>();

                if (workspace.Source == VisualStudioCodeWorkspaceSource.StorageJson ||
                    workspace.Source == VisualStudioCodeWorkspaceSource.StorageJsonVscdb)
                {
                    tasks.Add(StorageJsonWorkspaceReader.RemoveWorkspaceAsync(workspace));
                }

                if (workspace.Source == VisualStudioCodeWorkspaceSource.Vscdb ||
                    workspace.Source == VisualStudioCodeWorkspaceSource.StorageJsonVscdb)
                {
                    tasks.Add(VscdbWorkspaceReader.RemoveWorkspaceAsync(workspace));
                }

                if (workspace.Source == VisualStudioCodeWorkspaceSource.Unknown)
                {
                    throw new NotImplementedException();
                }

                try
                {
                    int[] results = await Task.WhenAll(tasks);

                    if (results.All(r => r == 1))
                    {
                        page.RemoveWorkspace(workspace);
                        new ToastStatusMessage($"Removed \"{workspace.WorkspaceName}\"").Show();
                    }
                    else
                    {
                        // Optional: log or notify which result(s) failed
                        new ToastStatusMessage($"Failed to remove \"{workspace.WorkspaceName}\"").Show();
                    }
                }
                catch (Exception ex)
                {
                    // Optional: log exception
                    new ToastStatusMessage($"Error removing \"{workspace.WorkspaceName}\": {ex.Message}").Show();
                }
            });

            return CommandResult.KeepOpen();
        }
    }
}