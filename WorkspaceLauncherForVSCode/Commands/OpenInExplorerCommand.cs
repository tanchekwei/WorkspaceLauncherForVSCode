// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.IO;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Commands
{
    public sealed partial class OpenInExplorerCommand : InvokableCommand
    {
        private readonly VisualStudioCodeWorkspace? workspace;
        private readonly VisualStudioCodePage page;
        public OpenInExplorerCommand(string arguments, VisualStudioCodeWorkspace? workspace, VisualStudioCodePage page, string name = "Open in Explorer", string path = "explorer.exe", string? workingDir = null, OpenInShellHelper.ShellRunAsType runAs = OpenInShellHelper.ShellRunAsType.None, bool runWithHiddenWindow = false)
        {
            Name = name;
            _path = path;
            _arguments = arguments;
            _workingDir = workingDir;
            _runAs = runAs;
            _runWithHiddenWindow = runWithHiddenWindow;
            Icon = Classes.Icon.FileExplorer;
            this.workspace = workspace;
            this.page = page;
        }

        public override CommandResult Invoke()
        {
            if (_arguments == null)
            {
                return CommandResult.Dismiss();
            }
            var pathNotFoundResult = CommandHelpers.GetPathNotFoundResult(_arguments, workspace, page);
            if (pathNotFoundResult != null)
            {
                return pathNotFoundResult;
            }
            if (workspace?.WorkspaceType == Enums.WorkspaceType.Solution)
            {
                _arguments = Path.GetDirectoryName(_arguments);
            }
            OpenInShellHelper.OpenInShell(_path, _arguments);
            return CommandResult.Dismiss();
        }

        private string _path;
        private string? _workingDir;
        private string? _arguments;
        private OpenInShellHelper.ShellRunAsType _runAs;
        private bool _runWithHiddenWindow;
    }
}
