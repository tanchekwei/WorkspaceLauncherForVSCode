// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
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
    private readonly VisualStudioCodePage _page;

    public WorkspaceLauncherForVSCodeCommandsProvider()
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        _settingsManager = new SettingsManager();
        _vscodeService = new VisualStudioCodeService();
        DisplayName = "Visual Studio / Code";
#if DEBUG
        DisplayName += " (Dev)";
#endif
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        Settings = _settingsManager.Settings;

        _vscodeService.LoadInstances(_settingsManager.EnabledEditions, _settingsManager.PreferredEdition);

        _settingsListener = new SettingsListener(_settingsManager);
        _settingsListener.InstanceSettingsChanged += OnInstanceSettingsChanged;

        _page = new VisualStudioCodePage(_settingsManager, _vscodeService, _settingsListener);
    }

    private void OnInstanceSettingsChanged(object? sender, System.EventArgs e)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        _page.StartRefresh();
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return [
            new CommandItem(_page) {
                Title = DisplayName,
                MoreCommands = [
                    new CommandContextItem(Settings!.SettingsPage),
                ],
            },
        ];
    }
}
