using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameLauncher.BigScreen.Services;
using GameLauncher.BigScreen.ViewModels;

namespace GameLauncher.BigScreen.Views
{
    /// <summary>
    /// Vista de selección de juegos (wheel horizontal) con video animado.
    /// </summary>
    public partial class GamesWheelView : Page
    {
        private bool _isBuffering;
        private bool _isMonitoringPlayback;
        private DateTime _playbackStartTime;

        // ── Attract Mode ──────────────────────────────────────────
        private AttractModeService? _attractMode;

        public void ConnectAttractMode(AttractModeService service)
        {
            _attractMode = service;
            _attractMode.AttractModeStarted  += OnAttractModeStarted;
            _attractMode.AttractModeStopped  += OnAttractModeStopped;
            _attractMode.GameChanged         += OnAttractModeGameChanged;

            // Reset idle timer on any key press on this page
            this.PreviewKeyDown += (_, __) => _attractMode?.ResetIdleTimer();
        }
        // ───────────────────────────────────────────────────────────────

        public GamesWheelView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is GamesWheelViewModel oldVm)
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;

            if (e.NewValue is GamesWheelViewModel newVm)
                newVm.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GamesWheelViewModel.SelectedGame))
            {
                var vm = DataContext as GamesWheelViewModel;
                if (vm?.SelectedGame != null)
                {
                    // Scroll in both controls depending on current view mode
                    GamesListBox.ScrollIntoView(vm.SelectedGame);
                    if (vm.IsListMode)
                        GamesListView.ScrollIntoView(vm.SelectedGame);
                }
            }
            else if (e.PropertyName == nameof(GamesWheelViewModel.GameVideoUri))
            {
                UpdateVideoPlayer();
            }
            else if (e.PropertyName == nameof(GamesWheelViewModel.EditorViewModel))
            {
                // Watch for field editing changes to manage TextBox focus
                var vm = DataContext as GamesWheelViewModel;
                if (vm?.EditorViewModel != null)
                    vm.EditorViewModel.PropertyChanged += EditorViewModel_PropertyChanged;
            }
        }

        private void EditorViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameEditorViewModel.IsFieldEditing))
            {
                var vm = DataContext as GamesWheelViewModel;
                if (vm?.EditorViewModel?.IsFieldEditing == true)
                {
                    // Focus the TextBox that just became visible
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
                    {
                        FocusEditorTextBox();
                    });
                }
            }
        }

        private void FocusEditorTextBox()
        {
            // Find the visible TextBox with Tag="EditorTextBox" inside EditorOverlay
            var textBox = FindVisibleEditorTextBox(EditorOverlay);
            if (textBox != null)
            {
                textBox.Focus();
                textBox.SelectAll();
                textBox.PreviewKeyDown -= EditorTextBox_PreviewKeyDown;
                textBox.PreviewKeyDown += EditorTextBox_PreviewKeyDown;
            }
        }

        private TextBox? FindVisibleEditorTextBox(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TextBox tb && tb.Tag as string == "EditorTextBox" && tb.Visibility == Visibility.Visible)
                    return tb;

                var result = FindVisibleEditorTextBox(child);
                if (result != null) return result;
            }
            return null;
        }

        private void EditorTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as GamesWheelViewModel;
            if (vm?.EditorViewModel == null) return;

            if (e.Key == Key.Enter && vm.EditorViewModel.SelectedField?.FieldType != EditorFieldType.MultilineText)
            {
                // Confirm text edit on Enter (except multiline)
                vm.EditorConfirmTextEdit();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // Cancel text edit on Escape
                vm.EditorCancelField();
                e.Handled = true;
            }
        }

        private void UpdateVideoPlayer()
        {
            try
            {
                StopMonitoringPlayback();
                GameVideoPlayer.Stop();
                GameVideoPlayer.Source = null;

                var vm = DataContext as GamesWheelViewModel;
                var uri = vm?.GameVideoUri;

                if (uri != null)
                {
                    // Make overlay Visible but nearly invisible (0.01, not 0).
                    // Opacity=0 lets WPF skip rendering entirely, preventing the
                    // video decoder from producing frames. 0.01 forces rendering
                    // while being imperceptible to the user.
                    _isBuffering = true;
                    VideoOverlay.Opacity = 0.01;
                    VideoOverlay.IsHitTestVisible = false;
                    VideoOverlay.Visibility = Visibility.Visible;
                    VideoScaleTransform.ScaleX = 0.3;
                    VideoScaleTransform.ScaleY = 0.3;

                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                    {
                        var currentVm = DataContext as GamesWheelViewModel;
                        if (currentVm?.GameVideoUri == uri)
                        {
                            GameVideoPlayer.Source = uri;
                        }
                    });
                }
                else
                {
                    HideVideoOverlay();
                }
            }
            catch
            {
            }
        }

        private void ShowVideoOverlay()
        {
            _isBuffering = false;
            VideoOverlay.IsHitTestVisible = true;
            VideoOverlay.Visibility = Visibility.Visible;

            // Activar audio solo cuando el video se hace visible
            GameVideoPlayer.Volume = 1;

            var duration = new Duration(TimeSpan.FromMilliseconds(400));
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            var scaleX = new DoubleAnimation(0.3, 1.0, duration) { EasingFunction = ease };
            var scaleY = new DoubleAnimation(0.3, 1.0, duration) { EasingFunction = ease };
            var opacity = new DoubleAnimation(0.01, 1.0, duration) { EasingFunction = ease };

            VideoScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            VideoScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
            VideoOverlay.BeginAnimation(OpacityProperty, opacity);
        }

        private void HideVideoOverlay()
        {
            StopMonitoringPlayback();
            _isBuffering = false;

            if (VideoOverlay.Visibility != Visibility.Visible)
                return;

            // If still nearly invisible (buffering phase), just collapse immediately
            if (VideoOverlay.Opacity < 0.05)
            {
                VideoOverlay.BeginAnimation(OpacityProperty, null);
                VideoOverlay.Opacity = 0;
                VideoOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            var duration = new Duration(TimeSpan.FromMilliseconds(250));
            var ease = new CubicEase { EasingMode = EasingMode.EaseIn };

            var scaleX = new DoubleAnimation(1.0, 0.3, duration) { EasingFunction = ease };
            var scaleY = new DoubleAnimation(1.0, 0.3, duration) { EasingFunction = ease };
            var opacity = new DoubleAnimation(1.0, 0.0, duration) { EasingFunction = ease };

            opacity.Completed += (s, e) =>
            {
                VideoOverlay.Visibility = Visibility.Collapsed;
            };

            VideoScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            VideoScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
            VideoOverlay.BeginAnimation(OpacityProperty, opacity);
        }

        private void GameVideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Start playback muted while overlay is nearly invisible.
            // Audio only activates when video is revealed on screen.
            GameVideoPlayer.Volume = 0;
            GameVideoPlayer.Play();
            _playbackStartTime = DateTime.UtcNow;
            StartMonitoringPlayback();
        }

        /// <summary>
        /// Subscribes to CompositionTarget.Rendering to poll video position
        /// each frame. Only reveals the video once Position has advanced past
        /// 300ms, proving the decoder is producing frames smoothly.
        /// </summary>
        private void StartMonitoringPlayback()
        {
            if (_isMonitoringPlayback) return;
            _isMonitoringPlayback = true;
            CompositionTarget.Rendering += OnRenderFrame;
        }

        private void StopMonitoringPlayback()
        {
            if (!_isMonitoringPlayback) return;
            _isMonitoringPlayback = false;
            CompositionTarget.Rendering -= OnRenderFrame;
        }

        private void OnRenderFrame(object? sender, EventArgs e)
        {
            if (!_isBuffering)
            {
                StopMonitoringPlayback();
                return;
            }

            try
            {
                var position = GameVideoPlayer.Position;
                var elapsed = DateTime.UtcNow - _playbackStartTime;

                // Video position has advanced past 500ms — decoder is producing frames
                if (position.TotalMilliseconds > 500)
                {
                    StopMonitoringPlayback();
                    ShowVideoOverlay();
                    return;
                }

                // Safety timeout: if 4 seconds have passed, show anyway
                if (elapsed.TotalMilliseconds > 4000)
                {
                    StopMonitoringPlayback();
                    ShowVideoOverlay();
                }
            }
            catch
            {
                StopMonitoringPlayback();
            }
        }

        private void GameVideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            GameVideoPlayer.Position = TimeSpan.Zero;
            GameVideoPlayer.Play();
        }

        private void GameVideoPlayer_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            StopMonitoringPlayback();
            _isBuffering = false;
            VideoOverlay.BeginAnimation(OpacityProperty, null);
            VideoOverlay.Opacity = 0;
            VideoOverlay.Visibility = Visibility.Collapsed;
        }

        // ── Attract Mode event handlers ───────────────────────────────────

        private void OnAttractModeStarted(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AttractModeOverlay.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(800)))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut } };
                AttractModeOverlay.BeginAnimation(OpacityProperty, fadeIn);

                // Start first video
                _attractMode?.PlayNext();
            });
        }

        private void OnAttractModeStopped(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                AttractVideoPlayer.Stop();
                AttractVideoPlayer.Source = null;

                var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(400)))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
                fadeOut.Completed += (_, __) => AttractModeOverlay.Visibility = Visibility.Collapsed;
                AttractModeOverlay.BeginAnimation(OpacityProperty, fadeOut);
            });
        }

        private void OnAttractModeGameChanged(object? sender, AttractModeGameArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // Feed video to the overlay's MediaElement
                AttractVideoPlayer.Source = new Uri(e.VideoPath, UriKind.Absolute);
                AttractVideoPlayer.Play();

                // Update labels
                AttractGameTitle.Text = e.Game.Title ?? string.Empty;
                var year = e.Game.ReleaseDate?.Year.ToString() ?? string.Empty;
                var dev  = e.Game.Developer ?? string.Empty;
                AttractGameMeta.Text = string.Join("  ·  ",
                    new[] { dev, year, e.Game.Platform }.Where(s => !string.IsNullOrWhiteSpace(s)));
            });
        }

        /// <summary>Routed event from XAML MediaEnded on AttractVideoPlayer.</summary>
        private void AttractVideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _attractMode?.PlayNext();
        }
    }
}
