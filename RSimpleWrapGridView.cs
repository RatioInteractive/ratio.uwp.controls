using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Ratio.UWP.Controls
{
    public sealed class RSimpleWrapGridView : GridView
    {
        private ItemsWrapGrid _itemsWrapGrid;
        private bool _pendingSizeChange;

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(
            "ItemSize", typeof(Size), typeof(RSimpleWrapGridView), new PropertyMetadata(default(Size),ItemSizePropertyChangedCallback));

        private static void ItemSizePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if(!(dependencyObject is RSimpleWrapGridView simpleGridView)) return;
            SetColumn(simpleGridView,(Size)dependencyPropertyChangedEventArgs.NewValue,simpleGridView.ItemPadding,simpleGridView.SingleColumn);
        }

        public Size ItemSize
        {
            get => (Size) GetValue(ItemSizeProperty);
            set => SetValue(ItemSizeProperty, value);
        }

        public static readonly DependencyProperty ItemPaddingProperty = DependencyProperty.Register(
            "ItemPadding", typeof(Thickness), typeof(RSimpleWrapGridView), new PropertyMetadata(default(Thickness),ItemPaddingPropertyChangedCallback));

        private static void ItemPaddingPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if(!(dependencyObject is RSimpleWrapGridView simpleGridView)) return;
            SetColumn(simpleGridView,simpleGridView.ItemSize,(Thickness)dependencyPropertyChangedEventArgs.NewValue,simpleGridView.SingleColumn);
        }

        public Thickness ItemPadding
        {
            get => (Thickness) GetValue(ItemPaddingProperty);
            set => SetValue(ItemPaddingProperty, value);
        }

//        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
//            "ItemWidth", typeof(int), typeof(RSimpleWrapGridView), new PropertyMetadata(default(int), ItemWidthCallback));
//
//        private static void ItemWidthCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
//        {
//            if(!(dependencyObject is RSimpleWrapGridView simpleGridView)) return;
//            if(!(simpleGridView.ItemsPanelRoot is ItemsWrapGrid itemsWrapGrid))
//            {
//                simpleGridView._pendingSizeChange = true;
//                return;
//            }
//            try
//            {
//                var itemWidth = (int)dependencyPropertyChangedEventArgs.NewValue;
//                 itemsWrapGrid.ItemWidth = itemWidth;
//            }
//            catch (Exception e)
//            {
//                Debug.WriteLine(e.Message);
//                throw;
//            }
//            
//        }

//        public int ItemWidth
//        {
//            get => (int) GetValue(ItemWidthProperty);
//            set => SetValue(ItemWidthProperty, value);
//        }
//
//        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
//            "ItemHeight", typeof(int), typeof(RSimpleWrapGridView), new PropertyMetadata(default(int),ItemHeightCallback));
//
//        private static void ItemHeightCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
//        {
//            if (!(dependencyObject is RSimpleWrapGridView simpleGridView)) return;
//            if (!(simpleGridView.ItemsPanelRoot is ItemsWrapGrid itemsWrapGrid))
//            {
//                simpleGridView._pendingSizeChange = true;
//                return;
//            }
//            try
//            {                
//                var itemHeight = (int)dependencyPropertyChangedEventArgs.NewValue;
//                itemsWrapGrid.ItemHeight = itemHeight;
//
//            }
//            catch (Exception e)
//            {
//                Debug.WriteLine(e.Message);
//                throw;
//            }
//
//        }

        private static void SimpleGridViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!(sender is RSimpleWrapGridView simpleGridView)) return;
            simpleGridView._itemsWrapGrid = simpleGridView.ItemsPanelRoot as ItemsWrapGrid;
            if (simpleGridView._itemsWrapGrid == null)
            {
                simpleGridView._pendingSizeChange = true;
                return;
            }
            SetColumn(simpleGridView,simpleGridView.ItemSize,simpleGridView.ItemPadding,simpleGridView.SingleColumn);
        }

