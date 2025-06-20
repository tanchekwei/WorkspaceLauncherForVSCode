// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Classes;

public static class Icon
{
    public static readonly IconInfo VisualStudio = IconHelpers.FromRelativePath(@"Assets\VisualStudioIcon.svg");
    public static readonly IconInfo VisualStudioCode = IconHelpers.FromRelativePath("Assets\\VisualStudioCodeIcon.svg");
    public static readonly IconInfo VisualStudioAndVisualStudioCode = IconHelpers.FromRelativePath("Assets\\VisualStudioAndVisualStudioCodeIcon.svg");
    public static readonly IconInfo VisualStudioCodeInsider = IconHelpers.FromRelativePath("Assets\\VisualStudioCodeInsiderIcon.svg");
    public static readonly IconInfo FileExplorer = IconHelpers.FromRelativePath("Assets\\FileExplorer.svg");
}
