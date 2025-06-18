using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Workspaces.Readers;

namespace WorkspaceLauncherForVSCode.Services
{
    public static class VisualStudioCodeWorkspaceProvider
    {
        public static async Task<IEnumerable<VisualStudioCodeWorkspace>> GetWorkspacesAsync(VisualStudioCodeInstance instance, WorkspaceStorage workspaceStorage, CancellationToken cancellationToken)
        {
            // using var logger = new TimeLogger();
            if (cancellationToken.IsCancellationRequested || !File.Exists(instance.ExecutablePath))
            {
                return Enumerable.Empty<VisualStudioCodeWorkspace>();
            }

            var vscdbTask = VscdbWorkspaceReader.GetWorkspacesAsync(instance, cancellationToken);
            var storageJsonTask = StorageJsonWorkspaceReader.GetWorkspacesAsync(instance, cancellationToken);

            await Task.WhenAll(vscdbTask, storageJsonTask);

            var allWorkspaces = new ConcurrentBag<VisualStudioCodeWorkspace>(vscdbTask.Result.Concat(storageJsonTask.Result));
            var dbWorkspaces = await workspaceStorage.GetWorkspacesAsync();
            foreach (var dbWorkspace in dbWorkspaces)
            {
                foreach (var vsCodeWorkspace in allWorkspaces)
                {
                    if (dbWorkspace.Path == vsCodeWorkspace.Path)
                    {
                        vsCodeWorkspace.Frequency = dbWorkspace.Frequency;
                        break;
                    }
                }
            }

            return allWorkspaces;
        }
    }
}