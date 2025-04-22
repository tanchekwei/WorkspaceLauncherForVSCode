// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CmdPalVsCode.Properties;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;
using System.IO;

namespace CmdPalVsCode;

public class SettingsManager : JsonSettingsManager
{
    private static readonly string _namespace = "vscode";

    private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";

    private static readonly List<ChoiceSetSetting.Choice> _choices =
    [
        new ChoiceSetSetting.Choice(Resource.setting_preferredEdition_option_default_label, "Default"),
        new ChoiceSetSetting.Choice(Resource.setting_preferredEdition_option_insider_label, "Insider"),
    ];


    private readonly ToggleSetting _useStrictSearch = new(
        Namespaced(nameof(UseStrichtSearch)),
        Resource.settings_useStrictSearch_label,
        Resource.settings_useStrictSearch_desc,
        false); // TODO -- double check default value

    private readonly ToggleSetting _showDetails = new(
       Namespaced(nameof(ShowDetails)),
       Resource.settings_showDetails_label,
       Resource.settings_showDetails_desc,
       false); // TODO -- double check default value

    private readonly ChoiceSetSetting _preferredEdition = new(
        Namespaced(nameof(PreferredEdition)),
        Resource.setting_preferredEdition_label,
        Resource.setting_preferredEdition_desc,
        _choices);

    public bool UseStrichtSearch => _useStrictSearch.Value;
    public bool ShowDetails => _showDetails.Value;

    public string PreferredEdition => _preferredEdition.Value ?? "Default";


    internal static string SettingsJsonPath()
    {
        var directory = Utilities.BaseSettingsPath("Microsoft.CmdPal");
        Directory.CreateDirectory(directory);

        // now, the state is just next to the exe
        return Path.Combine(directory, "settings.json");
    }

    public SettingsManager()
    {
        FilePath = SettingsJsonPath();

        Settings.Add(_showDetails);
        Settings.Add(_useStrictSearch);
        Settings.Add(_preferredEdition);

        // Load settings from file upon initialization
        LoadSettings();

        Settings.SettingsChanged += (s, a) =>
        {
            VSCodeHandler.LoadInstances(PreferredEdition);
            this.SaveSettings();
        };
    }
}