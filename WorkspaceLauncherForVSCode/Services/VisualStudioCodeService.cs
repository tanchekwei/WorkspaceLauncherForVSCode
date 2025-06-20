// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Services
{
    public class VisualStudioCodeService : IVisualStudioCodeService
    {
        public List<VisualStudioCodeInstance> Instances { get; private set; } = new List<VisualStudioCodeInstance>();

        public void LoadInstances(VisualStudioCodeEdition enabledEditions, string preferredEdition)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            Instances = VisualStudioCodeInstanceProvider.GetInstances(enabledEditions, preferredEdition);
        }

        public async Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(List<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            var workspaceMap = new ConcurrentDictionary<string, VisualStudioCodeWorkspace>();
            await Parallel.ForEachAsync(Instances, cancellationToken, async (instance, ct) =>
            {
                var workspaces = await VisualStudioCodeWorkspaceProvider.GetWorkspacesAsync(instance, dbWorkspaces, ct);
                foreach (var workspace in workspaces)
                {
                    if (workspace.Path == null) continue;
                    if (!workspaceMap.TryAdd(workspace.Path, workspace))
                    {
                        var existing = workspaceMap[workspace.Path];
                        if (existing.Source != workspace.Source)
                        {
                            existing.Source = VisualStudioCodeWorkspaceSource.StorageJsonVscdb;
                            if (workspace.SourcePath.Count > 0)
                            {
                                existing.SourcePath.Add(workspace.SourcePath[0]);
                            }
                            if (workspace.Frequency > 0)
                            {
                                existing.Frequency = workspace.Frequency;
                            }
                        }
                    }
                }
            });

            var result = workspaceMap.Values.ToList();
            return result;
        }

        public Task<List<VisualStudioCodeWorkspace>> GetVisualStudioSolutions(List<VisualStudioCodeWorkspace> dbWorkspaces, bool showPrerelease)
        {
            return VisualStudioProvider.GetSolutions(dbWorkspaces, showPrerelease);
        }
    }
}