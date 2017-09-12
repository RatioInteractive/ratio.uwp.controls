using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Ratio.UWP.Controls
{
    public class ControlUtilities
    {

        public static DependencyObject RecursiveFindByType(DependencyObject dependencyObject, Type type, string name = null)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var depObj = VisualTreeHelper.GetChild(dependencyObject, i);
                var frameworkElement = depObj as FrameworkElement;
                if ((depObj.GetType() == type && name == null) || (name != null && name == frameworkElement?.Name)) return depObj;
                var child = RecursiveFindByType(depObj, type, name);
                if (child != null) return child;
            }
            return null;
        }

        public static DependencyObject LookUpTreeByType(DependencyObject dependencyObject, Type type, string name = null)
        {
            while (true)
            {
                
                var depObject = VisualTreeHelper.GetParent(dependencyObject);
                if (depObject != null)
                {
                    var fe = depObject as FrameworkElement;
                    if (fe != null)
                    {
                        if (fe.GetType() == type && (name != null && fe.Name == name)) return depObject;
                    }
                    dependencyObject = depObject;
                    continue;
                }
                return null;
            }
        }

        public static void TraverseAndApply(DependencyObject dependencyObject, Action<DependencyObject> action)
        {
            var count = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                action(child);
                TraverseAndApply(child,action);
            }
        }
    }
}
