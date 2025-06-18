using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode;

namespace WorkspaceLauncherForVSCode.Commands;

public sealed partial class RefreshWorkspacesCommand : InvokableCommand
{
    public override string Name => "Refresh";
    private readonly IVisualStudioCodeService _visualStudioCodeService;
    private readonly SettingsManager _settingsManager;
    private readonly VisualStudioCodePage _visualStudioCodePage;

    public RefreshWorkspacesCommand(IVisualStudioCodeService visualStudioCodeService, SettingsManager settingsManager, VisualStudioCodePage visualStudioCodePage)
    {
        Icon = new IconInfo("\xE72C"); // Refresh icon
        _visualStudioCodeService = visualStudioCodeService;
        _settingsManager = settingsManager;
        _visualStudioCodePage = visualStudioCodePage;
    }

    public override CommandResult Invoke()
    {
        try
        {
            _visualStudioCodeService.LoadInstances(_settingsManager.EnabledEditions, _settingsManager.PreferredEdition);
            _visualStudioCodePage.ClearAllItems();
            _visualStudioCodePage.UpdateSearchText(_visualStudioCodePage.SearchText, "");
        }
        catch
        {
        }
        return CommandResult.KeepOpen();
    }
}
