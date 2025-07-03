// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Commands;

public static class PageCommandResultHandler
{
    public static CommandResult HandleCommandResult(CommandResultType resultType, VisualStudioCodePage? page)
    {
        if (page != null)
        {
            // reset search text
            if (!string.IsNullOrEmpty(page.SearchText))
            {
                page.UpdateSearchText(page.SearchText, "");
                page.SearchText = "";
            }
        }

        return resultType switch
        {
            CommandResultType.GoBack => CommandResult.GoBack(),
            CommandResultType.KeepOpen => CommandResult.KeepOpen(),
            _ => CommandResult.Dismiss(),
        };
    }
}
