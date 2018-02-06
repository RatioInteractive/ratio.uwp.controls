using Windows.UI.Xaml.Input;

namespace Ratio.UWP.Controls
{
    public sealed class CarouselItem : BaseItem
    {
        public CarouselItem()
        {
            TabFocusNavigation = KeyboardNavigationMode.Once;
            DefaultStyleKey = typeof(CarouselItem);
        }
    }
}
