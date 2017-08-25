namespace Ratio.UWP.Controls
{
    public class RLoopingStackPanelSaveState
    {
        public double HorizontalOffset { get; set; }

        public int FocusIndex { get; set; }

        public RLoopingStackPanelSaveState(RCarouselSaveState state)
        {
            HorizontalOffset = state.HorizontalOffset;
            FocusIndex = state.FocusIndex;
        }

        public RLoopingStackPanelSaveState()
        {
            
        }
    }
}
