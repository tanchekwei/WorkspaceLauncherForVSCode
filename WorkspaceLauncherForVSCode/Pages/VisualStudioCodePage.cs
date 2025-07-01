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
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Pages;
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
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    private CancellationTokenSource _cancellationTokenSource = new();

    private RefreshWorkspacesCommand _refreshWorkspacesCommand;
    private CommandContextItem _refreshWorkspacesCommandContextItem;
    private CommandContextItem _helpCommandContextItem;
    private readonly IListItem[] _noResultsRefreshItem;
    private readonly IListItem[] _refreshSuggestionItem;
    private readonly HelpPage _helpPage;

    public event Action<int> TotalChanged;
    public event Action<int> TotalVisualStudioChanged;
    public event Action<int> TotalVisualStudioCodeChanged;

    public VisualStudioCodePage(SettingsManager settingsManager, IVisualStudioCodeService vscodeService, SettingsListener settingsListener)
    {
        Title = Resource.page_title;
#if DEBUG
        Title += " (Dev)";
#endif
        this.Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        Name = Resource.page_command_name;
        Id = "VisualStudioCodePage";

        _settingsManager = settingsManager;
        _vscodeService = vscodeService;
        _workspaceStorage = new WorkspaceStorage();
        ShowDetails = _settingsManager.ShowDetails;
        _refreshWorkspacesCommand = new(_vscodeService, settingsManager, this);
        _refreshWorkspacesCommandContextItem = new CommandContextItem(_refreshWorkspacesCommand);

        _helpPage = new HelpPage();
        TotalChanged += _helpPage.UpdateTotal;
        TotalVisualStudioChanged += _helpPage.UpdateTotalVisualStudio;
        TotalVisualStudioCodeChanged += _helpPage.UpdateTotalVisualStudioCode;
        _helpCommandContextItem = new CommandContextItem(_helpPage);
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
        _ = LoadInitialWorkspacesAsync();
    }

    public void StartRefresh()
    {
        _ = RefreshWorkspacesAsync(isUserInitiated: true);
    }

    private async Task LoadInitialWorkspacesAsync()
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        var workspaces = await _workspaceStorage.GetWorkspacesAsync();
        if (workspaces.Count > 0)
        {
            UpdateWorkspaceList(workspaces, CancellationToken.None);
        }

        await RefreshWorkspacesAsync(isUserInitiated: false);
    }

    public override IListItem[] GetItems()
    {
        try
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            lock (_itemsLock)
            {
                if (_allItems.Count == 0 && !IsLoading)
                {
                    RefreshWorkspacesAsync(isUserInitiated: false).GetAwaiter().GetResult();
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
        }
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (oldSearch == newSearch)
        {
            return;
        }
#if DEBUG
        using var logger = new TimeLogger();
        Logger.Log($"SearchText: {newSearch}");
#endif
        lock (_itemsLock)
        {
            _cachedFilteredItems = WorkspaceFilter.Filter(newSearch, _allItems, _settingsManager.SearchBy);
            _visibleItems.Clear();
            _visibleItems.AddRange(_cachedFilteredItems.Take(_settingsManager.PageSize));
            HasMoreItems = _cachedFilteredItems.Count > _settingsManager.PageSize;
        }

        RaiseItemsChanged(_visibleItems.Count);
    }

    public override void LoadMore()
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        IsLoading = true;

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
        RaiseItemsChanged(_visibleItems.Count);
    }

    private async Task RefreshWorkspacesAsync(bool isUserInitiated)
    {
        if (!_refreshSemaphore.Wait(0))
        {
            // Skip if a refresh is already in progress
            return;
        }
#if DEBUG
        using var logger = new TimeLogger();
#endif

        try
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            IsLoading = true;

            if (isUserInitiated)
            {
                _vscodeService.LoadInstances(_settingsManager.EnabledEditions, _settingsManager.PreferredEdition);
            }

            var dbWorkspaces = await _workspaceStorage.GetWorkspacesAsync();
            var workspacesTask = _vscodeService.GetWorkspacesAsync(dbWorkspaces, cancellationToken);
            Task<List<VisualStudioCodeWorkspace>>? solutionsTask = null;
            if (_settingsManager.EnableVisualStudio)
            {
                solutionsTask = _vscodeService.GetVisualStudioSolutions(dbWorkspaces, true);
            }
            await Task.WhenAll(workspacesTask, solutionsTask ?? Task.CompletedTask);

            if (cancellationToken.IsCancellationRequested) return;

            var workspaces = await workspacesTask;
            var solutions = solutionsTask != null ? await solutionsTask : new List<VisualStudioCodeWorkspace>();

            TotalVisualStudioCodeChanged?.Invoke(workspaces.Count);
            TotalVisualStudioChanged?.Invoke(solutions.Count);

            workspaces.AddRange(solutions);

            TotalChanged?.Invoke(workspaces.Count);

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
            if (!IsLoading)
            {
                // This can happen if the operation was cancelled very early.
                // Ensure the semaphore is released regardless.
                _refreshSemaphore.Release();
            }

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                IsLoading = false;
            }
            _refreshSemaphore.Release();
        }
    }

    private void UpdateWorkspaceList(List<VisualStudioCodeWorkspace> workspaces, CancellationToken cancellationToken)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        var newItems = workspaces
            .Select(w => WorkspaceItemFactory.Create(w, this, _workspaceStorage, _settingsManager, _refreshWorkspacesCommandContextItem, _helpCommandContextItem))
            .ToList();

        lock (_itemsLock)
        {
            if (cancellationToken.IsCancellationRequested) return;

            _allItems.Clear();
            _allItems.AddRange(newItems);

            // Apply the current filter to the newly loaded items
            _cachedFilteredItems = WorkspaceFilter.Filter(SearchText, _allItems, _settingsManager.SearchBy);
            _visibleItems.Clear();
            _visibleItems.AddRange(_cachedFilteredItems.Take(_settingsManager.PageSize));
            HasMoreItems = _cachedFilteredItems.Count > _settingsManager.PageSize;
        }
        RaiseItemsChanged(_visibleItems.Count);
    }

    private void OnPageSettingsChanged(object? sender, EventArgs e)
    {
        ShowDetails = _settingsManager.ShowDetails;
        UpdateSearchText(SearchText, SearchText);
    }

    public Task TogglePinStatus(string path)
    {
        lock (_itemsLock)
        {
            var itemToUpdate = _allItems.FirstOrDefault(item => (item.Command as IHasWorkspace)?.Workspace?.Path == path);
            if (itemToUpdate != null)
            {
                var workspace = (itemToUpdate.Command as IHasWorkspace)?.Workspace;
                if (workspace != null)
                {
                    workspace.PinDateTime = workspace.PinDateTime.HasValue ? null : DateTime.UtcNow;
                    if (_allItems.Remove(itemToUpdate))
                    {
                        _allItems.Add(WorkspaceItemFactory.Create(workspace, this, _workspaceStorage, _settingsManager, _refreshWorkspacesCommandContextItem, _helpCommandContextItem));
                    }

                    _cachedFilteredItems = WorkspaceFilter.Filter(SearchText, _allItems, _settingsManager.SearchBy);
                    _visibleItems.Clear();
                    _visibleItems.AddRange(_cachedFilteredItems.Take(_settingsManager.PageSize));
                    HasMoreItems = _cachedFilteredItems.Count > _settingsManager.PageSize;
                }
            }
        }
        RaiseItemsChanged(_visibleItems.Count);
        return Task.CompletedTask;
    }

    public async Task UpdateFrequencyAsync(string path)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif

        await _workspaceStorage.UpdateWorkspaceFrequencyAsync(path);

        lock (_itemsLock)
        {
            var itemToUpdate = _allItems.FirstOrDefault(item => (item.Command as IHasWorkspace)?.Workspace?.Path == path);
            if (itemToUpdate != null)
            {
                var openCommand = itemToUpdate.Command as IHasWorkspace;
                if (openCommand?.Workspace != null)
                {
                    openCommand.Workspace.Frequency++;
                }

                // Re-apply filter and sort
                _cachedFilteredItems = WorkspaceFilter.Filter(SearchText, _allItems, _settingsManager.SearchBy);
                _visibleItems.Clear();
                _visibleItems.AddRange(_cachedFilteredItems.Take(_settingsManager.PageSize));
                HasMoreItems = _cachedFilteredItems.Count > _settingsManager.PageSize;
            }
        }
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
        TotalChanged -= _helpPage.UpdateTotal;
        TotalVisualStudioChanged -= _helpPage.UpdateTotalVisualStudio;
        TotalVisualStudioCodeChanged -= _helpPage.UpdateTotalVisualStudioCode;
        _workspaceStorage.Dispose();
        _refreshSemaphore.Dispose();
    }
}
