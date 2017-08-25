using System.Windows.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Ratio.UWP.Controls
{
    [ContentProperty(Name = "Content")]
    public sealed class RCommandPanel : ContentControl
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(RCommandPanel), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter", typeof(object), typeof(RCommandPanel), new PropertyMetadata(default(object)));

        public object CommandParameter
        {
            get => (object) GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public bool MarkAsHandled { get; set; }

        public RCommandPanel()
        {
            Background = new SolidColorBrush(Colors.Transparent);
            MarkAsHandled = true;
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (Command == null) return;
            if (!Command.CanExecute(CommandParameter)) return;
            Command.Execute(CommandParameter);
            e.Handled = MarkAsHandled;
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Space && e.Key != VirtualKey.Enter) return;
            if (Command == null) return;
            if (!Command.CanExecute(CommandParameter)) return;
            Command.Execute(CommandParameter);
            e.Handled = MarkAsHandled;
        }
    }
}
