// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Services.VisualStudio;

namespace WorkspaceLauncherForVSCode.Services
{
    public static class VisualStudioProvider
    {
        public static Task<List<VisualStudioCodeWorkspace>> GetSolutions(
            List<VisualStudioCodeWorkspace> dbWorkspaces, bool showPrerelease)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            var visualStudioService = new VisualStudioService();
            visualStudioService.InitInstances(Array.Empty<string>());
            var results = visualStudioService.GetResults(showPrerelease);

            var list = results.Select(r =>
            {
                var vs = new VisualStudioCodeWorkspace
                {
                    Name = r.Name,
                    Path = r.FullPath,
                    WindowsPath = r.FullPath,
                    WorkspaceType = WorkspaceType.Solution,
                    VSInstance = r.Instance,
                    LastAccessed = r.LastAccessed,
                };
                vs.SetWorkspaceType();
                vs.SetVSMetadata();
                foreach (var workspace in dbWorkspaces)
                {
                    if (workspace.Path == vs.Path)
                    {
                        vs.Frequency = workspace.Frequency;
                        vs.PinDateTime = workspace.PinDateTime;
                        break;
                    }
                }
                return vs;
            }).ToList();
            return Task.FromResult(list);
        }
    }
}