// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

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
        public static async Task<IEnumerable<VisualStudioCodeWorkspace>> GetWorkspacesAsync(VisualStudioCodeInstance instance, List<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            if (cancellationToken.IsCancellationRequested || !File.Exists(instance.ExecutablePath))
            {
                return Enumerable.Empty<VisualStudioCodeWorkspace>();
            }

            var vscdbTask = VscdbWorkspaceReader.GetWorkspacesAsync(instance, cancellationToken);
            var storageJsonTask = StorageJsonWorkspaceReader.GetWorkspacesAsync(instance, cancellationToken);

            await Task.WhenAll(vscdbTask, storageJsonTask);

            var allWorkspaces = new ConcurrentBag<VisualStudioCodeWorkspace>(vscdbTask.Result.Concat(storageJsonTask.Result));
            foreach (var workspace in dbWorkspaces)
            {
                foreach (var vsCodeWorkspace in allWorkspaces)
                {
                    if (workspace.Path == vsCodeWorkspace.Path)
                    {
                        vsCodeWorkspace.Frequency = workspace.Frequency;
                        break;
                    }
                }
            }

            return allWorkspaces;
        }
    }
}