// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Components;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Commands;

public partial class OpenSolutionCommand : InvokableCommand, IHasWorkspace
{
    public VisualStudioCodeWorkspace Workspace { get; set; }
    private readonly VisualStudioCodePage page;
    private readonly CommandResultType commandResult;
    private readonly bool _elevated;

    public OpenSolutionCommand(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page, CommandResultType commandResult, bool elevated = false)
    {
        Workspace = workspace;
        this.page = page;
        this.commandResult = commandResult;
        _elevated = elevated;

        if (_elevated)
        {
            Name = "Run as Administrator";
            this.Icon = new("\uE7EF");
        }
        else
        {
            Name = "Open";
            this.Icon = Classes.Icon.VisualStudio;
        }
    }

    public override CommandResult Invoke()
    {
        if (Workspace.WindowsPath is not null)
        {
            var pathNotFoundResult = CommandHelpers.IsPathValid(Workspace.WindowsPath);
            if (pathNotFoundResult != null)
            {
                return pathNotFoundResult;
            }
        }

        if (string.IsNullOrEmpty(Workspace.Path))
        {
            return CommandResult.Dismiss();
        }

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

        OpenInShellHelper.OpenInShell(Workspace.VSInstance.InstancePath, Workspace.Path, runAs: _elevated ? OpenInShellHelper.ShellRunAsType.Administrator : OpenInShellHelper.ShellRunAsType.None);

        // Update frequency
        Task.Run(() => page.UpdateFrequencyAsync(Workspace.Path));

        switch (commandResult)
        {
            case CommandResultType.GoBack:
                return CommandResult.GoBack();
            case CommandResultType.KeepOpen:
                // reset search text
                page.UpdateSearchText(page.SearchText, "");
                page.SearchText = "";
                return CommandResult.KeepOpen();
            case CommandResultType.Dismiss:
                page.UpdateSearchText(page.SearchText, "");
                page.SearchText = "";
                return CommandResult.Dismiss();
            default:
                return CommandResult.Dismiss();
        }
    }
}
