using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Services
{
    public class VisualStudioCodeService : IVisualStudioCodeService
    {
        public List<VisualStudioCodeInstance> Instances { get; private set; } = new List<VisualStudioCodeInstance>();

        public void LoadInstances(VisualStudioCodeEdition enabledEditions, string preferredEdition)
        {
            // using var logger = new TimeLogger();
            Instances = VisualStudioCodeInstanceProvider.GetInstances(enabledEditions, preferredEdition);
        }

        public async Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(WorkspaceStorage workspaceStorage, CancellationToken cancellationToken)
        {
            // using var logger = new TimeLogger();
            var workspaceMap = new ConcurrentDictionary<string, VisualStudioCodeWorkspace>();
            await Parallel.ForEachAsync(Instances, cancellationToken, async (instance, ct) =>
            {
                var workspaces = await VisualStudioCodeWorkspaceProvider.GetWorkspacesAsync(instance, workspaceStorage, ct);
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
            new ToastStatusMessage($"Loaded {result.Count} workspaces").Show();
            return result;
        }
    }
}