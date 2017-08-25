using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    [TemplateVisualState(GroupName = "ItemsStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "ItemsStates", Name = "PointerOver")]
    [TemplateVisualState(GroupName = "ItemsStates", Name = "SelectedState")]
    public sealed class RFilterItem : ContentControl
    {
        public event EventHandler<object> ItemSelected;
        #region Fields

        private string _pendingState;

        #endregion

        #region Properties

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register(
            "Selected", typeof(bool), typeof(RFilterItem), new PropertyMetadata(default(bool), SelectedChanged));

        private static void SelectedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rFilterItem = dependencyObject as RFilterItem;
            if (rFilterItem == null) return;
            if ((bool)dependencyPropertyChangedEventArgs.NewValue)
            {
                var success = VisualStateManager.GoToState(rFilterItem, "SelectedState", true);
                if (!success) rFilterItem._pendingState = "SelectedState";
            }
            else
            {
                var success = VisualStateManager.GoToState(rFilterItem, "Normal", true);
                if (!success) rFilterItem._pendingState = "Normal";
            }
        }

        public bool Selected
        {
            get => (bool)GetValue(SelectedProperty);
            set => SetValue(SelectedProperty, value);
        }

        public List<RFilterItem> Siblings { get; set; }

        #endregion

        public RFilterItem()
        {
            DefaultStyleKey = typeof(RFilterItem);
            Siblings = new List<RFilterItem>();
            Loaded += (sender, args) =>
            {
                VisualStateManager.GoToState(this, "Normal", true);
                LayoutUpdated += OnLayoutUpdated;
            };
        }

        private void OnLayoutUpdated(object o, object o1)
        {
            if (_pendingState != null) VisualStateManager.GoToState(this, _pendingState, true);
            LayoutUpdated -= OnLayoutUpdated;
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            var success = VisualStateManager.GoToState(this, "PointerOver", true);
            Debug.WriteLine($"State change to PointerOver success: {success}");
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (Selected) return;
            SetSelected();
            OnItemSelected(DataContext);
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Space || e.Key == VirtualKey.Enter)
            {
                SetSelected();
                OnItemSelected(DataContext);
            }
            base.OnKeyUp(e);
        }

        private void SetSelected()
        {
            if (Selected) return;
            Selected = true;
            foreach (var rFilterItem in Siblings)
            {
                rFilterItem.Selected = false;
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            if (Selected)
            {
                var success = VisualStateManager.GoToState(this, "SelectedState", true);
                Debug.WriteLine($"State change to SelectedState success: {success}");
            }
            else
            {
                var success = VisualStateManager.GoToState(this, "Normal", true);
                Debug.WriteLine($"State change to Normal success: {success}");
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Debug.WriteLine("Filter Item got focus.");
            base.OnGotFocus(e);
        }

        private void OnItemSelected(object e)
        {
            ItemSelected?.Invoke(this, e);
        }
    }
}
