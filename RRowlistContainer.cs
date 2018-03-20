using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Ratio.UWP.Controls
{
    public sealed class RRowlistContainer : ContentPresenter
    {
        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(
            "ItemSize", typeof(Size), typeof(RRowlistContainer), new PropertyMetadata(default(Size)));

        public Size ItemSize
        {
            get => (Size) GetValue(ItemSizeProperty);
            set => SetValue(ItemSizeProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(RRowlistContainer), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ShiftStepsProperty = DependencyProperty.Register(
            "ShiftSteps", typeof(int), typeof(RRowlistContainer), new PropertyMetadata(default(int)));

        public int ShiftSteps
        {
            get => (int) GetValue(ShiftStepsProperty);
            set => SetValue(ShiftStepsProperty, value);
        }

        public static readonly DependencyProperty ScrollButtonSizeProperty = DependencyProperty.Register(
            "ScrollButtonSize", typeof(Size), typeof(RRowlistContainer), new PropertyMetadata(default(Size)));

        public Size ScrollButtonSize
        {
            get => (Size) GetValue(ScrollButtonSizeProperty);
            set => SetValue(ScrollButtonSizeProperty, value);
        }
    }
}
