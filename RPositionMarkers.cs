using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    /// <summary>
    /// This control displays a set of dots for a carousel to show its position.  More generically, this control
    /// displays a set of markers that can be selected programmatically, and notifies handlers if a marker is clicked.
    /// The selected marker is set to the "Highlighted" visual state while the others are set to the "NonHighlighted".
    /// </summary>
    public sealed class RPositionMarkers : GridView
    {
        /// <summary>
        /// Event arguments is the sender (this) and the item of the marker that was clicked on.
        /// </summary>
        public event EventHandler<object> OnMarkerClicked;

        public RPositionMarkers()
        {
            this.DefaultStyleKey = typeof(RPositionMarkers);
            IsTabStop = false;
            XYFocusKeyboardNavigation = XYFocusKeyboardNavigationMode.Disabled;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.ItemClick += OnItemClicked;
            this.SelectionChanged += OnSelectionChanged;
        }

        private void OnItemClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem != this.SelectedItem)
            {
                // The click is only notified.  The selection change is decided and initiated by the event handler(s).
                OnMarkerClicked?.Invoke(this, e.ClickedItem);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When the selection changes, dim or highlight the old and new selected marker elements, respectively.
            foreach (object eRemovedItem in e.RemovedItems)
            {
                ChangeItemVisualState(eRemovedItem, "NonHighlighted");
            }
            foreach (object eAddedItem in e.AddedItems)
            {
                ChangeItemVisualState(eAddedItem, "Highlighted");
            }
        }

        private void ChangeItemVisualState(object item, string visualStateName)
        {
            var container = ContainerFromItem(item);
            if (container is ContentControl contentControl)
            {
                var markerElement = contentControl.ContentTemplateRoot;
                if (markerElement is Control markerControl)
                {
                    VisualStateManager.GoToState(markerControl, visualStateName, true);
                }
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var control = element as Control;
            if (control != null) control.IsTabStop = false;
        }
    }
}
