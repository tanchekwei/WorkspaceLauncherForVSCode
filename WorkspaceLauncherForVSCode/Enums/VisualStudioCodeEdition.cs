using System;

namespace WorkspaceLauncherForVSCode.Enums
{
    [Flags]
    public enum VisualStudioCodeEdition
    {
        None = 0,
        Default = 1,
        System = 2,
        Insider = 4,
        Custom = 8
    }
}