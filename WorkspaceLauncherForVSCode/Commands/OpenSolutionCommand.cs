// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Components;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Commands
{
    public partial class OpenSolutionCommand : InvokableCommand, IHasWorkspace
    {
        public override string Name => "Open";
        public VisualStudioCodeWorkspace Workspace { get; set; }
        private readonly VisualStudioCodePage _page;

        public OpenSolutionCommand(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page)
        {
            Workspace = workspace;
            _page = page;
            this.Icon = Classes.Icon.VisualStudio;
        }

        public override CommandResult Invoke()
        {
            if (Workspace.WindowsPath is not null)
            {
                var pathNotFoundResult = CommandHelpers.IsPathNotFound(Workspace.WindowsPath);
                if (pathNotFoundResult != null)
                {
                    return pathNotFoundResult;
                }
            }

            if (string.IsNullOrEmpty(Workspace.Path))
            {
                return CommandResult.Dismiss();
            }

            // Update frequency
            Task.Run(() => _page.UpdateFrequencyAsync(Workspace.Path));

            OpenWindows.Instance.UpdateOpenWindowsList();
            var openVSWindows = OpenWindows.Instance.Windows.Where(w => w.Process.Name == "devenv");

            var solutionName = System.IO.Path.GetFileNameWithoutExtension(Workspace.Path);

            foreach (var window in openVSWindows)
            {
                if (window.Title.Contains(solutionName))
                {
                    window.SwitchToWindow();
                    return CommandResult.Dismiss();
                }
            }

            Process.Start(new ProcessStartInfo(Workspace.Path) { UseShellExecute = true });

            return CommandResult.Dismiss();
        }
    }
}