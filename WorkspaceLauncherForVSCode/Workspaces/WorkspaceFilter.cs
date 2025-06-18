using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Workspaces
{
    public static class WorkspaceFilter
    {
        public static List<ListItem> Filter(string searchText, List<ListItem> allItems, bool useStrictSearch, SearchBy searchBy, bool sortByFrequency)
        {
            List<ListItem> filteredItems;
            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredItems = new List<ListItem>(allItems);
            }
            else
            {
                if (useStrictSearch)
                {
                    filteredItems = allItems
                        .Where(x =>
                        {
                            switch (searchBy)
                            {
                                case SearchBy.Title:
                                    return x.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false;
                                case SearchBy.Path:
                                    return x.Subtitle?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false;
                                case SearchBy.Both:
                                default:
                                    return (x.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                           (x.Subtitle?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false);
                            }
                        })
                        .ToList();
                }
                else
                {
                    var matcher = StringMatcher.Instance;
                    matcher.UserSettingSearchPrecision = SearchPrecisionScore.Regular;

                    filteredItems = allItems
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
                        .Where(x => x.match.Success)
                        .OrderByDescending(x => x.match.Score)
                        .Select(x => x.item)
                        .ToList();
                }
            }

            if (sortByFrequency)
            {
                var workspaceItems = new List<Tuple<OpenVisualStudioCodeCommand, ListItem>>();
                var otherItems = new List<ListItem>();

                foreach (var item in filteredItems)
                {
                    if (item.Command is OpenVisualStudioCodeCommand openCommand)
                    {
                        workspaceItems.Add(new Tuple<OpenVisualStudioCodeCommand, ListItem>(openCommand, item));
                    }
                    else
                    {
                        otherItems.Add(item);
                    }
                }


                var sortedWorkspaceItems = workspaceItems
                    .OrderByDescending(x => x.Item1.Workspace.Frequency)
                    .Select(x => x.Item2)
                    .ToList();

                sortedWorkspaceItems.AddRange(otherItems);
                return sortedWorkspaceItems;
            }

            return filteredItems;
        }
    }
}