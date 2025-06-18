using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Listeners
{
    public class SettingsListener
    {
        private readonly SettingsManager _settingsManager;
        private VisualStudioCodeEdition _previousEditions;
        private string _previousPreferredEdition;
        private bool _previousShowDetails;
        private bool _previousUseStrictSearch;
        private SearchBy _previousSearchBy;

        public event EventHandler? InstanceSettingsChanged;
        public event EventHandler? PageSettingsChanged;

        public SettingsListener(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _previousEditions = _settingsManager.EnabledEditions;
            _previousPreferredEdition = _settingsManager.PreferredEdition;
            _previousShowDetails = _settingsManager.ShowDetails;
            _previousUseStrictSearch = _settingsManager.UseStrichtSearch;
            _previousSearchBy = _settingsManager.SearchBy;

            _settingsManager.Settings.SettingsChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object? sender, Settings e)
        {
            var currentEditions = _settingsManager.EnabledEditions;
            var currentPreferredEdition = _settingsManager.PreferredEdition;
            var currentShowDetails = _settingsManager.ShowDetails;
            var currentUseStrictSearch = _settingsManager.UseStrichtSearch;
            var currentSearchBy = _settingsManager.SearchBy;

            if (currentEditions != _previousEditions || currentPreferredEdition != _previousPreferredEdition)
            {
                InstanceSettingsChanged?.Invoke(this, EventArgs.Empty);
                _previousEditions = currentEditions;
                _previousPreferredEdition = currentPreferredEdition;
            }

            if (currentShowDetails != _previousShowDetails || currentUseStrictSearch != _previousUseStrictSearch || currentSearchBy != _previousSearchBy)
            {
                PageSettingsChanged?.Invoke(this, EventArgs.Empty);
                _previousShowDetails = currentShowDetails;
                _previousUseStrictSearch = currentUseStrictSearch;
                _previousSearchBy = currentSearchBy;
            }
        }
    }
}