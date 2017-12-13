namespace Ratio.UWP.Controls
{
    public class SelectedItemArgs
    {
        public object Item { get; }
        public object Parameter { get; }

        public SelectedItemArgs(object item, object parameter)
        {
            Item = item;
            Parameter = parameter;
        }
    }
}
