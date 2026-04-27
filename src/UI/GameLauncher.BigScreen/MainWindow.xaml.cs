using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using GameLauncher.BigScreen.ViewModels;

namespace GameLauncher.BigScreen;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// Handles global keyboard navigation for BigScreen mode.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        PreviewKeyDown += MainWindow_PreviewKeyDown;

        // Set version text from assembly
        try
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
                SplashVersionText.Text = $"v{version.Major}.{version.Minor}.{version.Build}";
        }
        catch { }
    }

    /// <summary>
    /// Updates the splash status text.
    /// </summary>
    public void SetSplashStatus(string text)
    {
        SplashStatusText.Text = text;
    }


    /// <summary>
    /// Hides the splash overlay with a fade-out animation.
    /// onComplete is called after the animation finishes.
    /// </summary>
    public void DismissSplash(Action? onComplete = null)
    {
        if (SplashOverlay.Visibility != Visibility.Visible)
        {
            onComplete?.Invoke();
            return;
        }

        var fadeOut = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(800)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        fadeOut.Completed += (s, e) =>
        {
            SplashOverlay.Visibility = Visibility.Collapsed;
            onComplete?.Invoke();
        };

        SplashOverlay.BeginAnimation(OpacityProperty, fadeOut);
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Block all input while splash is visible
        if (SplashOverlay.Visibility == Visibility.Visible)
        {
            e.Handled = true;
            return;
        }

        // Get current page's DataContext (ViewModel)
        var frame = FindName("NavigationFrame") as Frame;
        var currentPage = frame?.Content as Page;
        var dataContext = currentPage?.DataContext;

        if (dataContext is PlatformFiltersViewModel platformVM)
        {
            HandlePlatformKeys(platformVM, e);
        }
        else if (dataContext is HomeViewModel homeVM)
        {
            HandleHomeKeys(homeVM, e);
        }
        else if (dataContext is SourcesViewModel sourcesVM)
        {
            HandleSourcesKeys(sourcesVM, e);
        }
        else if (dataContext is GamesWheelViewModel gamesVM)
        {
            HandleGamesKeys(gamesVM, e);
        }
        else if (dataContext is GameDetailsViewModel detailsVM)
        {
            HandleDetailsKeys(detailsVM, e);
        }

        // Global: Escape goes back
        if (!e.Handled && e.Key == Key.Escape)
        {
            var app = Application.Current as App;
            // Use reflection-free approach: just go back if possible
            if (frame != null && frame.CanGoBack)
            {
                frame.GoBack();
                e.Handled = true;
            }
        }
    }

    private void HandlePlatformKeys(PlatformFiltersViewModel vm, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:
                vm.NavigateUpCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Down:
                vm.NavigateDownCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Left:
                GetPlatformFiltersView()?.SeekVideo(-5);
                e.Handled = true;
                break;
            case Key.Right:
                GetPlatformFiltersView()?.SeekVideo(5);
                e.Handled = true;
                break;
            case Key.Enter:
                vm.SelectItemCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Escape:
                if (vm.CanGoBack)
                {
                    vm.GoBackCommand.Execute(null);
                    e.Handled = true;
                }
                break;
        }
    }

    private Views.PlatformFiltersView? GetPlatformFiltersView()
    {
        var frame = FindName("NavigationFrame") as Frame;
        return frame?.Content as Views.PlatformFiltersView;
    }

    private void HandleGamesKeys(GamesWheelViewModel vm, KeyEventArgs e)
    {
        // If any overlay is active, route input there instead
        if (vm.ShowingManageMenu)
        {
            switch (e.Key)
            {
                case Key.Up:    vm.ManageNavigateUp();   e.Handled = true; break;
                case Key.Down:  vm.ManageNavigateDown(); e.Handled = true; break;
                case Key.Enter:
                case Key.Space: vm.ManageToggleSelected(); e.Handled = true; break;
                case Key.Escape:
                case Key.Back:  vm.CloseManageMenu();    e.Handled = true; break;
            }
            return;
        }

        if (vm.ShowingEditor)
        {
            switch (e.Key)
            {
                case Key.Up:    vm.EditorNavigateUp();        e.Handled = true; break;
                case Key.Down:  vm.EditorNavigateDown();      e.Handled = true; break;
                case Key.Left:  vm.EditorNavigateLeft();      e.Handled = true; break;
                case Key.Right: vm.EditorNavigateRight();     e.Handled = true; break;
                case Key.Enter: vm.EditorConfirm();           e.Handled = true; break;
                case Key.Escape:
                case Key.Back:  vm.EditorCancelField();       e.Handled = true; break;
                case Key.Tab:   vm.EditorNextSection();       e.Handled = true; break;
                case Key.S:
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                        vm.EditorPreviousSection();
                    else
                        _ = vm.EditorSaveAsync();
                    e.Handled = true;
                    break;
            }
            return;
        }

        if (vm.ShowingGallery)
        {
            switch (e.Key)
            {
                case Key.Left:  vm.GalleryNavigate(-1, 0);  e.Handled = true; break;
                case Key.Right: vm.GalleryNavigate(1, 0);   e.Handled = true; break;
                case Key.Up:    vm.GalleryNavigate(0, -1);  e.Handled = true; break;
                case Key.Down:  vm.GalleryNavigate(0, 1);   e.Handled = true; break;
                case Key.Enter: vm.GallerySelect();          e.Handled = true; break;
                case Key.Escape:
                case Key.Back:  vm.GalleryBack();            e.Handled = true; break;
                case Key.PageUp:   vm.PageLeftCommand.Execute(null);  e.Handled = true; break;
                case Key.PageDown: vm.PageRightCommand.Execute(null); e.Handled = true; break;
            }
            return;
        }

        switch (e.Key)
        {
            // Navigation
            case Key.Left:
                vm.NavigateLeftCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Right:
                vm.NavigateRightCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.PageUp:
                vm.PageLeftCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.PageDown:
                vm.PageRightCommand.Execute(null);
                e.Handled = true;
                break;

            // BUG-01 FIX: MVVM Toolkit strips 'Async' suffix → LaunchGameCommand, not LaunchGameAsyncCommand
            case Key.Enter:
                vm.LaunchGameCommand.Execute(null);
                e.Handled = true;
                break;

            // Game management
            case Key.A:
                vm.OpenManageMenuCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F:
                vm.ToggleFavoriteCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.X:
                vm.ShowDetailsCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.I:
                vm.ShowImagesCommand.Execute(null);
                e.Handled = true;
                break;

            // Phase 3: Sort / Filter / View
            case Key.Tab:
                vm.ToggleViewModeCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.S:
                vm.CycleSortFieldCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.D:
                vm.ToggleSortDirectionCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Q:
                vm.CycleFilterCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }

    private void HandleDetailsKeys(GameDetailsViewModel vm, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                vm.LaunchGameCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F:
                vm.ToggleFavoriteCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.C:
                vm.ToggleCompletedCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }

    private void HandleHomeKeys(HomeViewModel vm, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                vm.NavigateLeftCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Right:
                vm.NavigateRightCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Enter:
                _ = vm.NavigateToSelectedCardCommand.ExecuteAsync(null);
                e.Handled = true;
                break;
        }
    }

    private void HandleSourcesKeys(SourcesViewModel vm, KeyEventArgs e)
    {
        if (vm.ShowDetails)
        {
            // Input when Details Overlay is open
            switch (e.Key)
            {
                case Key.Back:
                case Key.Escape:
                    vm.GoBackToListCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Enter:
                case Key.Space:
                    vm.OpenInBrowserCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
        else
        {
            // Input when in List View
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Space:
                    _ = vm.OpenDetailsAsync(); // Open detailed view instead of browser directly
                    e.Handled = true;
                    break;
                case Key.Tab:
                    _ = vm.SwitchSourceCommand.ExecuteAsync(null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
