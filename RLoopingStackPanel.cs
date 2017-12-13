using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

#pragma warning disable 169

namespace Ratio.UWP.Controls
{
    [TemplatePart(Name = "stackPanel", Type = typeof(StackPanel))]
    [TemplatePart(Name = "scrollViewer", Type = typeof(ScrollViewer))]
    public class RLoopingStackPanel : Control
    {
        #region Events
        public event EventHandler DirectManipulationStarted;
        public event EventHandler FocusedItemChanged;
        public event EventHandler ItemsPopulated;
        public event EventHandler ResizingCompleted;
        public event EventHandler ResizingStarted;
        public event EventHandler<ScrollViewer> ScrollViewChanged;
        #endregion

        #region Fields
        private StackPanel _stackPanel;
        private ScrollViewer _scrollViewer;
        private RLoopingStackPanelSaveState _pendingStateChange;
        private int _actualItemWidth;
        private int _actualItemHeight;
        // Flags to disable autoscroll when a [user-initiated] scroll is in progress.
        private bool _scrollInProgress;
        private bool _resizingInProgress;
        private DispatcherTimer _recenterAfterResizingTimer;
        private DispatcherTimer _resizingCompletedTimer;
        #endregion

        #region Properties
        #region Dependency Properties
        public static readonly DependencyProperty FocusedItemProperty = DependencyProperty.Register(
            "FocusedItem", typeof(object), typeof(RLoopingStackPanel), new PropertyMetadata(default(object), FocusedItemPropertyChanged));

        private static void FocusedItemPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var loopingStackPanel = dependencyObject as RLoopingStackPanel;
            loopingStackPanel?.FocusedItemChanged?.Invoke(loopingStackPanel, new EventArgs());
        }

        public object FocusedItem
        {
            get => GetValue(FocusedItemProperty);
            set => SetValue(FocusedItemProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(RLoopingStackPanel), new PropertyMetadata(Orientation.Horizontal));

        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight", typeof(int), typeof(RLoopingStackPanel), new PropertyMetadata(default(int),ItemHeightChanged));

        public int ItemHeight
        {
            get => (int)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth", typeof(double), typeof(RLoopingStackPanel), new PropertyMetadata(default(double), ItemWidthChanged));

        private static void ItemWidthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Debug.WriteLine($"ItemWidthChanged: Old-{dependencyPropertyChangedEventArgs.OldValue} New- {dependencyPropertyChangedEventArgs.NewValue}");
            if ((double)dependencyPropertyChangedEventArgs.NewValue <= 0) return;
            var loopingStackPanel = dependencyObject as RLoopingStackPanel;
            loopingStackPanel?.CalculateActualItemSize();
            loopingStackPanel?.ResizeItems();
            loopingStackPanel?.RecenterItems();
        }

        private static void ItemHeightChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Debug.WriteLine($"ItemHeightChanged: Old-{dependencyPropertyChangedEventArgs.OldValue} New- {dependencyPropertyChangedEventArgs.NewValue}");
            if ((int)dependencyPropertyChangedEventArgs.NewValue == 0) return;
            var loopingStackPanel = dependencyObject as RLoopingStackPanel;
            loopingStackPanel?.CalculateActualItemSize();
            loopingStackPanel?.ResizeItems();
        }

        /// <summary>
        /// The width of each item in the control.  The width may be specified as an absolute value or as relative to
        /// the control's width.  If the value is greater than 1, it is treated as an absolute value.  If it is 1
        /// or less, it is treated as a relative value; it is not possible to set an absolute item width of 1.
        /// Note: When ItemWidth is treated as relative, ItemHeight is ignored and the actual height is calculated
        /// using ItemAspectRatio.
        /// </summary>
        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public int FullItemWidth
        {
            get
            {
                if (_stackPanel.Children.Count <= 0) return 0;

                CarouselItem anyContainer = (CarouselItem)_stackPanel.Children[0];
                return (int)(_actualItemWidth + anyContainer.Margin.Left + anyContainer.Margin.Right);
            }
        }