//        public int ItemHeight
//        {
//            get => (int) GetValue(ItemHeightProperty);
//            set => SetValue(ItemHeightProperty, value);
//        }

        public static readonly DependencyProperty SingleColumnProperty = DependencyProperty.Register(
            "SingleColumn", typeof(bool), typeof(RSimpleWrapGridView), new PropertyMetadata(default(bool),SingleColumnCallback));

        private static void SingleColumnCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is RSimpleWrapGridView simpleGridView)) return;
            try
            {
                var singleColumn = (bool)dependencyPropertyChangedEventArgs.NewValue;
                SetColumn(simpleGridView,simpleGridView.ItemSize,simpleGridView.ItemPadding, singleColumn);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        public bool SingleColumn
        {
            get => (bool) GetValue(SingleColumnProperty);
            set => SetValue(SingleColumnProperty, value);
        }

        public static readonly DependencyProperty SelectedCommandNameProperty = DependencyProperty.Register(
            "SelectedCommandName", typeof(string), typeof(RSimpleWrapGridView), new PropertyMetadata(default(string)));


        public string SelectedCommandName
        {
            get => (string) GetValue(SelectedCommandNameProperty);
            set => SetValue(SelectedCommandNameProperty, value);
        }
        public RSimpleWrapGridView()
        {
            DefaultStyleKey = typeof(GridView);
            Loaded += SimpleGridViewOnLoaded;
            LayoutUpdated += OnLayoutUpdated;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= SimpleGridViewOnLoaded;
            Unloaded -= OnUnloaded;
            _itemsWrapGrid = null;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new SimpleWrapGridItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is SimpleWrapGridItem simpleWrapGridItem)
            {
                if (!string.IsNullOrEmpty(SelectedCommandName))
                {
                    var binding = new Binding
                    {
                        Path = new PropertyPath(SelectedCommandName),
                        Source = item
                    };
                    simpleWrapGridItem.ItemBinding = binding;
                    simpleWrapGridItem.SetBinding(BaseItem.SelectedCommandProperty, binding);

                    var sizeBinding = new Binding()
                    {
                        Path = new PropertyPath("ItemSize"),
                        Source = this
                    };
                    simpleWrapGridItem.SetBinding(BaseItem.SpecifiedSizeProperty, sizeBinding);
                }
                simpleWrapGridItem.SourceItem = item;

            }
            base.PrepareContainerForItemOverride(element, item);
        }

        private static bool SetColumn(RSimpleWrapGridView simpleGridView, Size itemSize, Thickness itemPadding, bool singleColumn = false)
        {
            if (simpleGridView._itemsWrapGrid == null)
            {
                simpleGridView._itemsWrapGrid = simpleGridView.ItemsPanelRoot as ItemsWrapGrid;
                if (simpleGridView._itemsWrapGrid == null) return false;
            }
            if (singleColumn)
            {
                simpleGridView._itemsWrapGrid.MaximumRowsOrColumns = 1;
//                simpleGridView.ClearValue(ItemWidthProperty);
//                simpleGridView.ClearValue(ItemHeightProperty);
                simpleGridView._itemsWrapGrid.ClearValue(ItemsWrapGrid.ItemWidthProperty);                
                simpleGridView._itemsWrapGrid.ClearValue(ItemsWrapGrid.ItemHeightProperty);
//                Debug.WriteLine("COL: Simple grid: ItemHeight: {0}, ItemWidth: {1}, Panel ItemHeight: {2}, ItemWidth: {3}",simpleGridView.ItemHeight,simpleGridView.ItemWidth,simpleGridView._itemsWrapGrid.ItemHeight,simpleGridView._itemsWrapGrid.ItemWidth);
            }
            else
            {
                simpleGridView._itemsWrapGrid.ClearValue(ItemsWrapGrid.MaximumRowsOrColumnsProperty);
                simpleGridView._itemsWrapGrid.ItemHeight = itemSize.Height + itemPadding.Top + itemPadding.Bottom;
                simpleGridView._itemsWrapGrid.ItemWidth = itemSize.Width + itemPadding.Left + itemPadding.Right;
//                Debug.WriteLine("SIZE: Simple grid: ItemHeight: {0}, ItemWidth: {1}, Panel ItemHeight: {2}, ItemWidth: {3}", simpleGridView.ItemHeight, simpleGridView.ItemWidth, simpleGridView._itemsWrapGrid.ItemHeight, simpleGridView._itemsWrapGrid.ItemWidth);
            }
            return true;
        }

        private void OnLayoutUpdated(object sender, object o)
        {
            if (!_pendingSizeChange) return;
            SetColumn(this,ItemSize,ItemPadding ,SingleColumn);
            _pendingSizeChange = false;
        }
    }
}
