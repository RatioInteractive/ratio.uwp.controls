using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

#pragma warning disable 169

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Ratio.UWP.Controls
{
    [TemplatePart(Name = "loopingStackPanel", Type = typeof(RLoopingStackPanel))]
    [TemplatePart(Name = "leftButton", Type = typeof(Button))]
    [TemplatePart(Name = "rightButton", Type = typeof(Button))]
    [TemplatePart(Name = "leftPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "rightPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "positionMarkers", Type = typeof(RPositionMarkers))]
    public sealed class RCarousel : Control
    {
        #region Properties
        private RCarouselSaveState _pendingStateRestoration;
        private Grid _rootGrid;
        private RLoopingStackPanel _loopingStackPanel;
        private RPositionMarkers _positionMarkers;
        private Border _focusVisual;
        private ScrollViewer _carouselScrollViewer;
        private Button _leftButton;
        private Button _rightButton;
        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _isAutoRotationInitialized;
        private bool _isNavigatedByKeyboard;
        private bool _showFocusVisualOnResizeCompleted;
        private Storyboard _focusVisualEntrance;
        private Storyboard _focusVisualExit;

        private Storyboard FocusVisualEntrance => _focusVisualEntrance ?? (_focusVisualEntrance = _rootGrid.Resources["FocusVisualEntrance"] as Storyboard);
        private Storyboard FocusVisualExit => _focusVisualExit ?? (_focusVisualExit = _rootGrid.Resources["FocusVisualExit"] as Storyboard);

        #region Dependency Properties

        public static readonly DependencyProperty AutoRotationEnabledProperty = DependencyProperty.Register(
            "AutoRotationEnabled", typeof(bool), typeof(RCarousel), new PropertyMetadata(default(bool)));

        public bool AutoRotationEnabled
        {
            get => (bool)GetValue(AutoRotationEnabledProperty);
            set
            {
                SetValue(AutoRotationEnabledProperty, value);
                UpdateAutoRotationConfiguration();
            }
        }

        public static readonly DependencyProperty AutoRotationIntervalSecondsProperty = DependencyProperty.Register(
            "AutoRotationIntervalSeconds", typeof(int), typeof(RCarousel), new PropertyMetadata(default(int)));

        public int AutoRotationIntervalSeconds
        {
            get => (int)GetValue(AutoRotationIntervalSecondsProperty);
            set
            {
                SetValue(AutoRotationIntervalSecondsProperty, value);
                UpdateAutoRotationConfiguration();
            }
        }

        public static readonly DependencyProperty FocusedItemProperty = DependencyProperty.Register(
            "FocusedItem", typeof(object), typeof(RCarousel), new PropertyMetadata(default(object)));

        public object FocusedItem
        {
            get => GetValue(FocusedItemProperty);
            set => SetValue(FocusedItemProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight", typeof(int), typeof(RCarousel), new PropertyMetadata(default(int), OnItemHeightChanged));

        private static void OnItemHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = d as RCarousel;
            if (carousel?._focusVisual != null)
            {
                carousel._focusVisual.Height = (int) e.NewValue;
            }
        }

        /// <summary>
        ///  The height of each item.  If ItemWidth is not an absolute value, this property is ignored.
        /// </summary>
        public int ItemHeight
        {
            get => (int)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth", typeof(double), typeof(RCarousel), new PropertyMetadata(default(double), OnItemWidthChanged));

        private static void OnItemWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = d as RCarousel;
            if (carousel?._focusVisual != null)
            {
                carousel._focusVisual.Width = (double) e.NewValue;
            }
        }

        /// <summary>
        /// The width of each item in the carousel.  The width may be specified as an absolute value or as relative to
        /// the carousel's width.  If the value is greater than 1, it is treated as an absolute value.  If it is 1
        /// or less, it is treated as a relative value; it is not possible to set an absolute item width of 1.
        /// Note: When ItemWidth is treated as relative, ItemHeight is ignored and the actual height is calculated
        /// using ItemAspectRatio.
        /// </summary>
        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly DependencyProperty ItemAspectRatioProperty = DependencyProperty.Register(
            "ItemAspectRatio", typeof(double), typeof(RCarousel), new PropertyMetadata(default(double)));

        /// <summary>
        /// The aspect ratio to apply to calculate the item height.  This value is only used when ItemWidth is treated
        /// as a fraction of the carousel's width, and ItemHeight would be ignored.
        /// </summary>
        public double ItemAspectRatio
        {
            get => (double)GetValue(ItemAspectRatioProperty);
            set => SetValue(ItemAspectRatioProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(RCarousel), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register(
            "ItemContainerStyle", typeof(Style), typeof(RCarousel), new PropertyMetadata(default(Style), OnItemContainerStyleChanged));

        private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = d as RCarousel;
            carousel?.UpdateFocusVisualSize();
        }

        public Style ItemContainerStyle
        {
            get => (Style)GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IList), typeof(RCarousel), new PropertyMetadata(default(IList)));

        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty LeftButtonContainerWidthProperty = DependencyProperty.Register(
            "LeftButtonContainerWidth", typeof(GridLength), typeof(RCarousel), new PropertyMetadata(new GridLength(40, GridUnitType.Pixel)));

        public GridLength LeftButtonContainerWidth
        {
            get => (GridLength)GetValue(LeftButtonContainerWidthProperty);
            set => SetValue(LeftButtonContainerWidthProperty, value);
        }

        public static readonly DependencyProperty RightButtonContainerWidthProperty = DependencyProperty.Register(
            "RightButtonContainerWidth", typeof(GridLength), typeof(RCarousel), new PropertyMetadata(new GridLength(40, GridUnitType.Pixel)));

        public GridLength RightButtonContainerWidth
        {
            get => (GridLength)GetValue(RightButtonContainerWidthProperty);
            set => SetValue(RightButtonContainerWidthProperty, value);
        }

        public static readonly DependencyProperty LeftButtonStyleProperty = DependencyProperty.Register(
            "LeftButtonStyle", typeof(Style), typeof(RCarousel), new PropertyMetadata(default(Style)));

        public Style LeftButtonStyle
        {
            get => (Style)GetValue(LeftButtonStyleProperty);
            set => SetValue(LeftButtonStyleProperty, value);
        }

        public static readonly DependencyProperty RightButtonStyleProperty = DependencyProperty.Register(
            "RightButtonStyle", typeof(Style), typeof(RCarousel), new PropertyMetadata(default(Style)));

        public Style RightButtonStyle
        {
            get => (Style)GetValue(RightButtonStyleProperty);
            set => SetValue(RightButtonStyleProperty, value);
        }

        public static readonly DependencyProperty LeftButtonContentProperty = DependencyProperty.Register(
            "LeftButtonContent", typeof(object), typeof(RCarousel), new PropertyMetadata(""));

        public object LeftButtonContent
        {
            get => GetValue(LeftButtonContentProperty);
            set => SetValue(LeftButtonContentProperty, value);
        }

        public static readonly DependencyProperty RightButtonContentProperty = DependencyProperty.Register(
            "RightButtonContent", typeof(object), typeof(RCarousel), new PropertyMetadata(""));

        public object RightButtonContent
        {
            get => GetValue(RightButtonContentProperty);
            set => SetValue(RightButtonContentProperty, value);
        }

        public static readonly DependencyProperty ShiftStepsProperty = DependencyProperty.Register(
            "ShiftSteps", typeof(int), typeof(RCarousel), new PropertyMetadata(1));

        public int ShiftSteps
        {
            get => (int)GetValue(ShiftStepsProperty);
            set => SetValue(ShiftStepsProperty, value);
        }

        public static readonly DependencyProperty SelectedCommandNameProperty = DependencyProperty.Register(
            "SelectedCommandName", typeof(string), typeof(RCarousel), new PropertyMetadata(default(string)));

        public string SelectedCommandName
        {
            get => (string) GetValue(SelectedCommandNameProperty);
            set => SetValue(SelectedCommandNameProperty, value);
        }

        public static readonly DependencyProperty DisplayMarkersProperty = DependencyProperty.Register(
            "DisplayMarkers", typeof(bool), typeof(RCarousel), new PropertyMetadata(true));

        public bool DisplayMarkers
        {
            get => (bool) GetValue(DisplayMarkersProperty);
            set => SetValue(DisplayMarkersProperty, value);
        }

        public static readonly DependencyProperty DisplayScrollButtonsProperty = DependencyProperty.Register(
            "DisplayScrollButtons", typeof(bool), typeof(RCarousel), new PropertyMetadata(default(bool)));

        public bool DisplayScrollButtons
        {
            get => (bool) GetValue(DisplayScrollButtonsProperty);
            set => SetValue(DisplayScrollButtonsProperty, value);
        }
        #endregion
        #endregion

        #region Public Methods
        public RCarouselSaveState SaveState()
        {
            return new RCarouselSaveState(_loopingStackPanel.SaveState());
        }

        public void RestoreState(RCarouselSaveState saveState)
        {
            if (saveState != null)
            {
                if (_loopingStackPanel != null)
                {
                    _loopingStackPanel.RestoreState(new RLoopingStackPanelSaveState(saveState));
                }
                else
                {
                    _pendingStateRestoration = saveState;
                }
            }

        }
        #endregion


        public RCarousel()
        {
            TabFocusNavigation = KeyboardNavigationMode.Once;
            DefaultStyleKey = typeof(RCarousel);
        }

        private void InitializeAutoRotation()
        {
            _timer.Tick += AutoRotation_Tick;
            _timer.Interval = new System.TimeSpan(0, 0, AutoRotationIntervalSeconds);
            if (AutoRotationEnabled) _timer.Start();
            _isAutoRotationInitialized = true;
        }

        private void DisableAutoRotation()
        {
            if (_timer.IsEnabled) _timer.Stop();
            _timer.Tick -= AutoRotation_Tick;
            _isAutoRotationInitialized = false;
        }

        private void UpdateAutoRotationConfiguration()
        {
            if (_isAutoRotationInitialized) DisableAutoRotation();
            InitializeAutoRotation();
        }

        private void ResetAutoRotationTimer()
        {
            if (_isAutoRotationInitialized)
            {
                if (_timer.IsEnabled) _timer.Stop();
                if (AutoRotationEnabled) _timer.Start();
            }
        }

        private void AutoRotation_Tick(object sender, object e)
        {
            _loopingStackPanel?.AutoScrollBySteps(ShiftSteps);
        }

        #region Overrides
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateAutoRotationConfiguration();
            _rootGrid = GetTemplateChild("rootGrid") as Grid;
            _loopingStackPanel = GetTemplateChild("loopingStackPanel") as RLoopingStackPanel;
            _leftButton = GetTemplateChild("leftButton") as Button;
            _rightButton = GetTemplateChild("rightButton") as Button;
            _positionMarkers = GetTemplateChild("positionMarkers") as RPositionMarkers;
            _focusVisual = GetTemplateChild("focusVisual") as Border;
            if (_loopingStackPanel != null)
            {
                _loopingStackPanel.DirectManipulationStarted += LoopingStackPanelOnDirectManipulationStarted;
                _loopingStackPanel.FocusedItemChanged += LoopingStackPanelOnFocusedItemChanged;
                _loopingStackPanel.ItemsPopulated += LoopingStackPanelOnItemsPopulated;
                _loopingStackPanel.ResizingStarted += LoopingStackPanelOnResizingStarted;
                _loopingStackPanel.ResizingCompleted += LoopingStackPanelOnResizingCompleted;
                _loopingStackPanel.ScrollViewChanged += LoopingStackPanelOnScrollViewChanged;
                if (_pendingStateRestoration != null)
                {
                    _loopingStackPanel.RestoreState(new RLoopingStackPanelSaveState(_pendingStateRestoration));
                    _pendingStateRestoration = null;
                }

            }
            if (_leftButton != null)
            {
                if (DisplayScrollButtons)
                {
                    _leftButton.Click += LeftButtonOnClick;
                }
                _leftButton.Visibility = Visibility.Collapsed;
            }

            if (_rightButton != null)
            {
                if (DisplayScrollButtons)
                {
                    _rightButton.Click += RightButtonOnClick;
                }
                else
                {
                    _rightButton.Visibility = Visibility.Collapsed;
                }
            }

            if (_positionMarkers != null)
            {
                if (DisplayMarkers)
                {
                    _positionMarkers.OnMarkerClicked += PositionMarkersOnMarkerClicked;
                }
                else
                {
                    _positionMarkers.Visibility = Visibility.Collapsed;
                }
            }

            // Subscribe to events so that the left/right buttons can be shown or hidden accordingly. 
            PointerEntered += OnPointerEntered;
            PointerExited += OnPointerExited;
//            GotFocus += RCarousel_GotFocus;
//            LostFocus += RCarousel_LostFocus;

            UpdateFocusVisualSize();
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(null).PointerDevice.PointerDeviceType == PointerDeviceType.Mouse)
            {
                VisualStateManager.GoToState(this, "ButtonsShowing", true);
            }
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "ButtonsHidden", true);
        }

