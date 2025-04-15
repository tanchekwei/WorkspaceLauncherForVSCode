// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalVsCode;

public partial class CmdPalVsCodeCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public CmdPalVsCodeCommandsProvider()
    {
        DisplayName = "VS Code";
        Icon = IconHelpers.FromRelativePath("Assets\\VsCodeIcon.png");
        _commands = [
            new CommandItem(new VSCodePage()) { Title = DisplayName },
        ];

        VSCodeHandler.LoadInstances();
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
