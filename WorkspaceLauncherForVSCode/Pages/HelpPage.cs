// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;

using OpenUrlCommand = WorkspaceLauncherForVSCode.Commands.OpenUrlCommand;

namespace WorkspaceLauncherForVSCode.Pages
{
    public sealed partial class HelpPage : ListPage
    {
        private static readonly ListItem _openSettingsItem = new ListItem(new OpenInExplorerCommand(Utilities.BaseSettingsPath(Constant.AppName), null, null, "Open extension settings / logs folder"));
        private static readonly ListItem _viewSourceItem = new ListItem(new OpenUrlCommand("https://github.com/tanchekwei/WorkspaceLauncherForVSCode", "View source code", Classes.Icon.GitHub));
        private static readonly ListItem _reportBugItem = new ListItem(new OpenUrlCommand("https://github.com/tanchekwei/WorkspaceLauncherForVSCode/issues/new", "Report issue", Classes.Icon.GitHub));
        private int _total;
        private int _totalVisualStudio;
        private int _totalVisualStudioCode;
        public HelpPage()
        {
            Name = "Help";
            Icon = Classes.Icon.Help;
            Id = "HelpPage";
        }

        public override IListItem[] GetItems()
        {
            return [
                _reportBugItem,
                _viewSourceItem,
                new ListItem()
                {
                    Title = $"{_totalVisualStudio}",
                    Subtitle = "Visual Studio Count",
                    Icon = Classes.Icon.VisualStudio,
                },
                new ListItem()
                {
                    Title = $"{_totalVisualStudioCode}",
                    Subtitle = "Visual Studio Code Count",
                    Icon = Classes.Icon.VisualStudioCode,
                },
                new ListItem()
                {
                    Title = $"{_total}",
                    Subtitle = "Visual Studio / Code Count",
                    Icon = Classes.Icon.VisualStudioAndVisualStudioCode,
                },
                new ListItem()
                {
                    Title = "1.11.0.0",
                    Subtitle = "Extension Version",
                    Icon = Classes.Icon.Extension,
                },
                _openSettingsItem,
            ];
        }

        public void UpdateTotal(int count)
        {
            _total = count;
            RaiseItemsChanged();
        }

        public void UpdateTotalVisualStudio(int count)
        {
            _totalVisualStudio = count;
            RaiseItemsChanged();
        }

        public void UpdateTotalVisualStudioCode(int count)
        {
            _totalVisualStudioCode = count;
            RaiseItemsChanged();
        }
    }
}
