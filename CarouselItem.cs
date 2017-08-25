using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    public sealed class CarouselItem : GridViewItem
    {
        public object SourceItem { get; set; }
        public CarouselItem()
        {
            TabFocusNavigation = KeyboardNavigationMode.Once;
            DefaultStyleKey = typeof(CarouselItem);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Debug.WriteLine("Carousel Item got focus.");
            base.OnGotFocus(e);
        }
    }
}
