using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ratio.UWP.Controls
{
    public sealed class RItemsControl : ItemsControl
    {
        private bool _wiredup;
        public event EventHandler ItemsCompleted;
        public RItemsControl()
        {
            DefaultStyleKey = typeof(RItemsControl);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var collection = ItemsSource as IList;
            if (collection == null || collection.Count <= 0) return;
            if (_wiredup || Items == null || collection.Count != Items.Count) return;
            _wiredup = true;
            OnItemsCompleted();
        }

        private void OnItemsCompleted()
        {
            ItemsCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
