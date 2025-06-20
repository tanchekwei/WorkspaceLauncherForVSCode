// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Diagnostics;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Components
{
    internal sealed class WindowProcess
    {
        private uint processId;
        private string? processName;

        internal uint ProcessId => processId;
        internal string? Name => processName;

        internal WindowProcess(IntPtr hwnd)
        {
            processId = GetProcessIDFromWindowHandle(hwnd);
            processName = GetProcessNameFromProcessID(processId);
        }

        private static uint GetProcessIDFromWindowHandle(IntPtr hwnd)
        {
            _ = NativeMethods.GetWindowThreadProcessId(hwnd, out var processId);
            return processId;
        }

        private static string? GetProcessNameFromProcessID(uint processId)
        {
            var process = Process.GetProcessById((int)processId);
            return process?.ProcessName;
        }
    }
}