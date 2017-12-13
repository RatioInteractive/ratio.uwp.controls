using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    public class BaseItem :GridViewItem
    {
        public static readonly DependencyProperty SelectedCommandProperty = DependencyProperty.Register(
            "SelectedCommand", typeof(ICommand), typeof(BaseItem), new PropertyMetadata(default(ICommand)));

        public ICommand SelectedCommand
        {
            get => (ICommand)GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }
        public object SourceItem { get; set; }

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
    }
}
