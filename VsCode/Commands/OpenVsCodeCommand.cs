using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalVsCode;

/// <summary>
/// Command to open a Visual Studio Code workspace.
/// </summary>
internal sealed partial class OpenVSCodeCommand : InvokableCommand
{
    public override string Name => "Open VS Code";
    private string workspacePath;
    private string executablePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenVSCodeCommand"/> class.
    /// </summary>
    /// <param name="executablePath">The path to the VS Code executable.</param>
    /// <param name="workspacePath">The path to the workspace to open.</param>
    public OpenVSCodeCommand(string executablePath, string workspacePath)
    {
        this.workspacePath = workspacePath;
        this.executablePath = executablePath;
    }

    /// <summary>
    /// Invokes the command to open the workspace in VS Code.
    /// </summary>
    /// <returns>The result of the command execution.</returns>
    public override CommandResult Invoke()
    {
        // Open the workspace in VS Code
        ShellHelpers.OpenInShell(executablePath, $"--file-uri {workspacePath}", null, ShellHelpers.ShellRunAsType.None, false);

        ClipboardHelper.SetText(workspacePath);
        return CommandResult.Hide();
    }
}
