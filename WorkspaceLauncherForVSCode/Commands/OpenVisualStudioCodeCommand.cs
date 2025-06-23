// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode;

/// <summary>
/// Command to open a Visual Studio Code workspace.
/// </summary>
internal sealed partial class OpenVisualStudioCodeCommand : InvokableCommand, IHasWorkspace
{
    public override string Name => "Open";
    private readonly VisualStudioCodePage page;
    private readonly CommandResultType commandResult;

    public VisualStudioCodeWorkspace Workspace { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenVisualStudioCodeCommand"/> class.
    /// </summary>
    /// <param name="workspace">The Visual Studio Code workspace to open.</param>
    /// <param name="page">The Visual Studio Code page instance.</param>
    /// <param name="commandResult">The command result setting value.</param>
    public OpenVisualStudioCodeCommand(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page, CommandResultType commandResult)
    {
        Workspace = workspace;
        this.page = page;
        this.commandResult = commandResult;
        this.Icon = Classes.Icon.VisualStudioCode;
        Name += $" {workspace.WorkspaceType.ToString()}";

        if (workspace.WorkspaceType == WorkspaceType.Solution)
        {
            //this.Subtitle = string.Empty;
        }
    }

    /// <summary>
    /// Invokes the command to open the workspace in Visual Studio Code.
    /// </summary>
    /// <returns>The result of the command execution.</returns>
    public override CommandResult Invoke()
    {
        if (Workspace.WorkspaceType == WorkspaceType.Solution)
        {
            return CommandResult.Confirm(new ConfirmationArgs { Title = "Error", Description = "Cannot open a solution with this command." });
        }

        if (Workspace.WindowsPath is null || Workspace.Path is null || Workspace.VSCodeInstance is null)
        {
            return CommandResult.Confirm(new ConfirmationArgs { Title = "Error", Description = "Workspace path, or instance is null. Cannot open." });
        }

        var pathNotFoundResult = CommandHelpers.IsPathNotFound(Workspace.WindowsPath);
        if (pathNotFoundResult != null)
        {
            return pathNotFoundResult;
        }

        string? arguments;
        // Open the workspace in Visual Studio Code
        if (Workspace.Path.EndsWith(".code-workspace", System.StringComparison.OrdinalIgnoreCase))
        {
            arguments = $"--file-uri \"{Workspace.Path}\"";
        }
        else
        {
            arguments = $"--folder-uri \"{Workspace.Path}\"";
        }

        ShellHelpers.OpenInShell(Workspace.VSCodeInstance.ExecutablePath, arguments, null, ShellHelpers.ShellRunAsType.None, false);

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
