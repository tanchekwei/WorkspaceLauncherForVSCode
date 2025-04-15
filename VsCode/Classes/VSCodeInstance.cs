// Copyright (c) Jonah Fintz. Licensed under the MIT License.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalVsCode;

/// <summary>
/// Enum for system of user installation.
/// </summary>
enum VSCodeInstallationType
{
    System,
    User
}

/// <summary>
/// Enum for type of Visual Studio Code.
/// </summary>
enum VSCodeType
{
    Default,
    Insider
}

/// <summary>
/// Represents an instance of Visual Studio Code.
/// </summary>
internal class VSCodeInstance
{
    public string Name;
    public string ExecutablePath;
    public string StoragePath;
    public VSCodeInstallationType InstallationType;
    public VSCodeType VSCodeType;

    /// <summary>
    /// Initializes a new instance of the <see cref="VSCodeInstance"/> class.
    /// </summary>
    /// <param name="name">The name of the VS Code instance.</param>
    /// <param name="executablePath">The path to the executable file.</param>
    /// <param name="storagePath">The path to the storage file.</param>
    /// <param name="installationType">The type of installation (system or user).</param>
    /// <param name="type">The type of VS Code (default or insider).</param>
    public VSCodeInstance(string name, string executablePath, string storagePath, VSCodeInstallationType installationType, VSCodeType type)
    {
        this.Name = name;
        this.ExecutablePath = executablePath;
        this.StoragePath = storagePath;
        this.InstallationType = installationType;
        this.VSCodeType = type;
    }

    /// <summary>
    /// Gets the icon associated with the VS Code instance.
    /// </summary>
    /// <returns>An icon representing the VS Code instance.</returns>
    public IIconInfo GetIcon()
    {
        switch (VSCodeType)
        {
            case VSCodeType.Insider:
                return IconHelpers.FromRelativePath("Assets\\VsCodeInsiderIcon.svg");
            case VSCodeType.Default:
            default:
                return IconHelpers.FromRelativePath("Assets\\VsCodeIcon.png");
        }
    }
}
