// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Components
{
    internal sealed class OpenWindows
    {
        private static readonly object _enumWindowsLock = new();
        private readonly List<Window> windows = new();
        private static OpenWindows? instance;

        internal List<Window> Windows => new(windows);

        internal static OpenWindows Instance
        {
            get
            {
                instance ??= new OpenWindows();
                return instance;
            }
        }

        private OpenWindows() { }

        internal void UpdateOpenWindowsList()
        {
            lock (_enumWindowsLock)
            {
                windows.Clear();
                EnumWindowsProc callbackptr = new EnumWindowsProc(WindowEnumerationCallBack);
                _ = NativeMethods.EnumWindows(callbackptr, IntPtr.Zero);
            }
        }

        private bool WindowEnumerationCallBack(IntPtr hwnd, IntPtr lParam)
        {
            Window newWindow = new Window(hwnd);

            if (newWindow.IsWindow && newWindow.Visible && newWindow.IsOwner &&
                (!newWindow.IsToolWindow || newWindow.IsAppWindow) && !newWindow.TaskListDeleted &&
                newWindow.ClassName != "Windows.UI.Core.CoreWindow" &&
                !newWindow.IsCloaked)
            {
                windows.Add(newWindow);
            }

            return true;
        }
    }
}