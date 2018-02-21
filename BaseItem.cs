﻿using System;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    public class BaseItem :GridViewItem
    {
        public Binding ItemBinding { get; set; }
        public static readonly DependencyProperty SelectedCommandProperty = DependencyProperty.Register(
            "SelectedCommand", typeof(ICommand), typeof(BaseItem), new PropertyMetadata(default(ICommand)));

        public ICommand SelectedCommand
        {
            get => (ICommand)GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }
        public object SourceItem { get; set; }

        public BaseItem()
        {
            Unloaded += (sender, args) =>
            {

                if (ItemBinding.Source is IDisposable itemDisposable) itemDisposable.Dispose();
                ItemBinding = null;
                if (SourceItem is IDisposable disposableSourceItem) disposableSourceItem.Dispose();
                SourceItem = null;
                if (DataContext is IDisposable disposable) disposable.Dispose();
                DataContext = null;
            };
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
//            Debug.WriteLine($"Key captured by item control. Key: {e.OriginalKey}");
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
