using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Ratio.UWP.Controls
{
    public sealed class RItemsControl : ItemsControl
    {
        public static readonly DependencyProperty ShiftStepsProperty = DependencyProperty.Register(
            "ShiftSteps", typeof(int), typeof(RItemsControl), new PropertyMetadata(default(int), ShiftStepsChanged));

        private static void ShiftStepsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is RItemsControl itemsControl)) return;
//            try
//            {
//                var steps = (int) dependencyPropertyChangedEventArgs.NewValue;
//                SetShiftSteps(itemsControl,steps);
//            }
//            catch (InvalidCastException exception)
//            {
//                Debug.WriteLine(exception);
//                throw;
//            }
        }

        private static void SetShiftSteps(ItemsControl itemsControl, int steps)
        {
            if(itemsControl?.Items == null || steps == 0) return;
            for (int i = 0; i < itemsControl.Items?.Count; i++)
            {
                var container = itemsControl.ContainerFromIndex(i);
                if(container == null) continue;
                var rowlist = VisualTreeHelper.GetChild(container, 0) as RRowlist;
                if (rowlist == null) continue;
                rowlist.ShiftSteps = steps;
            }
        }

        public int ShiftSteps
        {
            get => (int) GetValue(ShiftStepsProperty);
            set => SetValue(ShiftStepsProperty, value);
        }
        public static readonly DependencyProperty SubItemTemplateProperty = DependencyProperty.Register(
            "SubItemTemplate", typeof(DataTemplate), typeof(RItemsControl), new PropertyMetadata(default(DataTemplate),SubItemTemplateChanged));

        private static void SubItemTemplateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if(!(dependencyObject is RItemsControl itemsControl)) return;
            if(!(dependencyPropertyChangedEventArgs.NewValue is DataTemplate dataTemplate)) return;
            //SetSubRowlistItemTemplate(itemsControl,dataTemplate);
        }

        private static void SetSubRowlistItemTemplate(RItemsControl itemsControl, DataTemplate dataTemplate)
        {
            if(itemsControl == null || dataTemplate == null || itemsControl.Items == null) return;
            for (int i = 0; i < itemsControl.Items?.Count; i++)
            {
                var container = itemsControl.ContainerFromIndex(i);
                if(container == null) continue;
                var rowlist = VisualTreeHelper.GetChild(container, 0) as RRowlist;
                if (rowlist == null) continue;
                rowlist.ItemTemplate = dataTemplate;
            }

        }

        public DataTemplate SubItemTemplate
        {
            get => (DataTemplate) GetValue(SubItemTemplateProperty);
            set => SetValue(SubItemTemplateProperty, value);
        }
        public static readonly DependencyProperty SubItemSizeProperty = DependencyProperty.Register(
            "SubItemSize", typeof(Size), typeof(RItemsControl), new PropertyMetadata(default(Size),SubItemSizeChanged));

        private static void SubItemSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyPropertyChangedEventArgs.NewValue is Size size)) return;
            if (!(dependencyObject is RItemsControl itemsControl)) return;

//            SetSubRowlistItemsSize(itemsControl, size);         
        }

        private static void SetSubRowlistItemsSize(RItemsControl itemsControl, Size size)
        {
            if (itemsControl.Items == null) return;
            
            for (int i = 0; i < itemsControl.Items?.Count; i++)
            {
                var container = itemsControl.ContainerFromIndex(i);
                if(container == null) continue;
                var rowlist = VisualTreeHelper.GetChild(container, 0) as RRowlist;
                if (rowlist == null) continue;                
                rowlist.ItemHeight = size.Height;
                rowlist.ItemWidth = size.Width;
            }
        }

        public Size SubItemSize
        {
            get => (Size) GetValue(SubItemSizeProperty);
            set => SetValue(SubItemSizeProperty, value);
        }

        public static readonly DependencyProperty ScrollButtonSizeProperty = DependencyProperty.Register(
            "ScrollButtonSize", typeof(Size), typeof(RItemsControl), new PropertyMetadata(default(Size),ScrollButtonSizeChanged));

        private static void ScrollButtonSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyPropertyChangedEventArgs.NewValue is Size size)) return;
            if (!(dependencyObject is RItemsControl itemsControl)) return;
            //SetScrollButtonSize(itemsControl, size);
        }

        private static void SetScrollButtonSize(ItemsControl itemsControl, Size size)
        {
            if (itemsControl.Items == null) return;
            for (int i = 0; i < itemsControl.Items?.Count; i++)
            {
                var container = itemsControl.ContainerFromIndex(i);
                if(container == null) continue;
                var rowlist = VisualTreeHelper.GetChild(container, 0) as RRowlist;
                if (rowlist == null) continue;
                rowlist.ScrollButtonSize = size;
            }
        }

        public Size ScrollButtonSize
        {
            get => (Size) GetValue(ScrollButtonSizeProperty);
            set => SetValue(ScrollButtonSizeProperty, value);
        }

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

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RRowlistContainer();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (element is RRowlistContainer rowlistContainer)
            {
                BindRowlist(rowlistContainer);
            }
        }

        private void BindRowlist(RRowlistContainer rowlistContainer)
        {
            var sizeBinding = new Binding()
            {
                Path = new PropertyPath("SubItemSize"),
                Source = this
            };
            rowlistContainer.SetBinding(RRowlistContainer.ItemSizeProperty, sizeBinding);
            var itemTemplateBinding = new Binding()
            {
                Path = new PropertyPath("SubItemTemplate"),
                Source = this
            };
            rowlistContainer.SetBinding(RRowlistContainer.ItemTemplateProperty, itemTemplateBinding);

            var shiftStepsBinding = new Binding()
            {
                Path = new PropertyPath("ShiftSteps"),
                Source = this
            };
            rowlistContainer.SetBinding(RRowlistContainer.ShiftStepsProperty, shiftStepsBinding);

            var buttonSizeBinding = new Binding()
            {
                Path = new PropertyPath("ScrollButtonSize"),
                Source = this
            };
            rowlistContainer.SetBinding(RRowlistContainer.ScrollButtonSizeProperty, buttonSizeBinding);
        }

        private void OnItemsCompleted()
        {
//            SetSubRowlistItemsSize(this,SubItemSize);
//            SetSubRowlistItemTemplate(this,SubItemTemplate);
//            SetScrollButtonSize(this,ScrollButtonSize);
//            SetShiftSteps(this,ShiftSteps);
            ItemsCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
