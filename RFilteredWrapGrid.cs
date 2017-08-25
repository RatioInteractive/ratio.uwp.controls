using System;
using System.Collections;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    public sealed class RFilteredWrapGrid : GridView
    {
        public event EventHandler ItemsLoaded;
        public RFilteredWrapGrid()
        {
            UseSystemFocusVisuals = true;
            DefaultStyleKey = typeof(RFilteredWrapGrid);
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var collection = ItemsSource as IList;
            if (collection == null || collection.Count <= 0) return;
            if (Items == null || collection.Count != Items.Count) return;
            OnItemsLoaded();
            SizeChanged -= OnSizeChanged;
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Debug.WriteLine($"Focus obtained by Wrap Grid");
            base.OnGotFocus(e);
        }

        private void OnItemsLoaded()
        {
            ItemsLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
