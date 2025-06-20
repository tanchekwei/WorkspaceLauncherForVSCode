// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Workspaces
{
    public static class WorkspaceFilter
    {
        public static List<ListItem> Filter(string searchText, List<ListItem> allItems, SearchBy searchBy, bool sortByFrequency)
        {
            List<ListItem> filteredItems;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredItems = [.. allItems];
            }
            else
            {
                var matcher = StringMatcher.Instance;
                matcher.UserSettingSearchPrecision = SearchPrecisionScore.Regular;

                var matches = allItems
                    .Select(item =>
                    {
                        MatchResult titleMatch = new MatchResult(false, 0);
                        MatchResult subtitleMatch = new MatchResult(false, 0);

                        switch (searchBy)
                        {
                            case SearchBy.Title:
                                titleMatch = matcher.FuzzyMatch(searchText, item.Title);
                                break;
                            case SearchBy.Path:
                                subtitleMatch = matcher.FuzzyMatch(searchText, item.Subtitle ?? "");
                                break;
                            case SearchBy.Both:
                            default:
                                titleMatch = matcher.FuzzyMatch(searchText, item.Title);
                                subtitleMatch = matcher.FuzzyMatch(searchText, item.Subtitle ?? "");
                                break;
                        }

                        var bestMatch = titleMatch.Score >= subtitleMatch.Score ? titleMatch : subtitleMatch;
                        return new { item, match = bestMatch };
                    })
                    .Where(x => x.match.Success);
                if (sortByFrequency)
                {
                    filteredItems = matches
                        .OrderByDescending(x => (x.item.Command as IHasWorkspace)?.Workspace.Frequency)
                        .ThenByDescending(x => x.match.Score)
                        .Select(x => x.item)
                        .ToList();
                }
                else
                {
                    filteredItems = matches
                        .OrderByDescending(x => x.match.Score)
                        .Select(x => x.item)
                        .ToList();
                }
            }

            return filteredItems;
        }
    }
}