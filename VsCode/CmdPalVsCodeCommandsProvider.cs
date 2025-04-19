using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalVsCode;

public partial class CmdPalVsCodeCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private readonly SettingsManager _settingsManager = new();
    public CmdPalVsCodeCommandsProvider()
    {
        DisplayName = "VS Code";
        Icon = IconHelpers.FromRelativePath("Assets\\VsCodeIcon.png");

        Settings = _settingsManager.Settings;
        _commands = [
            new CommandItem(new VSCodePage(_settingsManager)) {
                Title = DisplayName,
                MoreCommands = [
                    new CommandContextItem(Settings.SettingsPage),
                ],
            },
        ];

        VSCodeHandler.LoadInstances(_settingsManager.PreferredEdition);

    }


    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
