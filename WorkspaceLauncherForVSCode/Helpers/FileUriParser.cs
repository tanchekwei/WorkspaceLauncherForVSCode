using System;
using System.Diagnostics.CodeAnalysis;

namespace WorkspaceLauncherForVSCode.Helpers;

public static class FileUriParser
{
  public static bool TryConvertToWindowsPath(string fileUri, [NotNullWhen(true)] out string? windowsPath)
  {
    windowsPath = null;

    if (string.IsNullOrWhiteSpace(fileUri))
      return false;

    // Fix lowercase %3A issues in drive letters
    fileUri = fileUri.Replace("%3A", ":").Replace("%3a", ":");

    if (!Uri.TryCreate(fileUri, UriKind.Absolute, out var uri))
      return false;

    if (!uri.IsFile)
      return false;

    string localPath = uri.LocalPath.TrimStart('/');
    windowsPath = CapitalizeDriveLetter(localPath);

    return true;
  }

  private static string CapitalizeDriveLetter(string path)
  {
    if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
    {
      return char.ToUpperInvariant(path[0]) + path.Substring(1);
    }
    return path;
  }
}
