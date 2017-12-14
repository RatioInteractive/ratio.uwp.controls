using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace Ratio.UWP.Controls
{
    [TemplatePart(Name = "ScrollViewer", Type = typeof(ScrollViewer))]
    [ContentProperty(Name = "Content")]
    public sealed class RVerticalScroller : Control
    {
        #region Properties

        private ScrollViewer _scrollViewer;
        #region Attached Properties
        public static readonly DependencyProperty SuppressHorizontalScrollWheelProperty = DependencyProperty.RegisterAttached(
            "SuppressHorizontalScrollWheel", typeof(bool), typeof(RVerticalScroller), new PropertyMetadata(default(bool)));

        public static void SetSuppressHorizontalScrollWheel(DependencyObject target, bool value)
        {
            target.SetValue(SuppressHorizontalScrollWheelProperty,value);
            
        }
        public static bool GetSuppressHorizontalScrollWheel(DependencyObject target)
        {
            return (bool) target.GetValue(SuppressHorizontalScrollWheelProperty);
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(RVerticalScroller), new PropertyMetadata(default(object)));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

                public static readonly DependencyProperty VerticalScrollStepSizeProperty = DependencyProperty.Register(
            "VerticalScrollStepSize", typeof(double), typeof(RRowlist), new PropertyMetadata(50));

        public double VerticalScrollStepSize
        {
            get => (double) GetValue(VerticalScrollStepSizeProperty);
            set => SetValue(VerticalScrollStepSizeProperty, value);
        }

        #endregion

        public double? VerticalOffset => _scrollViewer?.VerticalOffset;
        #endregion

        public RVerticalScroller()
        {
            DefaultStyleKey = typeof(RVerticalScroller);
        }

        public void WireupSuppressedControls()
        {
            var wireupAction = new Action<DependencyObject>(depObject =>
            {
                if (!GetSuppressHorizontalScrollWheel(depObject)) return;
                WireUpScrollWheel(depObject);
            });
            ControlUtilities.TraverseAndApply(this, wireupAction);
        }

        public void UnwireSuppressedControls()
        {
            var unwireAction = new Action<DependencyObject>(depObject =>
            {
                if (!GetSuppressHorizontalScrollWheel(depObject)) return;
                UnwireScrollWheel(depObject);
            });
            ControlUtilities.TraverseAndApply(this, unwireAction);
        }

        public void ScrollToVerticalOffset(double? offset)
        {
            _scrollViewer?.ChangeView(0, offset, 1, true);
        }

        #region Overrides

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
        }

        #endregion

        #region Event Handlers
        private void ScrollContentPresenterOnPointerWheelChanged(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            if (_scrollViewer == null) return;
            var pointer = pointerRoutedEventArgs.GetCurrentPoint(_scrollViewer);
            double delta = pointer.Properties.MouseWheelDelta > 0 ? (-1 * VerticalScrollStepSize) : VerticalScrollStepSize;
            _scrollViewer.ChangeView(0, _scrollViewer.VerticalOffset + delta, 1);
            pointerRoutedEventArgs.Handled = true;
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == VirtualKey.GamepadLeftThumbstickUp)
            {
                ScrollToVerticalOffset(_scrollViewer.VerticalOffset - VerticalScrollStepSize);
            }
            else if (e.OriginalKey == VirtualKey.GamepadLeftThumbstickDown)
            {
                ScrollToVerticalOffset(_scrollViewer.VerticalOffset + VerticalScrollStepSize);
            }
            base.OnKeyUp(e);
        }

        #endregion

        #region Support Methods

        private void UnwireScrollWheel(DependencyObject control)
        {
            var scrollContentPresenter = ControlUtilities.RecursiveFindByType(control, typeof(ScrollContentPresenter)) as ScrollContentPresenter;
            if (scrollContentPresenter != null) scrollContentPresenter.PointerWheelChanged -= ScrollContentPresenterOnPointerWheelChanged;
            var horizontalScrollbar = ControlUtilities.RecursiveFindByType(control, typeof(ScrollBar), "HorizontalScrollBar") as ScrollBar;
            if (horizontalScrollbar != null) horizontalScrollbar.PointerWheelChanged -= ScrollContentPresenterOnPointerWheelChanged;
        }

        private void WireUpScrollWheel(DependencyObject control)
        {
            var scrollContentPresenter = ControlUtilities.RecursiveFindByType(control, typeof(ScrollContentPresenter)) as ScrollContentPresenter;
            if (scrollContentPresenter != null)
            {
                scrollContentPresenter.PointerWheelChanged += ScrollContentPresenterOnPointerWheelChanged;
            }
            var horizontalScrollbar = ControlUtilities.RecursiveFindByType(control, typeof(ScrollBar), "HorizontalScrollBar") as ScrollBar;
            if (horizontalScrollbar != null)
            {
                horizontalScrollbar.PointerWheelChanged += ScrollContentPresenterOnPointerWheelChanged;
            }
        }

        #endregion
    }
}
