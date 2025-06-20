// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Classes
{
    internal static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(Utilities.BaseSettingsPath(Constant.AppName), "main.log");
        private static readonly object _lock = new object();
        private static readonly SettingsManager _settingsManager = new SettingsManager();

        public static void Log(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            if (!_settingsManager.EnableLogging)
            {
                return;
            }

            try
            {
                lock (_lock)
                {
                    var className = Path.GetFileNameWithoutExtension(sourceFilePath);
                    var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{className}.{memberName}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogFilePath, logMessage);
                }
            }
            catch
            {
                // Suppress exceptions during logging to avoid crashing the app.
            }
        }
    }

    internal sealed partial class TimeLogger : IDisposable
    {
        private readonly string _memberName;
        private readonly string _sourceFilePath;
        private readonly Stopwatch _stopwatch;

        public TimeLogger([CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            _memberName = memberName;
            _sourceFilePath = sourceFilePath;
            _stopwatch = Stopwatch.StartNew();
            Logger.Log("Started", _memberName, _sourceFilePath);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Logger.Log($"Finished in {_stopwatch.ElapsedMilliseconds}ms", _memberName, _sourceFilePath);
        }
    }
}