using System;
using System.Diagnostics;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ratio.UWP.Controls
{
    public sealed class RSimpleWrapGridView : GridView
    {
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth", typeof(int), typeof(RSimpleWrapGridView), new PropertyMetadata(default(int), ItemWidthCallback));

        private static void ItemWidthCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if(!(dependencyObject is RSimpleWrapGridView simpleGridView)) return;
            if(!(simpleGridView.ItemsPanelRoot is ItemsWrapGrid itemsWrapGrid)) return;
            try
            {
                var itemWidth = (int)dependencyPropertyChangedEventArgs.NewValue;
                 itemsWrapGrid.ItemWidth = itemWidth;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
            
        }

        public int ItemWidth
        {
            get => (int) GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight", typeof(int), typeof(RSimpleWrapGridView), new PropertyMetadata(default(int),ItemHeightCallback));

        private static void ItemHeightCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is RSimpleWrapGridView simpleGridView)) return;
            if (!(simpleGridView.ItemsPanelRoot is ItemsWrapGrid itemsWrapGrid)) return;
            try
            {                
                var itemHeight = (int)dependencyPropertyChangedEventArgs.NewValue;
                itemsWrapGrid.ItemHeight = itemHeight;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }

        }

        private static void SimpleGridViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!(sender is RSimpleWrapGridView simpleGridView)) return;
            if (!(simpleGridView.ItemsPanelRoot is ItemsWrapGrid itemsWrapGrid)) return;
            if (simpleGridView.SingleColumn)
            {
                itemsWrapGrid.MaximumRowsOrColumns = 1;
                simpleGridView.ClearValue(ItemWidthProperty);
                simpleGridView.ClearValue(ItemHeightProperty);
            }
            else
            {
                itemsWrapGrid.ClearValue(ItemsWrapGrid.MaximumRowsOrColumnsProperty);
                itemsWrapGrid.ItemHeight = simpleGridView.ItemHeight;
                itemsWrapGrid.ItemWidth = simpleGridView.ItemWidth;
            }
        }

        public int ItemHeight
        {
            get => (int) GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly DependencyProperty SingleColumnProperty = DependencyProperty.Register(
            "SingleColumn", typeof(bool), typeof(RSimpleWrapGridView), new PropertyMetadata(default(bool),SingleColumnCallback));

        private static void SingleColumnCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is RSimpleWrapGridView simpleGridView)) return;
            if (!(simpleGridView.ItemsPanelRoot is ItemsWrapGrid itemsWrapGrid)) return;
            try
            {
                var singleColumn = (bool)dependencyPropertyChangedEventArgs.NewValue;
                if(singleColumn)
                {
                    itemsWrapGrid.MaximumRowsOrColumns = 1;
                    simpleGridView.ClearValue(ItemWidthProperty);
                    simpleGridView.ClearValue(ItemHeightProperty);
                }
                else
                {
                    itemsWrapGrid.ClearValue(ItemsWrapGrid.MaximumRowsOrColumnsProperty);
                }
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
        public RSimpleWrapGridView()
        {
            DefaultStyleKey = typeof(GridView);
            Loaded += SimpleGridViewOnLoaded;
        }
    }
}
