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
    private VSCodePage page;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenVSCodeCommand"/> class.
    /// </summary>
    /// <param name="executablePath">The path to the VS Code executable.</param>
    /// <param name="workspacePath">The path to the workspace to open.</param>
    public OpenVSCodeCommand(string executablePath, string workspacePath, VSCodePage page)
    {
        this.workspacePath = workspacePath;
        this.executablePath = executablePath;
        this.page = page;
    }

    /// <summary>
    /// Invokes the command to open the workspace in VS Code.
    /// </summary>
    /// <returns>The result of the command execution.</returns>
    public override CommandResult Invoke()
    {
        string? arguments;
        // Open the workspace in VS Code
        if (workspacePath.EndsWith(".code-workspace", System.StringComparison.OrdinalIgnoreCase))
        {
            arguments = $"--file-uri \"{workspacePath}\"";
        }
        else
        {
            arguments = $"--folder-uri \"{workspacePath}\"";
        }

        ShellHelpers.OpenInShell(executablePath, arguments, null, ShellHelpers.ShellRunAsType.None, false);

        // reset search text
        page.UpdateSearchText(page.SearchText, string.Empty);

        return CommandResult.Hide();
    }
}
