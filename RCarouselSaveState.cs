namespace Ratio.UWP.Controls
{
    public class RCarouselSaveState
    {
        public double HorizontalOffset { get; set; }
        public int FocusIndex { get; set; }

        public RCarouselSaveState(RLoopingStackPanelSaveState state)
        {
            HorizontalOffset = state.HorizontalOffset;
            FocusIndex = state.FocusIndex;
        }

        public RCarouselSaveState()
        {
            
        }
    }
}
