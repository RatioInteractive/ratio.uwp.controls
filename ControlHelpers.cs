using Windows.System.Profile;

namespace Ratio.UWP.Controls
{
    public static class ControlHelpers
    {
        public static bool IsXBoxFamily()
        {
            return AnalyticsInfo.VersionInfo.DeviceFamily.ToUpper().Contains("XBOX");
        }
    }
}
