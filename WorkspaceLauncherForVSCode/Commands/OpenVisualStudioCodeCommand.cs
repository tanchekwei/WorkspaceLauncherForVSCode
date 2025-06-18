using System.IO;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode;

/// <summary>
/// Command to open a Visual Studio Code workspace.
/// </summary>
internal sealed partial class OpenVisualStudioCodeCommand : InvokableCommand
{
    public override string Name => "Open";
    private readonly VisualStudioCodePage page;
    private readonly CommandResultType commandResult;

    internal VisualStudioCodeWorkspace Workspace { get; }

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
        this.Icon = VisualStudioCode.IconInfo;
        Name += $" {workspace.WorkspaceType.ToString()}";
    }

    /// <summary>
    /// Invokes the command to open the workspace in Visual Studio Code.
    /// </summary>
    /// <returns>The result of the command execution.</returns>
    public override CommandResult Invoke()
    {
        if (Workspace.WindowsPath is null || Workspace.Path is null || Workspace.Instance is null)
        {
            return CommandResult.Confirm(new ConfirmationArgs { Title = "Error", Description = "Workspace path, or instance is null. Cannot open." });
        }

        var pathNotFoundResult = CommandHelpers.GetPathNotFoundResult(Workspace.WindowsPath, Workspace, page);
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

        ShellHelpers.OpenInShell(Workspace.Instance.ExecutablePath, arguments, null, ShellHelpers.ShellRunAsType.None, false);

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
            default:
                return CommandResult.Dismiss();
        }
    }
}
