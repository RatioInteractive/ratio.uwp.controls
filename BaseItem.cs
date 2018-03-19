using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Ratio.UWP.Controls
{
    [TemplatePart(Name = "listViewItemPresenter", Type = typeof(ListViewItemPresenter))]
    public class BaseItem :GridViewItem
    {
        private ListViewItemPresenter _listViewItemPresenter;
        private IContainerFocusable _focusable;
        public Binding ItemBinding { get; set; }
        public static readonly DependencyProperty SelectedCommandProperty = DependencyProperty.Register(
            "SelectedCommand", typeof(ICommand), typeof(BaseItem), new PropertyMetadata(default(ICommand)));

        public ICommand SelectedCommand
        {
            get => (ICommand)GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }

        public static readonly DependencyProperty SpecifiedSizeProperty = DependencyProperty.Register(
            "SpecifiedSize", typeof(Size), typeof(BaseItem), new PropertyMetadata(default(Size)));

        public Size SpecifiedSize
        {
            get => (Size) GetValue(SpecifiedSizeProperty);
            set => SetValue(SpecifiedSizeProperty, value);
        }

        public object SourceItem { get; set; }

        private IContainerFocusable Focusable
        {
            get
            {
                if (DesignMode.DesignModeEnabled) return null;
                if (_focusable != null) return _focusable;
                if (_listViewItemPresenter == null) return null;
                if (!(VisualTreeHelper.GetChild(_listViewItemPresenter,0) is IContainerFocusable child)) return null;
                _focusable = child;
                return _focusable;
            }
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (SelectedCommand != null)
            {
                if (SelectedCommand.CanExecute(SourceItem))
                {
                    SelectedCommand.Execute(SourceItem);
                    e.Handled = true;
                }
            }
            base.OnTapped(e);
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            Debug.WriteLine($"Key captured by item control. Key: {e.OriginalKey}");
            if ((e.OriginalKey == VirtualKey.GamepadA || e.Key == VirtualKey.Space || e.Key == VirtualKey.Enter) && SelectedCommand != null)
            {
                if (SelectedCommand.CanExecute(SourceItem))
                {
                    SelectedCommand.Execute(SourceItem);
                    e.Handled = true;
                }
            }
            base.OnKeyUp(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Focusable?.ContainerGotFocus();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            Focusable?.ContainerLostFocus();
            base.OnLostFocus(e);
        }

        protected override void OnApplyTemplate()
        {
            _listViewItemPresenter = GetTemplateChild("listViewItemPresenter") as ListViewItemPresenter;
            base.OnApplyTemplate();
        }

//        protected override Size MeasureOverride(Size availableSize)
//        {
//            if (!double.IsInfinity(availableSize.Height))
//            {
//                Debug.WriteLine($"availibleSize: {availableSize}, Speced Size: {SpecifiedSize}");
//                if (SpecifiedSize.Width > 0 && Math.Abs(SpecifiedSize.Width - availableSize.Width) < 2.0f)
//                {
//                    return SpecifiedSize;
//                }
//            }
//            return base.MeasureOverride(availableSize);
//        }
    }
}
