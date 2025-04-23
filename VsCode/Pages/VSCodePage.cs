using CmdPalVsCode.Properties;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CmdPalVsCode;

internal sealed partial class VSCodePage : DynamicListPage
{
    private readonly SettingsManager _settingsManager;

    public VSCodePage(SettingsManager settingsManager)
    {
        Icon = IconHelpers.FromRelativePath("Assets\\VsCodeIcon.png");
        Title = Resource.page_title;
        Name = Resource.page_command_name;
        ShowDetails = settingsManager.ShowDetails;

        _settingsManager = settingsManager;
        _settingsManager.Settings.SettingsChanged += (s, a) =>
        {
            ShowDetails = settingsManager.ShowDetails;
        };
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        var items = new List<ListItem>();

        IsLoading = true;
        foreach (var workspace in VSCodeHandler.GetWorkspaces())
        {
            // add instance to the list
            var command = new OpenVSCodeCommand(workspace.Instance.ExecutablePath, workspace.Path);

            Details details = new Details()
            {
                Title = workspace.GetName(),
                HeroImage = workspace.Instance.GetIcon(),
                Metadata = workspace.GetMetadata(),
            };

            var tags = new List<Tag>();

            switch (_settingsManager.TagType)
            {
                case "None":
                    break;
                case "Type":
                    tags.Add(new Tag(workspace.GetWorkspaceType()));
                    if (workspace.GetVSType() != "")
                    {
                        tags.Add(new Tag(workspace.GetVSType()));
                    }
                    break;
                case "Target":
                    tags.Add(new Tag(workspace.Instance.Name));
                    break;
                case "TypeAndTarget":
                    tags.Add(new Tag(workspace.GetWorkspaceType()));
                    if (workspace.GetVSType() != "")
                    {
                        tags.Add(new Tag(workspace.GetVSType()));
                    }
                    tags.Add(new Tag(workspace.Instance.Name));
                    break;
            }

            items.Add(new ListItem(command)
            {
                Title = details.Title,
                Subtitle = Uri.UnescapeDataString(workspace.Path),
                Details = details,
                Icon = workspace.Instance.GetIcon(),
                Tags = tags.ToArray()
            });
        }

        if (items.Count == 0)
        {
            return [
                new ListItem(new NoOpCommand()) { Title = Resource.no_items_found, Subtitle = Resource.no_items_found_subtitle, Icon = IconHelpers.FromRelativePath("Assets\\VsCodeIcon.png") }
            ];
        }

        // filter items based on search text
        if (_settingsManager.UseStrichtSearch)
        {
            // strict search contains all characters in order of search value, with no random characters between
            items = items.FindAll(x => x.Subtitle.ToLower(CultureInfo.CurrentUICulture).Contains(SearchText.ToLower(CultureInfo.CurrentUICulture), StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            // string search contains characters in order of search value, with optional random characters between    
            // e.g. "abc" matches "a1b2c3", "ab", "a b c", etc.
            items = items.FindAll(item =>
            {
                int charIndex = 0;
                foreach (var character in SearchText.ToLower(CultureInfo.CurrentUICulture))
                {
                    charIndex = item.Title.ToLower(CultureInfo.CurrentUICulture).IndexOf(character, charIndex);
                    if (charIndex == -1)
                    {
                        return false;
                    }
                    else
                    {
                        charIndex++;
                    }
                }

                return true;
            });
        }

        // add debug item
        // items.Insert(0, new ListItem(new NoOpCommand()) { Title = ShowDetails.ToString() });

        IsLoading = false;

        return items.ToArray();
    }
}