        public static readonly DependencyProperty ItemAspectRatioProperty = DependencyProperty.Register(
            "ItemAspectRatio", typeof(double), typeof(RLoopingStackPanel), new PropertyMetadata(default(double), ItemAspectRatioChanged));

        private static void ItemAspectRatioChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if ((double)dependencyPropertyChangedEventArgs.NewValue <= 0.0) return;
            var loopingStackPanel = dependencyObject as RLoopingStackPanel;
            loopingStackPanel?.CalculateActualItemSize();
            loopingStackPanel?.ResizeItems();
        }

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
            "ItemTemplate", typeof(DataTemplate), typeof(RLoopingStackPanel), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty CarouselItemContainerStyleProperty = DependencyProperty.Register(
            "CarouselItemContainerStyle", typeof(Style), typeof(RLoopingStackPanel), new PropertyMetadata(default(Style)));

        public Style CarouselItemContainerStyle
        {
            get => (Style) GetValue(CarouselItemContainerStyleProperty);
            set => SetValue(CarouselItemContainerStyleProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IList), typeof(RLoopingStackPanel), new PropertyMetadata(default(IList), ItemsSourceChanged));

        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty SelectedCommandNameProperty = DependencyProperty.Register(
            "SelectedCommandName", typeof(string), typeof(RLoopingStackPanel), new PropertyMetadata(default(string)));

        public string SelectedCommandName
        {
            get => (string) GetValue(SelectedCommandNameProperty);
            set => SetValue(SelectedCommandNameProperty, value);
        }

        public double HorizontalOffset => _scrollViewer?.HorizontalOffset ?? 0;

        private ScrollContentPresenter _scrollContentPresenter;
        private ScrollBar _horizontalScrollbar;

        #endregion

        public int ChildrenCount => _stackPanel?.Children.Count ?? 0;
        #endregion

        public RLoopingStackPanel()
        {
            DefaultStyleKey = typeof(RLoopingStackPanel);            
            Loaded += (sender, args) => LayoutUpdated += OnLayoutUpdated;
        }

        #region Public methods
        private bool _scrollByJumpingToItem;
        public void JumpToItem(object targetItem, [CallerMemberName] string callingMethod = "")
        {
            if (_stackPanel == null) return;
            if (ItemsSource == null) return;
            if (ItemsSource.Contains(targetItem) == false) return;
            if (targetItem == FocusedItem) return;

            // Scroll directly to the target without wrapping around.
            int targetItemIndex = ItemsSource.IndexOf(targetItem);
            int focusedItemIndex = ItemsSource.IndexOf(FocusedItem);
            int directSteps = targetItemIndex - focusedItemIndex;
            _scrollByJumpingToItem = true;
            ScrollBySteps(directSteps);
        }

        private void UpdateFocusedItem(int steps = 1, [CallerMemberName] string callingMethod = "")
        {
            if(ItemsSource == null) throw new ArgumentNullException();
            if (steps == 0)
            {
                FocusedItem = ItemsSource[0];
                Debug.WriteLine($"Focused Item: {FocusedItem} called from {callingMethod}");
                return;
            }
            int focusedItemIndex = ItemsSource.IndexOf(FocusedItem);
            var potentialIndex = focusedItemIndex + steps;
            // Ensure the index is within range.  (Note the '%' operator is a remainder operator, not mod.)
            int Mod(int k, int n) => ((k %= n) < 0 ? k + n : k);
            int nextFocus = Mod(potentialIndex, ItemsSource.Count);
            FocusedItem = ItemsSource[nextFocus];
            Debug.WriteLine($"Focused Item: {FocusedItem} called from {callingMethod}");
        }

        public void PopulateLoopingStackPanel()
        {
            PopulateStackPanel(ItemsSource);
        }

        public void AutoScrollBySteps(int steps, [CallerMemberName] string callingMethod = "")
        {
            if (_scrollInProgress)
            {
                Debug.WriteLine($"Scroll request ignored because scrolling is already in progress, called from {callingMethod}");
                return;
            }
            if (_resizingInProgress)
            {
                Debug.WriteLine($"Scroll request ignored because resizing is in progress, called from {callingMethod}");
                return;
            }
            ScrollBySteps(steps);
        }

        public void ScrollBySteps(int steps, [CallerMemberName] string callingMethod = "")
        {
            if (_scrollViewer == null) return;
            if (steps == 0) return;
            if(ItemsSource == null) return;
            // Ensure steps is in the range of -maxSteps to +maxStes
            int maxSteps = ItemsSource.Count - 1;
            steps = Math.Max(-maxSteps, Math.Min(maxSteps, steps));

            // With 3 sets of children in the stackpanel, assuming the stackpanel is balanced, the target item is
            // guaranteed to be found to either side of the centered item.  With that, just scroll there.
            _scrollInProgress = true;
            _lastOffset = _scrollViewer.HorizontalOffset + (FullItemWidth * steps);
            _scrollViewer.ChangeView(_lastOffset, 0, 1, false);
        }

        public RLoopingStackPanelSaveState SaveState()
        {
            return new RLoopingStackPanelSaveState(){HorizontalOffset = _scrollViewer.HorizontalOffset, FocusIndex = ItemsSource.IndexOf(FocusedItem)};
        }

        public void RestoreState(RLoopingStackPanelSaveState state)
        {
            if (state != null)
            {
                if (_scrollViewer != null)
                {
                    RestorePreviousVisualState(state);
                }
                else
                {
                    _pendingStateChange = state;
                }
            }
        }

        public Thickness GetItemContainerMargin()
        {
            return _stackPanel?.Children.Count > 0
                ? ((CarouselItem)_stackPanel?.Children[0]).Margin
                : new Thickness();
        }
        #endregion

        #region Event Handlers

        private static void ItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var carouselList = dependencyPropertyChangedEventArgs.NewValue as IList;
            if (carouselList == null) return;
            var loopingStackPanel = dependencyObject as RLoopingStackPanel;
            if(carouselList.Count > 0)
                loopingStackPanel?.PopulateStackPanel(carouselList);
            var notifyCollection = dependencyPropertyChangedEventArgs.NewValue as INotifyCollectionChanged;
            if(notifyCollection == null || loopingStackPanel == null) return;
            notifyCollection.CollectionChanged += loopingStackPanel.NotifyCollectionOnCollectionChanged;

        }

        private void NotifyCollectionOnCollectionChanged(object o, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add || notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                PopulateStackPanel(ItemsSource);
            }
        }

        private bool _loadInitiatedScroll;
        private void OnLayoutUpdated(object sender, object o)
        {
            if (_scrollViewer != null && _stackPanel != null && ChildrenCount > 0)
            {
                if (_pendingStateChange != null)
                {
                    RestorePreviousVisualState(_pendingStateChange);
                    _pendingStateChange = null;
                }
                else
                {
                    SetupControlState();
                }
                LayoutUpdated -= OnLayoutUpdated;
            }
        }

        private void _scrollViewer_DirectManipulationStarted(object o, object o1)
        {
            _scrollInProgress = true;
            DirectManipulationStarted?.Invoke(this, new EventArgs());
        }

        private double _lastOffset;
        private void _scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_scrollViewer == null) return;

            if (_scrollByJumpingToItem == false && _scrollToNegateCollectionWrapMove == false)
            {
                CheckCenterItem();
            }

            if (e.IsIntermediate) return;
            Debug.WriteLine("Last Offset: {0}, Current Offset: {1}", _lastOffset, _scrollViewer.HorizontalOffset);
            if (_loadInitiatedScroll)
            {
                _loadInitiatedScroll = false;
                _lastOffset = _scrollViewer.HorizontalOffset;
                _scrollInProgress = false;
                return;
            }

            if (_scrollToNegateCollectionWrapMove)
            {
                _scrollToNegateCollectionWrapMove = false;
                _scrollInProgress = false;
                return;
            }

            if (_scrollByJumpingToItem)
            {
                // After we've avoided focused item changes during the jump, we now check the center item.
                _scrollByJumpingToItem = false;
                CheckCenterItem();
            }
            if (RebalanceScrollView() == false)
            {
                // Rebalancing was not needed so scrolling has completed.
                _scrollInProgress = false;
            }
            OnScrollViewChanged(_scrollViewer);
        }

        private void RLoopingStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Only respond to width changes because actions below will trigger a height change that should be ignored.
            if (e.PreviousSize.Width == e.NewSize.Width) return;

            double lastActualItemWidth = _actualItemWidth;
            CalculateActualItemSize();

            // If not resizing due to initialization.
            if (lastActualItemWidth > 0.0)
            {
                if (_resizingInProgress == false)
                {
                    _resizingInProgress = true;
                    ResizingStarted?.Invoke(this, new EventArgs());
                }

                // This will only be true if ItemWidth is relative (not fixed).
                if (_actualItemWidth != lastActualItemWidth)
                {
                    ResizeItems();
                }

                // Re-center the focus item when resizing has settled.
                // The settling duration should be long enough to avoid jitter while the user is dragging the window
                // border, but as short as possible so the items snap back into proper position when resizing is done/paused.
                if (_recenterAfterResizingTimer == null)
                {
                    _recenterAfterResizingTimer = new DispatcherTimer();
                    _recenterAfterResizingTimer.Interval = TimeSpan.FromMilliseconds(100);
                    _recenterAfterResizingTimer.Tick += _recenterAfterResizingTimer_Tick; ;
                }
                if (_recenterAfterResizingTimer.IsEnabled) _recenterAfterResizingTimer.Stop();
                _recenterAfterResizingTimer.Start();

                // Reset the resizing completed "watchdog" timer.
                if (_resizingCompletedTimer == null)
                {
                    _resizingCompletedTimer = new DispatcherTimer();
                    _resizingCompletedTimer.Interval = TimeSpan.FromMilliseconds(250);
                    _resizingCompletedTimer.Tick += _resizingCompletedTimer_Tick;
                }
                if (_resizingCompletedTimer.IsEnabled) _resizingCompletedTimer.Stop();
                _resizingCompletedTimer.Start();
            }
        }

        private async void _recenterAfterResizingTimer_Tick(object sender, object e)
        {
            _recenterAfterResizingTimer.Stop();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => RecenterItems());
        }

        private async void _resizingCompletedTimer_Tick(object sender, object e)
        {
            _resizingCompletedTimer.Stop();
            _resizingInProgress = false;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => ResizingCompleted?.Invoke(this, new EventArgs()));
        }

        #endregion

        #region Overrides
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            CalculateActualItemSize();
            _stackPanel = GetTemplateChild("stackPanel") as StackPanel;
            _scrollViewer = GetTemplateChild("scrollViewer") as ScrollViewer;
            if (_stackPanel != null)
            {
                _stackPanel.AreScrollSnapPointsRegular = true;
                if (ItemsSource != null && ItemsSource.Count > 0)
                {
                    PopulateStackPanel(ItemsSource);
                }
            }
            if (_scrollViewer != null)
            {
                _scrollViewer.HorizontalSnapPointsAlignment = SnapPointsAlignment.Center;
                _scrollViewer.HorizontalSnapPointsType = SnapPointsType.Mandatory;
                _scrollViewer.Loaded += (sender, args) => WireUp();
                _scrollViewer.Unloaded += (sender, args) => Unwire();
            }
            SizeChanged += RLoopingStackPanel_SizeChanged;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Debug.WriteLine("Focus obtained by RLooping StackPanel");
            base.OnGotFocus(e);
        }
        #endregion

        #region Supporting methods
        private void SetupControlState()
        {
            Debug.Assert(_stackPanel.Children.Count == ItemsSource.Count * 3);
            UpdateFocusedItem(0);
            _loadInitiatedScroll = true;
            int centerItemIndex = ItemsSource.Count;
            _lastOffset = FullItemWidth * centerItemIndex - (int)((ActualWidth - FullItemWidth) / 2);
            _scrollViewer.ChangeView(_lastOffset, 0, 1, true);
        }

        private void RestorePreviousVisualState(RLoopingStackPanelSaveState state)
        {
            if (state == null) return;
            UpdateFocusedItem(state.FocusIndex);
            _loadInitiatedScroll = true;
            int centerItemIndex = ItemsSource.Count + state.FocusIndex;
            _lastOffset = FullItemWidth * centerItemIndex - (int)((ActualWidth - FullItemWidth) / 2);
            _scrollViewer.ChangeView(_lastOffset, 0, 1, true);
        }

        private void Unwire()
        {
            if(_scrollViewer != null) _scrollViewer.ViewChanged -= _scrollViewer_ViewChanged;
            if (_scrollViewer != null) _scrollViewer.DirectManipulationStarted -= _scrollViewer_DirectManipulationStarted;
        }

        private void WireUp()
        {
            if (_scrollViewer != null) _scrollViewer.ViewChanged += _scrollViewer_ViewChanged;
            if (_scrollViewer != null) _scrollViewer.DirectManipulationStarted += _scrollViewer_DirectManipulationStarted;
        }

        /// <summary>
        /// Takes the source collection and wraps it in the container and adds it to the control, taking in to account
        /// how many items should appear left of the focus element.
        /// </summary>
        /// <param name="sourceCollection"></param>
        private void PopulateStackPanel(IList sourceCollection)
        {
            if(_stackPanel == null) return;
            _stackPanel.Children.Clear();
            for (int i = 0; i < 3; i++)
            {
                foreach (var item in sourceCollection)
                {
                    AddItem(item);
                }
            }
            CalculateActualItemSize();
            ResizeItems();
            RecenterItems();
            ItemsPopulated?.Invoke(this, new EventArgs());
        }

        private void AddItem(object item)
        {
            if(_stackPanel == null) return;
            var containerItem = CreateItemContainer();
            PrepareContainerForItem(ref containerItem, item);
            _stackPanel.Children.Add(containerItem);
        }

        private void WrapItems(int steps)
        {
            var absSteps = Math.Abs(steps);
            if (steps > 0)
            {
                for (var i = 0; i < absSteps; i++)
                {
                    ShiftTowardsBeginning();
                }
            }
            else
            {
                for (var i = 0; i < absSteps; i++)
                {
                    ShiftTowardsEnd();
                }
            }
            ShiftScrollViewToNegateCollectionWrapMove(steps);
        }

        private void ShiftTowardsEnd()
        {
            if (_stackPanel == null) return;
            if (_stackPanel.Children.Count <= 0) return;
            var last = _stackPanel.Children.Last();
            _stackPanel.Children.Remove(last);
            _stackPanel.Children.Insert(0, last);
        }

        private void ShiftTowardsBeginning()
        {
            if (_stackPanel == null) return;
            if (_stackPanel.Children.Count <= 0) return;
            var first = _stackPanel.Children.First();
            _stackPanel.Children.Remove(first);
            _stackPanel.Children.Add(first);
        }

        /// <returns>true if rebalancing is needed, false otherwise.</returns>
        private bool RebalanceScrollView()
        {
            if(_scrollViewer == null) return false;
            int offsetForBalancedScrollView = (int) Math.Round((_scrollViewer.ExtentWidth - _scrollViewer.ActualWidth) / 2);
            var delta = offsetForBalancedScrollView - _scrollViewer.HorizontalOffset;
            if (!(Math.Abs(delta) > FullItemWidth)) return false;
            var steps = (int) Math.Round((decimal) (delta / FullItemWidth), 0, MidpointRounding.AwayFromZero);
            //invert to shift in the opposite direction of the scroll
            var invertedStep = steps * -1;
            if (steps != 0)
            {
                WrapItems(invertedStep);
                return true;
            }
            return false;
        }

        private void CalculateActualItemSize()
        {
//            if (ActualWidth <= 0)
//            {
//                // Either template has not been applied yet or control really occupies no width.
//                _actualItemWidth = 0;
//                _actualItemHeight = 0;
//            }
            if (ItemWidth > 1.0f)
            {
                _actualItemWidth = (int) Math.Round(ItemWidth);
                _actualItemHeight = ItemHeight;
            }
            else if(ActualWidth > 0)
            {
                // ItemWidth is to be treated as a relative value to the control's width.
                _actualItemWidth = (int) Math.Round(ActualWidth * ItemWidth);
                _actualItemHeight = ItemAspectRatio > 0.0 ? (int) Math.Round(_actualItemWidth / ItemAspectRatio) : 0;
            }

        }

        private void ResizeItems()
        {
            if (_stackPanel == null) return;
            foreach (var child in _stackPanel.Children)
            {
                if (child is CarouselItem carouselItem)
                {
                    carouselItem.Width = _actualItemWidth;
                    carouselItem.Height = _actualItemHeight;
                }
            }
        }

        private void RecenterItems()
        {
            if (_stackPanel == null) return;
            if (_stackPanel.Children.Count == 0) return;
            IEnumerable<CarouselItem> focusedItemContainers = _stackPanel.Children
                .Select(e => (CarouselItem)e)
                .Where(e => e.Content == FocusedItem);
            if (!focusedItemContainers.Any()) return;
            // focusedItemContainers.Count() should be 3 with items added three times.  Pick the balanced, middle one.
            int middleIndex = (int)(focusedItemContainers.Count() / 2.0);
            CarouselItem centerItem = focusedItemContainers.ElementAt(middleIndex);
            int centerItemIndex = _stackPanel.Children.IndexOf(centerItem);
            _lastOffset = FullItemWidth * centerItemIndex - (int)((ActualWidth - FullItemWidth) / 2);
            _scrollViewer.ChangeView(_lastOffset, 0, 1, true);
        }

        protected virtual void OnScrollViewChanged(ScrollViewer e)
        {
            ScrollViewChanged?.Invoke(this, e);
        }

        /// <summary>
        /// See if the item in the horizontal center has changed and notify any subscribed event handlers.
        /// Note: Try to keep this method light-weight since this is called on each view change of the scroll viewer.
        /// </summary>
        private void CheckCenterItem()
        {
            if (FullItemWidth > 0.0)
            {
                int centerItemStackPanelChildIndex = (int)((_scrollViewer.HorizontalOffset + _scrollViewer.ActualWidth / 2) / FullItemWidth);
                if (_stackPanel.Children.Count > centerItemStackPanelChildIndex)
                {
                    var centerItem = ((CarouselItem)_stackPanel.Children[centerItemStackPanelChildIndex]).Content;
                    if (FocusedItem != centerItem)
                    {
                        FocusedItem = centerItem;
                    }
                }
            }
        }
        #endregion

        #region Virtuals
        protected virtual UIElement CreateItemContainer()
        {
            return new CarouselItem();
        }

        protected virtual void PrepareContainerForItem(ref UIElement element, object item)
        {
            if (!(element is CarouselItem carouselItem)) return;
            carouselItem.Height = _actualItemHeight;
            carouselItem.Width = _actualItemWidth;
            if (ItemTemplate != null)
            {
                carouselItem.ContentTemplate = ItemTemplate;
            }
            if (CarouselItemContainerStyle != null && CarouselItemContainerStyle.TargetType == typeof(CarouselItem))
            {
                
                carouselItem.Style = CarouselItemContainerStyle;
            }
            if (!string.IsNullOrEmpty(SelectedCommandName))
            {
                var binding = new Binding
                {
                    Path = new PropertyPath(SelectedCommandName),
                    Source = item
                };
                carouselItem.SetBinding(BaseItem.SelectedCommandProperty, binding);
            }
            carouselItem.Content = item;
            carouselItem.SourceItem = item;
        }

        private bool _scrollToNegateCollectionWrapMove;
        /// <summary>
        /// Use negative steps to move backwards
        /// </summary>
        /// <param name="steps"></param>
        private void ShiftScrollViewToNegateCollectionWrapMove(int steps = 1)
        {
            //Invert steps to scroll in the opposite direction of steps taken to cancel out wrap.
            _scrollToNegateCollectionWrapMove = true;
            var invertedSteps = -1 * steps;
            _lastOffset = _scrollViewer.HorizontalOffset + FullItemWidth * invertedSteps;
            _scrollViewer.ChangeView(_lastOffset, 0, 1, true);
        }
        #endregion
    }
}
