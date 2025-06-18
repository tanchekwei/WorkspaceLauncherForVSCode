using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Commands
{
    public sealed partial class RemoveWorkspaceCommandConfirmation : InvokableCommand
    {
        private readonly VisualStudioCodeWorkspace workspace;
        private readonly VisualStudioCodePage page;

        public override string Name => "Remove from List";
        public RemoveWorkspaceCommandConfirmation(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page)
        {
            this.workspace = workspace;
            this.page = page;
            Icon = new IconInfo("\xE74D"); // Delete symbol
        }

        public override CommandResult Invoke()
        {
            var confirmArgs = new ConfirmationArgs()
            {
                Title = $"Remove \"{workspace.WorkspaceName}\" from list",
                Description = $"⚠️ Note: This is an experimental action.\nIt does not delete the actual folder—only its reference in Visual Studio Code’s configuration.\nAre you sure you want to remove the record for \"{workspace.WorkspaceName}\" {workspace.WorkspaceTypeString.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture)} from below file{(workspace.SourcePath.Count > 1 ? "s" : string.Empty)}:\n{string.Join("\n", workspace.SourcePath)}",
                PrimaryCommand = new RemoveWorkspaceCommand(workspace, page),
                IsPrimaryCommandCritical = true,
            };

            return CommandResult.Confirm(confirmArgs);
        }
    }
}
