using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameLauncher.BigScreen.ViewModels;

namespace GameLauncher.BigScreen.Views
{
    /// <summary>
    /// Vista de selección de plataformas (wheel vertical) con video.
    /// </summary>
    public partial class PlatformFiltersView : Page
    {
        private bool _audioEnabled;
        private bool _isWaitingForPlayback;
        private bool _isMonitoringPlayback;
        private DateTime _playbackStartTime;
        private DispatcherTimer? _sagaNotesScrollTimer;

        public PlatformFiltersView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Place video container in the correct host on initial load
            ReparentVideoPanel();
        }

        /// <summary>
        /// Enables video audio. Called after splash screen is dismissed.
        /// </summary>
        public void EnableVideoAudio()
        {
            _audioEnabled = true;
            PlatformVideoPlayer.Volume = 1;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PlatformFiltersViewModel oldVm)
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;

            if (e.NewValue is PlatformFiltersViewModel newVm)
                newVm.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlatformFiltersViewModel.PlatformVideoUri))
            {
                UpdateVideoPlayer();
            }
            else if (e.PropertyName == nameof(PlatformFiltersViewModel.SelectedItem))
            {
                var vm = DataContext as PlatformFiltersViewModel;
                if (vm?.SelectedItem != null)
                {
                    PlatformListBox.ScrollIntoView(vm.SelectedItem);
                }

                // Reset video/placeholder when changing items to ensure clean state
                ResetToPlaceholder();
            }
            else if (e.PropertyName == nameof(PlatformFiltersViewModel.IsPlaylistSelected))
            {
                ReparentVideoPanel();
                UpdateSagaNotesScroll();
            }
            else if (e.PropertyName == nameof(PlatformFiltersViewModel.SagaCollectionStatus))
            {
                UpdateSagaStatusBadgeColor();
            }
        }

        private void SagaCoversScroller_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var current = SagaCoversScroller.HorizontalOffset;
            SagaCoversScroller.ScrollToHorizontalOffset(current - e.Delta);
            e.Handled = true;
        }

        /// <summary>
        /// Scrolls the game covers strip horizontally by the specified pixel delta.
        /// </summary>
        public void ScrollCovers(double delta)
        {
            try
            {
                var current = SagaCoversScroller.HorizontalOffset;
                SagaCoversScroller.ScrollToHorizontalOffset(current + delta);
            }
            catch { }
        }

        /// <summary>
        /// Scrolls the saga info panel (metadata + description) by the specified pixel delta.
        /// </summary>
        public void ScrollDescription(double delta)
        {
            try
            {
                var current = SagaInfoScroller.VerticalOffset;
                SagaInfoScroller.ScrollToVerticalOffset(current + delta);
            }
            catch { }
        }

        /// <summary>
        /// Seeks the video by the specified number of seconds (positive = forward, negative = backward).
        /// </summary>
        public void SeekVideo(double seconds)
        {
            try
            {
                if (PlatformVideoPlayer.Source == null) return;
                if (!PlatformVideoPlayer.NaturalDuration.HasTimeSpan) return;

                var duration = PlatformVideoPlayer.NaturalDuration.TimeSpan;
                var newPosition = PlatformVideoPlayer.Position + TimeSpan.FromSeconds(seconds);

                if (newPosition < TimeSpan.Zero)
                    newPosition = TimeSpan.Zero;
                else if (newPosition > duration)
                    newPosition = duration;

                PlatformVideoPlayer.Position = newPosition;
            }
            catch { }
        }

        private void UpdateSagaStatusBadgeColor()
        {
            var vm = DataContext as PlatformFiltersViewModel;
            var status = vm?.SagaCollectionStatus;

            Brush background;
            if (string.Equals(status, "SAGA COMPLETA", StringComparison.OrdinalIgnoreCase))
                background = (Brush)FindResource("SagaCompletaBrush");
            else if (string.Equals(status, "SEMI COMPLETA", StringComparison.OrdinalIgnoreCase))
                background = (Brush)FindResource("SemiCompletaBrush");
            else if (string.Equals(status, "SAGA PARCIAL", StringComparison.OrdinalIgnoreCase))
                background = (Brush)FindResource("SagaParcialBrush");
            else
                background = new SolidColorBrush(Color.FromRgb(0x00, 0xd4, 0xff)); // default cyan

            SagaStatusBadge.Background = background;
        }

        /// <summary>
        /// Moves the VideoContainer between StandardVideoHost and PlaylistVideoHost
        /// depending on whether a playlist is selected.
        /// </summary>
        private void ReparentVideoPanel()
        {
            var vm = DataContext as PlatformFiltersViewModel;
            bool isPlaylist = vm?.IsPlaylistSelected ?? false;

            // Remove from current parent
            if (VideoContainer.Parent is Border currentParent)
            {
                currentParent.Child = null;
            }
            else if (VideoContainer.Parent is Grid parentGrid)
            {
                parentGrid.Children.Remove(VideoContainer);
            }

            // Place into the appropriate host
            VideoContainer.Visibility = Visibility.Visible;
            if (isPlaylist)
            {
                PlaylistVideoHost.Child = VideoContainer;
            }
            else
            {
                StandardVideoHost.Child = VideoContainer;
            }

        }

        private void UpdateSagaNotesScroll()
        {
            var vm = DataContext as PlatformFiltersViewModel;
            bool isPlaylist = vm?.IsPlaylistSelected ?? false;

            if (isPlaylist && !string.IsNullOrEmpty(vm?.SagaDescription))
            {
                StartSagaNotesAutoScroll();
            }
            else
            {
                StopSagaNotesAutoScroll();
            }
        }

        private void StartSagaNotesAutoScroll()
        {
            if (_sagaNotesScrollTimer != null) return;

            _sagaNotesScrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _sagaNotesScrollTimer.Tick += SagaNotesScrollTimer_Tick;
            _sagaNotesScrollTimer.Start();
        }

        private void StopSagaNotesAutoScroll()
        {
            if (_sagaNotesScrollTimer == null) return;

            _sagaNotesScrollTimer.Stop();
            _sagaNotesScrollTimer.Tick -= SagaNotesScrollTimer_Tick;
            _sagaNotesScrollTimer = null;

            // Reset scroll position
            SagaInfoScroller.ScrollToVerticalOffset(0);
        }

        private void SagaNotesScrollTimer_Tick(object? sender, EventArgs e)
        {
            var currentOffset = SagaInfoScroller.VerticalOffset;
            var maxOffset = SagaInfoScroller.ScrollableHeight;

            if (maxOffset <= 0) return;

            if (currentOffset >= maxOffset)
            {
                // Reset to top after reaching the end
                SagaInfoScroller.ScrollToVerticalOffset(0);
            }
            else
            {
                SagaInfoScroller.ScrollToVerticalOffset(currentOffset + 0.5);
            }
        }

        private void ResetToPlaceholder()
        {
            StopMonitoringPlayback();
            _isWaitingForPlayback = false;
            PlatformVideoPlayer.Stop();
            PlatformVideoPlayer.Source = null;
            PlatformVideoPlayer.BeginAnimation(OpacityProperty, null);
            PlatformVideoPlayer.Opacity = 0;
            VideoPlaceholder.BeginAnimation(OpacityProperty, null);
            VideoPlaceholder.Opacity = 1;
            VideoPlaceholder.Visibility = Visibility.Visible;
        }

        private void UpdateVideoPlayer()
        {
            try
            {
                StopMonitoringPlayback();
                _isWaitingForPlayback = false;
                PlatformVideoPlayer.Stop();
                PlatformVideoPlayer.Source = null;

                // Reset visual state: hide video, show placeholder
                PlatformVideoPlayer.BeginAnimation(OpacityProperty, null);
                PlatformVideoPlayer.Opacity = 0;
                VideoPlaceholder.BeginAnimation(OpacityProperty, null);
                VideoPlaceholder.Opacity = 1;
                VideoPlaceholder.Visibility = Visibility.Visible;

                var vm = DataContext as PlatformFiltersViewModel;
                var uri = vm?.PlatformVideoUri;

                if (uri != null)
                {
                    _isWaitingForPlayback = true;
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
                    {
                        var currentVm = DataContext as PlatformFiltersViewModel;
                        if (currentVm?.PlatformVideoUri == uri)
                        {
                            PlatformVideoPlayer.Source = uri;
                        }
                    });
                }
            }
            catch
            {
            }
        }

        private void PlatformVideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Start playback hidden (Opacity=0 on video, placeholder on top).
            // Audio muted until video is actually visible on screen.
            PlatformVideoPlayer.Volume = 0;
            PlatformVideoPlayer.Play();

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
            if (!_isWaitingForPlayback)
            {
                StopMonitoringPlayback();
                return;
            }

            try
            {
                var position = PlatformVideoPlayer.Position;
                var elapsed = DateTime.UtcNow - _playbackStartTime;

                // Video position has advanced past 2.5s — decoder is well past any initial stutter
                if (position.TotalMilliseconds > 2500)
                {
                    StopMonitoringPlayback();
                    _isWaitingForPlayback = false;
                    RevealVideo();
                    return;
                }

                // Safety timeout: show anyway after 8 seconds
                if (elapsed.TotalMilliseconds > 8000)
                {
                    StopMonitoringPlayback();
                    _isWaitingForPlayback = false;
                    RevealVideo();
                }
            }
            catch
            {
                StopMonitoringPlayback();
            }
        }

        private void RevealVideo()
        {
            // Activar audio solo cuando el video se hace visible
            if (_audioEnabled)
                PlatformVideoPlayer.Volume = 1;

            // Crossfade: fade in video, fade out placeholder
            var duration = new Duration(TimeSpan.FromMilliseconds(400));
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            var fadeInVideo = new DoubleAnimation(0.0, 1.0, duration) { EasingFunction = ease };
            var fadeOutPlaceholder = new DoubleAnimation(1.0, 0.0, duration) { EasingFunction = ease };

            fadeOutPlaceholder.Completed += (s, e) =>
            {
                VideoPlaceholder.Visibility = Visibility.Collapsed;
            };

            PlatformVideoPlayer.BeginAnimation(OpacityProperty, fadeInVideo);
            VideoPlaceholder.BeginAnimation(OpacityProperty, fadeOutPlaceholder);
        }

        private void PlatformVideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlatformVideoPlayer.Position = TimeSpan.Zero;
            PlatformVideoPlayer.Play();
        }

        private void PlatformVideoPlayer_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            StopMonitoringPlayback();
            _isWaitingForPlayback = false;
            PlatformVideoPlayer.BeginAnimation(OpacityProperty, null);
            PlatformVideoPlayer.Opacity = 0;
            VideoPlaceholder.BeginAnimation(OpacityProperty, null);
            VideoPlaceholder.Opacity = 1;
            VideoPlaceholder.Visibility = Visibility.Visible;
        }
    }
}
