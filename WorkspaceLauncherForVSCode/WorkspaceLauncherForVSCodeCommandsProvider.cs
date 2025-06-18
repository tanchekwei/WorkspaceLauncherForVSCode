using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Services;

namespace WorkspaceLauncherForVSCode;

public partial class WorkspaceLauncherForVSCodeCommandsProvider : CommandProvider
{
    private readonly SettingsManager _settingsManager;
    private readonly VisualStudioCodeService _vscodeService;
    private readonly SettingsListener _settingsListener;

    public WorkspaceLauncherForVSCodeCommandsProvider()
    {
        // using var logger = new TimeLogger();
        _settingsManager = new SettingsManager();
        _vscodeService = new VisualStudioCodeService();
        DisplayName = "Workspace Launcher for VS Code";
        Icon = VisualStudioCode.IconInfo;
        Settings = _settingsManager.Settings;

        _vscodeService.LoadInstances(_settingsManager.EnabledEditions, _settingsManager.PreferredEdition);

        _settingsListener = new SettingsListener(_settingsManager);
        _settingsListener.InstanceSettingsChanged += OnInstanceSettingsChanged;
    }

    private void OnInstanceSettingsChanged(object? sender, System.EventArgs e)
    {
        _vscodeService.LoadInstances(_settingsManager.EnabledEditions, _settingsManager.PreferredEdition);
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return [
            new CommandItem(new VisualStudioCodePage(_settingsManager, _vscodeService, _settingsListener)) {
                Title = DisplayName,
                MoreCommands = [
                    new CommandContextItem(Settings!.SettingsPage),
                ],
            },
        ];
    }
}
