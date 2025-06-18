using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode
{
    public interface IVisualStudioCodeService
    {
        List<VisualStudioCodeInstance> Instances { get; }
        void LoadInstances(VisualStudioCodeEdition enabledEditions, string preferredEdition);
        Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(WorkspaceStorage workspaceStorage, CancellationToken cancellationToken);
    }
}