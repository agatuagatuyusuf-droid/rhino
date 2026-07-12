using System;

namespace RhinoCommercialPlatform.UI.Shell;

public sealed class MainShellState
{
    private NavigationPageId _currentPage = NavigationPageId.Dashboard;

    public event EventHandler<NavigationPageId>? PageChanged;

    public NavigationPageId CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage == value)
                return;

            var previous = _currentPage;
            _currentPage = value;
            PageChanged?.Invoke(this, value);
        }
    }

    public NavigationPageId PreviousPage { get; private set; } = NavigationPageId.Dashboard;

    public void NavigateTo(NavigationPageId pageId)
    {
        if (pageId == _currentPage)
            return;

        PreviousPage = _currentPage;
        CurrentPage = pageId;
    }

    public void GoBack()
    {
        NavigateTo(PreviousPage);
    }
}
