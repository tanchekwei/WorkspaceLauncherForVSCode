// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Properties;
using WorkspaceLauncherForVSCode.Workspaces;

namespace WorkspaceLauncherForVSCode;

public sealed partial class VisualStudioCodePage : DynamicListPage, IDisposable
{
    private readonly SettingsManager _settingsManager;
    private readonly IVisualStudioCodeService _vscodeService;
    private readonly SettingsListener _settingsListener;
    private readonly WorkspaceStorage _workspaceStorage;

    private readonly List<ListItem> _allItems = new();
    private readonly List<ListItem> _visibleItems = new();
    private List<ListItem> _cachedFilteredItems = new();

    private readonly object _itemsLock = new();
    private readonly SemaphoreSlim _getItemsSemaphore = new(1, 1);
    private readonly SemaphoreSlim _loadWorkspacesAsyncSemaphore = new(1, 1);

    private CancellationTokenSource _cancellationTokenSource = new();

    private RefreshWorkspacesCommand _refreshWorkspacesCommand;
    private CommandContextItem _refreshWorkspacesCommandContextItem;
    private CommandContextItem _openExtensionSettingsLogsCommandContextItem;
    private readonly IListItem[] _noResultsRefreshItem;
    private readonly IListItem[] _refreshSuggestionItem;

    public VisualStudioCodePage(SettingsManager settingsManager, IVisualStudioCodeService vscodeService, SettingsListener settingsListener)
    {
        Title = Resource.page_title;
#if DEBUG
        Title += " (Dev)";
#endif
        this.Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        Name = Resource.page_command_name;

        _settingsManager = settingsManager;
        _vscodeService = vscodeService;
        _workspaceStorage = new WorkspaceStorage();
        ShowDetails = _settingsManager.ShowDetails;
        _refreshWorkspacesCommand = new(_vscodeService, settingsManager, this);
        _refreshWorkspacesCommandContextItem = new CommandContextItem(_refreshWorkspacesCommand);
        _openExtensionSettingsLogsCommandContextItem = new CommandContextItem(new OpenInExplorerCommand(Utilities.BaseSettingsPath(Constant.AppName), null, this, "Open extension settings / logs folder"));
        _settingsListener = settingsListener;
        _settingsListener.PageSettingsChanged += OnPageSettingsChanged;
        _noResultsRefreshItem = [
            new ListItem(_refreshWorkspacesCommandContextItem)
            {
                Title = "No matching workspaces found",
                Subtitle = "Double-click or press Enter to refresh the list.",
            },
        ];
        _refreshSuggestionItem = [new ListItem(_refreshWorkspacesCommandContextItem)
        {
            Title = "Still not seeing what you´re looking for?",
            Subtitle = "Double-click or press Enter to refresh the list.",
        }];
        Task.Run(() => LoadInitialWorkspacesAsync());
    }

    // private void SetIcon()
    // {
    //     if (_settingsManager.EnableVisualStudio)
    //     {
    //         const VisualStudioCodeEdition AnyEditionFlags =
    //             VisualStudioCodeEdition.Default |
    //             VisualStudioCodeEdition.System |
    //             VisualStudioCodeEdition.Insider |
    //             VisualStudioCodeEdition.Custom;
    //
    //         if ((_settingsManager.EnabledEditions & AnyEditionFlags) == 0)
    //         {
    //             this.Icon = Classes.Icon.VisualStudio;
    //         }
    //         else
    //         {
    //             this.Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
    //         }
    //     }
    //     else
    //     {
    //         this.Icon = Classes.Icon.VisualStudioCode;
    //     }
    // }

