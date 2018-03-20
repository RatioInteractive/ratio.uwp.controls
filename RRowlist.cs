using System;
using System.Collections;
using System.Linq;
using System.Windows.Input;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    [TemplatePart(Name = "rowlistWrapGrid", Type = typeof(ItemsWrapGrid))]
    [TemplatePart(Name = "ScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "leftButton", Type = typeof(Button))]
    [TemplatePart(Name = "rightButton", Type = typeof(Button))]
    public sealed class RRowlist : GridView
    {
        #region Properties

        #region Dependency Properties

        #region Dimensions

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(
            "ItemSize", typeof(Size), typeof(RRowlist), new PropertyMetadata(default(Size)));

        public Size ItemSize
        {
            get => (Size) GetValue(ItemSizeProperty);
            set => SetValue(ItemSizeProperty, value);
        }
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth", typeof(double), typeof(RRowlist), new PropertyMetadata(default(double),ItemWidthChanged));

        private static void ItemWidthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rowlist = dependencyObject as RRowlist;
            if(rowlist?._rowlistWrapGrid == null) return;
//            rowlist._rowlistWrapGrid.ItemWidth = (double)dependencyPropertyChangedEventArgs.NewValue;
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight", typeof(double), typeof(RRowlist), new PropertyMetadata(default(double),ItemHeightChanged));

        private static void ItemHeightChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rowlist = dependencyObject as RRowlist;
            if (rowlist?._rowlistWrapGrid == null) return;
//            rowlist._rowlistWrapGrid.ItemHeight = (double)dependencyPropertyChangedEventArgs.NewValue;
        }

        public static readonly DependencyProperty ScrollButtonWidthProperty = DependencyProperty.Register(
            "ScrollButtonWidth", typeof(double), typeof(RRowlist), new PropertyMetadata(default(double)));

        public double ScrollButtonWidth
        {
            get => (double) GetValue(ScrollButtonWidthProperty);
            set => SetValue(ScrollButtonWidthProperty, value);
        }

        public static readonly DependencyProperty ScrollButtonHeightProperty = DependencyProperty.Register(
            "ScrollButtonHeight", typeof(double), typeof(RRowlist), new PropertyMetadata(default(double)));

        public double ScrollButtonHeight
        {
            get => (double) GetValue(ScrollButtonHeightProperty);
            set => SetValue(ScrollButtonHeightProperty, value);
        }

        public static readonly DependencyProperty ScrollButtonSizeProperty = DependencyProperty.Register(
            "ScrollButtonSize", typeof(Size), typeof(RRowlist), new PropertyMetadata(default(Size),ScrollButtonSizePropertyChangedCallback));

        private static void ScrollButtonSizePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rowlist = dependencyObject as RRowlist;
            if(rowlist == null) return;
            rowlist.ScrollButtonHeight = ((Size) dependencyPropertyChangedEventArgs.NewValue).Height;
            rowlist.ScrollButtonWidth = ((Size) dependencyPropertyChangedEventArgs.NewValue).Width;
        }

        public Size ScrollButtonSize
        {
            get => (Size) GetValue(ScrollButtonSizeProperty);
            set => SetValue(ScrollButtonSizeProperty, value);
        }

        public static readonly DependencyProperty LabelContainerHeightProperty = DependencyProperty.Register(
            "LabelContainerHeight", typeof(GridLength), typeof(RRowlist), new PropertyMetadata(new GridLength(60,GridUnitType.Pixel)));

        public static readonly DependencyProperty LeftButtonContainerWidthProperty = DependencyProperty.Register(
            "LeftButtonContainerWidth", typeof(GridLength), typeof(RRowlist), new PropertyMetadata(new GridLength(40,GridUnitType.Pixel)));

        public static readonly DependencyProperty RightButtonContainerWidthProperty = DependencyProperty.Register(
            "RightButtonContainerWidth", typeof(GridLength), typeof(RRowlist), new PropertyMetadata(new GridLength(40, GridUnitType.Pixel)));

        public GridLength RightButtonContainerWidth
        {
            get => (GridLength) GetValue(RightButtonContainerWidthProperty);
            set => SetValue(RightButtonContainerWidthProperty, value);
        }

        public GridLength LeftButtonContainerWidth
        {
            get => (GridLength) GetValue(LeftButtonContainerWidthProperty);
            set => SetValue(LeftButtonContainerWidthProperty, value);
        }

        public GridLength LabelContainerHeight
        {
            get => (GridLength) GetValue(LabelContainerHeightProperty);
            set => SetValue(LabelContainerHeightProperty, value);
        }

        public double ItemHeight
        {
            get => (double) GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public double ItemWidth
        {
            get => (double) GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }
        #endregion

        #region Templates

        public static readonly DependencyProperty LabelTemplateProperty = DependencyProperty.Register(
            "LabelTemplate", typeof(DataTemplate), typeof(RRowlist), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty LeftButtonStyleProperty = DependencyProperty.Register(
            "LeftButtonStyle", typeof(Style), typeof(RRowlist), new PropertyMetadata(default(Style)));

        public static readonly DependencyProperty RightButtonStyleProperty = DependencyProperty.Register(
            "RightButtonStyle", typeof(Style), typeof(RRowlist), new PropertyMetadata(default(Style)));

        public Style RightButtonStyle
        {
            get => (Style) GetValue(RightButtonStyleProperty);
            set => SetValue(RightButtonStyleProperty, value);
        }

        public Style LeftButtonStyle
        {
            get => (Style) GetValue(LeftButtonStyleProperty);
            set => SetValue(LeftButtonStyleProperty, value);
        }

        public DataTemplate LabelTemplate
        {
            get => (DataTemplate) GetValue(LabelTemplateProperty);
            set => SetValue(LabelTemplateProperty, value);
        }

        #endregion

        public static readonly DependencyProperty LeftButtonContentProperty = DependencyProperty.Register(
            "LeftButtonContent", typeof(object), typeof(RRowlist), new PropertyMetadata(""));

        public static readonly DependencyProperty RightButtonContentProperty = DependencyProperty.Register(
            "RightButtonContent", typeof(object), typeof(RRowlist), new PropertyMetadata(""));

        public static readonly DependencyProperty LabelContentProperty = DependencyProperty.Register(
            "LabelContent", typeof(object), typeof(RRowlist), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ShiftStepsProperty = DependencyProperty.Register(
            "ShiftSteps", typeof(int), typeof(RRowlist), new PropertyMetadata(1));

        public static readonly DependencyProperty SelectedCommandNameProperty = DependencyProperty.Register(
            "SelectedCommandName", typeof(string), typeof(RRowlist), new PropertyMetadata(default(string)));

        public string SelectedCommandName
        {
            get => (string) GetValue(SelectedCommandNameProperty);
            set => SetValue(SelectedCommandNameProperty, value);
        }

        public int ShiftSteps
        {
            get => (int) GetValue(ShiftStepsProperty);
            set => SetValue(ShiftStepsProperty, value);
        }


        public object LabelContent
        {
            get => GetValue(LabelContentProperty);
            set => SetValue(LabelContentProperty, value);
        }

        public object RightButtonContent
        {
            get => GetValue(RightButtonContentProperty);
            set => SetValue(RightButtonContentProperty, value);
        }

        public object LeftButtonContent
        {
            get => GetValue(LeftButtonContentProperty);
            set => SetValue(LeftButtonContentProperty, value);
        }
        #endregion

        private ScrollViewer _scrollViewer;
        private ItemsWrapGrid _rowlistWrapGrid;
        private Button _leftButton;
        private Button _rightButton;
        private RRowlistSaveState _pendingState;

        #endregion
        public RRowlist()
        {
            DefaultStyleKey = typeof(RRowlist);
            Loaded += OnRRowlistLoaded;
            Unloaded += OnUnloaded;
        }

        #region Overrides
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            _leftButton = GetTemplateChild("leftButton") as Button;
            _rightButton = GetTemplateChild("rightButton") as Button;

            if(_leftButton != null)
            {
                _leftButton.Click += LeftButtonOnClick;
                _leftButton.Visibility = Visibility.Collapsed;
            }

            if (_rightButton != null)
            {
                _rightButton.Click += RightButtonOnClick;
            }
            
            if (_scrollViewer == null) return;

            _scrollViewer.Loaded += (sender, args) => WireUp();
            _scrollViewer.Unloaded += (sender, args) => Unwire();

            // Subscribe to events so that the left/right buttons can be shown or hidden accordingly.
            PointerEntered += RRowlist_PointerEntered;
            PointerExited += RRowlist_PointerExited;
            GotFocus += RRowlist_GotFocus;
            LostFocus += RRowlist_LostFocus;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RowlistItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is RowlistItem rowlistItem)
            {
                if (!string.IsNullOrEmpty(SelectedCommandName))
                {
                    var binding = new Binding
                    {
                        Path = new PropertyPath(SelectedCommandName),
                        Source = item
                    };
                    rowlistItem.SetBinding(BaseItem.SelectedCommandProperty, binding);
                    var sizeBinding = new Binding()
                    {
                        Path = new PropertyPath("ItemSize"),
                        Source = this
                    };
                    rowlistItem.SetBinding(BaseItem.SpecifiedSizeProperty, sizeBinding);
                }
                rowlistItem.SourceItem = item;

            }
            base.PrepareContainerForItemOverride(element, item);
        }



        #endregion

        #region Event Handlers
        private void OnUnloaded(object o, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnRRowlistLoaded;
            Unloaded -= OnUnloaded;
            _leftButton.Click -= LeftButtonOnClick;
            _rightButton.Click -= RightButtonOnClick;
            PointerEntered -= RRowlist_PointerEntered;
            PointerExited -= RRowlist_PointerExited;
            GotFocus -= RRowlist_GotFocus;
            LostFocus -= RRowlist_LostFocus;
            Unwire();
            ItemsSource = null;
            DataContext = null;
        }

        private void OnRRowlistLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _rowlistWrapGrid = ItemsPanelRoot as ItemsWrapGrid;
            if (_rowlistWrapGrid != null)
            {
                _rowlistWrapGrid.ItemHeight = ItemSize.Height;
                _rowlistWrapGrid.ItemWidth = ItemSize.Width;
            }
            LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, object o)
        {
            if(Items == null) return;
            var list = ItemsSource as IList;
            if(list == null) return;
            if(list.Count == 0) return;
            // We want to make sure that all the items have been generated and match with the items source.
            if (list.Count != Items.Count) return;
            RestoreVisualState();
            LayoutUpdated -= OnLayoutUpdated;
        }

        private void RRowlist_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(null).PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                VisualStateManager.GoToState(this, "ButtonsShowing", true);
            }
        }

        private void RRowlist_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "ButtonsHidden", true);
        }

        private void RRowlist_GotFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "ButtonsShowing", true);
            StartBringIntoView(new BringIntoViewOptions() {AnimationDesired = true});
        }

        private void RRowlist_LostFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "ButtonsHidden", true);
        }

        private void RightButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (double.IsNaN(ItemSize.Width)) return;
            var currentOffset = _scrollViewer.HorizontalOffset;
            var changedOffset = currentOffset + (ShiftSteps > 0 ? ShiftSteps * ItemSize.Width : ItemSize.Width);
            _scrollViewer.ChangeView(changedOffset, 0, 1, false);
        }

        private void LeftButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (double.IsNaN(ItemSize.Width)) return;
            var currentOffset = _scrollViewer.HorizontalOffset;
            var changedOffset = currentOffset - (ShiftSteps > 0 ? ShiftSteps * ItemSize.Width : ItemSize.Width);
            _scrollViewer.ChangeView(changedOffset > 0 ? changedOffset : 0, 0, 1, false);
        }

        private void ScrollViewerOnViewChanged(object sender, ScrollViewerViewChangedEventArgs scrollViewerViewChangedEventArgs)
        {
            if (scrollViewerViewChangedEventArgs.IsIntermediate) return;
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null) return;
            _leftButton.Visibility = scrollViewer.HorizontalOffset < 1 ? Visibility.Collapsed : Visibility.Visible;
            _rightButton.Visibility = scrollViewer.HorizontalOffset > (scrollViewer.ExtentWidth - scrollViewer.ViewportWidth - 2) ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        #region Public Methods

        public void RestoreState(RRowlistSaveState state)
        {
            if(state == null) return;
            if (_scrollViewer == null)
            {
                _pendingState = state;
            }
            else
            {
                _scrollViewer.ChangeView(state.HorizontalOffset, 0, 1, true);
            }
            

        }

        public RRowlistSaveState SaveState()
        {
           return new RRowlistSaveState() {HorizontalOffset = _scrollViewer?.HorizontalOffset ?? 0};
        }
        #endregion

        #region Support Methods
        private void Unwire()
        {
            if (_scrollViewer != null) _scrollViewer.ViewChanged -= ScrollViewerOnViewChanged;
        }

        private void WireUp()
        {
            _scrollViewer.ViewChanged += ScrollViewerOnViewChanged;
        }

        private void RestoreVisualState()
        {
            if (_scrollViewer != null && _pendingState != null)
            {
                if (_scrollViewer.ExtentWidth - _scrollViewer.ViewportWidth > 0)
                {
                    _scrollViewer.ChangeView(_pendingState.HorizontalOffset, 0, 1, true);
                    _pendingState = null;
                }
            }
        }
        #endregion


    }
}
