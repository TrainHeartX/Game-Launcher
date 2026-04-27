using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GameLauncher.BigScreen.Transitions
{
    /// <summary>
    /// Control para presentar transiciones animadas entre vistas.
    /// </summary>
    public class TransitionPresenter : ContentControl
    {
        private ContentPresenter? _oldContentPresenter;
        private ContentPresenter? _newContentPresenter;
        private Grid? _rootGrid;

        static TransitionPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TransitionPresenter),
                new FrameworkPropertyMetadata(typeof(TransitionPresenter)));
        }

        public static readonly DependencyProperty TransitionTypeProperty =
            DependencyProperty.Register(
                nameof(TransitionType),
                typeof(TransitionType),
                typeof(TransitionPresenter),
                new PropertyMetadata(TransitionType.Fade));

        public static readonly DependencyProperty TransitionDurationProperty =
            DependencyProperty.Register(
                nameof(TransitionDuration),
                typeof(Duration),
                typeof(TransitionPresenter),
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300))));

        public TransitionType TransitionType
        {
            get => (TransitionType)GetValue(TransitionTypeProperty);
            set => SetValue(TransitionTypeProperty, value);
        }

        public Duration TransitionDuration
        {
            get => (Duration)GetValue(TransitionDurationProperty);
            set => SetValue(TransitionDurationProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rootGrid = new Grid();
            AddVisualChild(_rootGrid);

            _oldContentPresenter = new ContentPresenter();
            _newContentPresenter = new ContentPresenter();

            _rootGrid.Children.Add(_oldContentPresenter);
            _rootGrid.Children.Add(_newContentPresenter);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (_oldContentPresenter == null || _newContentPresenter == null)
                return;

            // Guardar contenido viejo
            _oldContentPresenter.Content = oldContent;
            _newContentPresenter.Content = newContent;

            // Ejecutar transición
            ExecuteTransition();
        }

        private void ExecuteTransition()
        {
            if (_oldContentPresenter == null || _newContentPresenter == null)
                return;

            switch (TransitionType)
            {
                case TransitionType.Fade:
                    ExecuteFadeTransition();
                    break;
                case TransitionType.SlideHorizontal:
                    ExecuteSlideHorizontalTransition();
                    break;
                case TransitionType.SlideVertical:
                    ExecuteSlideVerticalTransition();
                    break;
                case TransitionType.Scale:
                    ExecuteScaleTransition();
                    break;
                default:
                    // Sin transición
                    _oldContentPresenter.Opacity = 0;
                    _newContentPresenter.Opacity = 1;
                    break;
            }
        }

        private void ExecuteFadeTransition()
        {
            if (_oldContentPresenter == null || _newContentPresenter == null)
                return;

            // Fade out el contenido viejo
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TransitionDuration
            };

            // Fade in el contenido nuevo
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TransitionDuration
            };

            _oldContentPresenter.BeginAnimation(OpacityProperty, fadeOut);
            _newContentPresenter.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void ExecuteSlideHorizontalTransition()
        {
            if (_oldContentPresenter == null || _newContentPresenter == null || _rootGrid == null)
                return;

            var width = ActualWidth;

            // Preparar transforms
            _oldContentPresenter.RenderTransform = new TranslateTransform();
            _newContentPresenter.RenderTransform = new TranslateTransform(width, 0);

            // Slide out hacia la izquierda
            var slideOut = new DoubleAnimation
            {
                From = 0,
                To = -width,
                Duration = TransitionDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            // Slide in desde la derecha
            var slideIn = new DoubleAnimation
            {
                From = width,
                To = 0,
                Duration = TransitionDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            (_oldContentPresenter.RenderTransform as TranslateTransform)?.BeginAnimation(
                TranslateTransform.XProperty, slideOut);

            (_newContentPresenter.RenderTransform as TranslateTransform)?.BeginAnimation(
                TranslateTransform.XProperty, slideIn);
        }

        private void ExecuteSlideVerticalTransition()
        {
            if (_oldContentPresenter == null || _newContentPresenter == null)
                return;

            var height = ActualHeight;

            // Preparar transforms
            _oldContentPresenter.RenderTransform = new TranslateTransform();
            _newContentPresenter.RenderTransform = new TranslateTransform(0, height);

            // Slide out hacia arriba
            var slideOut = new DoubleAnimation
            {
                From = 0,
                To = -height,
                Duration = TransitionDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            // Slide in desde abajo
            var slideIn = new DoubleAnimation
            {
                From = height,
                To = 0,
                Duration = TransitionDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            (_oldContentPresenter.RenderTransform as TranslateTransform)?.BeginAnimation(
                TranslateTransform.YProperty, slideOut);

            (_newContentPresenter.RenderTransform as TranslateTransform)?.BeginAnimation(
                TranslateTransform.YProperty, slideIn);
        }

        private void ExecuteScaleTransition()
        {
            if (_oldContentPresenter == null || _newContentPresenter == null)
                return;

            // Preparar transforms
            _oldContentPresenter.RenderTransformOrigin = new Point(0.5, 0.5);
            _oldContentPresenter.RenderTransform = new ScaleTransform();

            _newContentPresenter.RenderTransformOrigin = new Point(0.5, 0.5);
            _newContentPresenter.RenderTransform = new ScaleTransform(0, 0);

            // Scale out (shrink)
            var scaleOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TransitionDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            // Scale in (grow)
            var scaleIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TransitionDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            // Fade out simultáneo
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TransitionDuration
            };

            // Fade in simultáneo
            var fadeIn = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TransitionDuration
            };

            var oldScale = _oldContentPresenter.RenderTransform as ScaleTransform;
            if (oldScale != null)
            {
                oldScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleOut);
                oldScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleOut);
            }
            _oldContentPresenter.BeginAnimation(OpacityProperty, fadeOut);

            var newScale = _newContentPresenter.RenderTransform as ScaleTransform;
            if (newScale != null)
            {
                newScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleIn);
                newScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleIn);
            }
            _newContentPresenter.BeginAnimation(OpacityProperty, fadeIn);
        }

        protected override int VisualChildrenCount => _rootGrid != null ? 1 : 0;

        protected override Visual GetVisualChild(int index)
        {
            if (_rootGrid == null || index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _rootGrid;
        }
    }

    /// <summary>
    /// Tipos de transición disponibles.
    /// </summary>
    public enum TransitionType
    {
        None,
        Fade,
        SlideHorizontal,
        SlideVertical,
        Scale
    }
}
