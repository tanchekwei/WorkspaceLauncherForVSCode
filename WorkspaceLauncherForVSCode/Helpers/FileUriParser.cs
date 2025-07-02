// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Diagnostics.CodeAnalysis;

namespace WorkspaceLauncherForVSCode.Helpers
{
    public static class FileUriParser
    {
        public static bool TryConvertToWindowsPath(string fileUri, [NotNullWhen(true)] out string? windowsPath)
        {
            windowsPath = null;

            if (string.IsNullOrWhiteSpace(fileUri))
            {
                return false;
            }

            // Case 1: Handle vscode-remote WSL URIs (e.g., vscode-remote://wsl+Ubuntu/home/user/project)
            if (fileUri.StartsWith("vscode-remote://wsl%2", StringComparison.OrdinalIgnoreCase))
            {
                return WslPathHelper.TryGetWindowsPathFromWslUri(fileUri, out windowsPath);
            }

            // Case 2: Handle file URIs for WSL (e.g., file://wsl.localhost/Ubuntu/home/user/project)
            const string fileUriScheme = "file://";
            if (fileUri.StartsWith(fileUriScheme, StringComparison.OrdinalIgnoreCase))
            {
                var potentialWslPath = fileUri.Substring(fileUriScheme.Length);
                if (potentialWslPath.StartsWith("wsl.localhost/", StringComparison.OrdinalIgnoreCase) || potentialWslPath.StartsWith("wsl$/", StringComparison.OrdinalIgnoreCase))
                {
                    windowsPath = "\\\\" + potentialWslPath.Replace('/', '\\');
                    return true;
                }
            }

            // Case 3: Handle direct UNC paths for WSL (e.g., \\wsl$\Ubuntu\home\user\project)
            if (fileUri.StartsWith(@"\\wsl$\", StringComparison.OrdinalIgnoreCase) || fileUri.StartsWith(@"\\wsl.localhost\", StringComparison.OrdinalIgnoreCase))
            {
                windowsPath = fileUri;
                return true;
            }

            // Case 4: Handle standard file URIs (e.g., file:///c:/Users/user/project)
            // Fix for lowercase drive letters in URIs
            fileUri = fileUri.Replace("%3A", ":").Replace("%3a", ":");

            if (Uri.TryCreate(fileUri, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                string localPath = uri.LocalPath.TrimStart('/');
                windowsPath = CapitalizeDriveLetter(localPath);
                return true;
            }

            // If none of the above, assume it might be a direct file path already
            // For example, vscode-remote://codespaces+...
            windowsPath = fileUri;
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
}