    private async Task LoadInitialWorkspacesAsync()
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        var workspaces = await _workspaceStorage.GetWorkspacesAsync();
        if (workspaces.Count == 0)
        {
            await LoadWorkspacesAsync(workspaces, _cancellationTokenSource.Token);
        }
        else
        {
            UpdateWorkspaceList(workspaces, CancellationToken.None);
            RaiseItemsChanged();
            // Trigger a background update
            _ = Task.Run(() => LoadWorkspacesAsync(workspaces, _cancellationTokenSource.Token));
        }
    }

    public override IListItem[] GetItems()
    {
        _getItemsSemaphore.Wait();
        try
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            lock (_itemsLock)
            {
                if (_allItems.Count == 0 && !IsLoading)
                {
                    // Load initial items in the background
                    _ = Task.Run(async () =>
                    {
                        var workspaces = await _workspaceStorage.GetWorkspacesAsync();
                        await LoadWorkspacesAsync(workspaces, _cancellationTokenSource.Token);
                    });
                }

                if (_visibleItems.Count == 0 && !IsLoading && !string.IsNullOrWhiteSpace(SearchText))
                {
                    return _noResultsRefreshItem;
                }
                return _visibleItems.Concat(_refreshSuggestionItem).ToArray();
            }
        }
        finally
        {
            _getItemsSemaphore.Release();
        }
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        if (cancellationToken.IsCancellationRequested)
            return;

        lock (_itemsLock)
        {
            _cachedFilteredItems = WorkspaceFilter.Filter(newSearch, _allItems, _settingsManager.SearchBy, _settingsManager.SortByFrequency);
            _visibleItems.Clear();
            _visibleItems.AddRange(_cachedFilteredItems.Take(_settingsManager.PageSize));
            HasMoreItems = _cachedFilteredItems.Count > _settingsManager.PageSize;
        }

        RaiseItemsChanged();
    }

    public override void LoadMore()
    {
        IsLoading = true;
        RaiseItemsChanged();

        lock (_itemsLock)
        {
            var currentCount = _visibleItems.Count;
            var moreItems = _cachedFilteredItems.Skip(currentCount).Take(_settingsManager.PageSize).ToList();

            if (moreItems.Count > 0)
            {
                _visibleItems.AddRange(moreItems);
            }

            HasMoreItems = _visibleItems.Count < _cachedFilteredItems.Count;
        }

        IsLoading = false;
        RaiseItemsChanged();
    }

    private async Task LoadWorkspacesAsync(List<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken)
    {
        _loadWorkspacesAsyncSemaphore.Wait(cancellationToken);
#if DEBUG
        using var logger = new TimeLogger();
#endif
        IsLoading = true;
        RaiseItemsChanged();

        try
        {

            var workspacesTask = _vscodeService.GetWorkspacesAsync(dbWorkspaces, cancellationToken);
            Task<List<VisualStudioCodeWorkspace>>? solutionsTask = null;
            if (_settingsManager.EnableVisualStudio)
            {
                solutionsTask = _vscodeService.GetVisualStudioSolutions(dbWorkspaces, true);
            }
            await Task.WhenAll(workspacesTask, solutionsTask ?? Task.CompletedTask);
            var workspaces = await workspacesTask;
            var solutions = solutionsTask != null ? await solutionsTask : new List<VisualStudioCodeWorkspace>();

            workspaces.AddRange(solutions);

            if (cancellationToken.IsCancellationRequested) return;
            await _workspaceStorage.SaveWorkspacesAsync(workspaces);
            UpdateWorkspaceList(workspaces, cancellationToken);
            new ToastStatusMessage($"Loaded {workspaces.Count} workspaces").Show();
        }
        catch (OperationCanceledException)
        {
            // Task was canceled, which is expected.
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IsLoading = false;
                RaiseItemsChanged();
            }
            _loadWorkspacesAsyncSemaphore.Release();
        }
    }

    private void UpdateWorkspaceList(List<VisualStudioCodeWorkspace> workspaces, CancellationToken cancellationToken)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        var newItems = workspaces
            .Select(w => WorkspaceItemFactory.Create(w, this, _settingsManager, _refreshWorkspacesCommandContextItem, _openExtensionSettingsLogsCommandContextItem))
            .ToList();

        lock (_itemsLock)
        {
            if (cancellationToken.IsCancellationRequested) return;

            _allItems.Clear();
            _allItems.AddRange(newItems);

            // Apply the current filter to the newly loaded items
            _cachedFilteredItems = WorkspaceFilter.Filter(SearchText, _allItems, _settingsManager.SearchBy, _settingsManager.SortByFrequency);
            _visibleItems.Clear();
            _visibleItems.AddRange(_cachedFilteredItems.Take(_settingsManager.PageSize));
            HasMoreItems = _cachedFilteredItems.Count > _settingsManager.PageSize;
        }
    }

    private void OnPageSettingsChanged(object? sender, EventArgs e)
    {
        ShowDetails = _settingsManager.ShowDetails;

        lock (_itemsLock)
        {
            _cachedFilteredItems = WorkspaceFilter.Filter(SearchText, _allItems, _settingsManager.SearchBy, _settingsManager.SortByFrequency);
            _visibleItems.Clear();
            _visibleItems.AddRange(_cachedFilteredItems.Take(_settingsManager.PageSize));
            HasMoreItems = _cachedFilteredItems.Count > _settingsManager.PageSize;
        }

        RaiseItemsChanged();
    }

    public void RemoveWorkspace(VisualStudioCodeWorkspace workspace)
    {
        lock (_itemsLock)
        {
            var itemToRemove = _allItems.FirstOrDefault(item => (item.Command as OpenVisualStudioCodeCommand)?.Workspace == workspace);
            if (itemToRemove != null)
            {
                _allItems.Remove(itemToRemove);
                _cachedFilteredItems.Remove(itemToRemove);
                _visibleItems.Remove(itemToRemove);
                HasMoreItems = _cachedFilteredItems.Count > _visibleItems.Count;
            }
        }

        RaiseItemsChanged();
    }

    public async Task UpdateFrequencyAsync(string path)
    {
        await _workspaceStorage.UpdateWorkspaceFrequencyAsync(path);

        lock (_itemsLock)
        {
            var itemToUpdate = _allItems.FirstOrDefault(item => (item.Command as IHasWorkspace)?.Workspace.Path == path);
            if (itemToUpdate != null)
            {
                var openCommand = itemToUpdate.Command as IHasWorkspace;
                if (openCommand?.Workspace != null)
                {
                    openCommand.Workspace.Frequency++;
                }

                // Re-apply filter and sort
                _cachedFilteredItems = WorkspaceFilter.Filter(SearchText, _allItems, _settingsManager.SearchBy, _settingsManager.SortByFrequency);
                _visibleItems.Clear();
                _visibleItems.AddRange(_cachedFilteredItems.Take(_settingsManager.PageSize));
                HasMoreItems = _cachedFilteredItems.Count > _settingsManager.PageSize;
            }
        }
        RaiseItemsChanged();
    }

    public void ClearAllItems()
    {
        _allItems.Clear();
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _settingsListener.PageSettingsChanged -= OnPageSettingsChanged;
        _workspaceStorage.Dispose();
        _getItemsSemaphore.Dispose();
    }
}