//        private void RCarousel_GotFocus(object sender, RoutedEventArgs e)
//        {
//            VisualStateManager.GoToState(this, "ButtonsShowing", true);
//        }
//
//        private void RCarousel_LostFocus(object sender, RoutedEventArgs e)
//        {
//            VisualStateManager.GoToState(this, "ButtonsHidden", true);
//        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Debug.WriteLine($"Focus obtained by RCarousel.");
            DisableAutoRotation();
            base.OnGotFocus(e);
            VisualStateManager.GoToState(this, "ButtonsShowing", true);
            ShowFocusVisual();
            StartBringIntoView(new BringIntoViewOptions() {AnimationDesired = true});
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            VisualStateManager.GoToState(this, "ButtonsHidden", true);
            HideFocusVisual();
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == VirtualKey.GamepadLeftThumbstickLeft || e.OriginalKey == VirtualKey.GamepadLeftThumbstickRight || e.OriginalKey == VirtualKey.GamepadLeftThumbstickUp || e.OriginalKey == VirtualKey.GamepadLeftThumbstickDown)
            {
                _isNavigatedByKeyboard = true;
                e.Handled = e.OriginalKey == VirtualKey.GamepadLeftThumbstickLeft || e.OriginalKey == VirtualKey.GamepadLeftThumbstickRight;
                DisableAutoRotation();
                base.OnKeyDown(e);
                return;
            }
            switch (e.Key)
            {
                case VirtualKey.Left:
                    _isNavigatedByKeyboard = true;
                    ScrollLeft();
                    e.Handled = true;
                    break;

                case VirtualKey.Right:
                    _isNavigatedByKeyboard = true;
                    ScrollRight();
                    e.Handled = true;
                    break;

//                case VirtualKey.Up:
//                case VirtualKey.Down:
//                case VirtualKey.PageUp:
//                case VirtualKey.PageDown:
//                case VirtualKey.Home:
//                case VirtualKey.End:
//                    // Don't let these keys bubble up and affect navigation on the page.
//                    e.Handled = true;
//                    break;

                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        #endregion

        #region Support Methods
        private void LoopingStackPanelOnDirectManipulationStarted(object sender, EventArgs e)
        {
            HideFocusVisual();
        }

        private void LoopingStackPanelOnFocusedItemChanged(object sender, EventArgs e)
        {
            if(DisplayMarkers)
                _positionMarkers.SelectedItem = _loopingStackPanel.FocusedItem;
            // For all the reasons that caused the focus item to change, reset the auto-rotation so a full period is
            // waited before the next change.
            ResetAutoRotationTimer();
        }

        private void LoopingStackPanelOnItemsPopulated(object sender, EventArgs e)
        {
            // When there are actual items, it's now possible to fully determine the focus visual's size (namely margin).
            UpdateFocusVisualSize();
        }

        private void LoopingStackPanelOnResizingCompleted(object sender, EventArgs e)
        {
            if (_showFocusVisualOnResizeCompleted)
            {
                _showFocusVisualOnResizeCompleted = false;
                ShowFocusVisual();
            }
        }

        private void LoopingStackPanelOnResizingStarted(object sender, EventArgs e)
        {
            _showFocusVisualOnResizeCompleted = _focusVisual.Opacity > 0;
            HideFocusVisual();
        }

        private void LoopingStackPanelOnScrollViewChanged(object sender, ScrollViewer scrollViewer)
        {
            if (DisplayScrollButtons)
            {
                _leftButton.Visibility = scrollViewer.HorizontalOffset < 1 ? Visibility.Collapsed : Visibility.Visible;
                _rightButton.Visibility = scrollViewer.HorizontalOffset > (scrollViewer.ExtentWidth - scrollViewer.ViewportWidth - 2) ? Visibility.Collapsed : Visibility.Visible;
            }

            // At the end of a scroll, show the focus visual if not shown already (it may have been hidden by a direct
            // manipulation) and the scroll was triggered by keyboard navigation.
            if (_isNavigatedByKeyboard)
            {
                _isNavigatedByKeyboard = false;
                ShowFocusVisual();
            }
        }

        private void LeftButtonOnClick(object sender, RoutedEventArgs e)
        {
            HideFocusVisual();
            ScrollLeft();
        }

        private void ScrollLeft()
        {
            if (double.IsNaN(ItemWidth)) return;
            _loopingStackPanel.ScrollBySteps(-1 * ShiftSteps);
        }

        private void RightButtonOnClick(object sender, RoutedEventArgs e)
        {
            HideFocusVisual();
            ScrollRight();
        }

        private void ScrollRight()
        {
            if (double.IsNaN(ItemWidth)) return;
            _loopingStackPanel.ScrollBySteps(ShiftSteps);
        }

        private void PositionMarkersOnMarkerClicked(object sender, object clickedItem)
        {
            if (DisplayScrollButtons)
                _loopingStackPanel.JumpToItem(clickedItem);
        }

        private void UpdateFocusVisualSize()
        {
            if (_focusVisual != null && ItemsSource != null)
            {
                var carouselItem = FocusedItem as CarouselItem;
                var horzOffset = 0.0;
                var vertOffset = 0.0;
                if (carouselItem != null)
                {
                    horzOffset = carouselItem.Margin.Left + carouselItem.Margin.Right + carouselItem.Padding.Left + carouselItem.Padding.Right;
                    vertOffset = carouselItem.Margin.Top + carouselItem.Margin.Bottom + carouselItem.Padding.Top + carouselItem.Padding.Bottom;
                }
                _focusVisual.Width = ItemWidth - horzOffset;
                _focusVisual.Height = ItemHeight - vertOffset;
//                if (ItemsSource.Count > 0) _focusVisual.Margin = _loopingStackPanel.GetItemContainerMargin();
            }
        }

        private void ShowFocusVisual()
        {
            FocusVisualExit.SkipToFill();
            if (_focusVisual != null && (int)Math.Round(_focusVisual.Opacity) == 0)
            {
                FocusVisualEntrance.Begin();
                DisableAutoRotation();
            }
        }

        private void HideFocusVisual()
        {
            Debug.WriteLine("Hide Focus border attempted.");
            FocusVisualEntrance.SkipToFill();
            if (_focusVisual != null && _focusVisual.Opacity > 0)
            {
                FocusVisualExit.Begin();
                if (AutoRotationEnabled) UpdateAutoRotationConfiguration();
            }
        }
        #endregion


    }
}
