// The following code was taken and modified from https://github.com/xyzzer/WinRTXamlToolkit.
// That original code is licensed as follows:
//
// ! The MIT License(MIT) Copyright(c) 2012 Filip Skakun
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of
// the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Ratio.UWP.Controls.Extensions
{
    /// <summary>
    /// Extends the FrameworkElement with attached properties that set the cursor when hovered over by the mouse pointer.
    /// </summary>
    public static class MouseOverCursor
    {
        #region SystemCursor
        public static readonly DependencyProperty SystemCursorProperty = DependencyProperty.RegisterAttached(
            "SystemCursor",
            typeof(CoreCursorType),
            typeof(MouseOverCursor),
            new PropertyMetadata(CoreCursorType.Arrow, OnSystemCursorChanged));

        /// <summary>
        /// Gets the system CoreCursorType to use when a mouse cursor is moved over the framework element.
        /// To specify custom cursors, use Cursor.
        /// </summary>
        public static CoreCursorType GetSystemCursor(DependencyObject d) => (CoreCursorType)d.GetValue(SystemCursorProperty);

        /// <summary>
        /// Sets the system CoreCursorType to use when a mouse cursor is moved over the framework element.
        /// To specify custom cursors, use Cursor.
        /// </summary>
        public static void SetSystemCursor(DependencyObject d, CoreCursorType value) => d.SetValue(SystemCursorProperty, value);

        private static void OnSystemCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newSystemCursor = (CoreCursorType)d.GetValue(SystemCursorProperty);
            SetCursor(d, new CoreCursor(newSystemCursor, 1));
        }

        #endregion

        #region Cursor
        public static readonly DependencyProperty CursorProperty = DependencyProperty.RegisterAttached(
            "Cursor",
            typeof(CoreCursor),
            typeof(MouseOverCursor),
            new PropertyMetadata(null, OnCursorChanged));

        /// <summary>
        /// Gets the cursor to use when a mouse cursor is moved over the framework element.
        /// This property allows custom cursors to be specified.  For system-defined cursors,
        /// SystemCursor can be used instead.
        /// </summary>
        public static CoreCursor GetCursor(DependencyObject d) => (CoreCursor)d.GetValue(CursorProperty);

        /// <summary>
        /// Sets the cursor to use when a mouse cursor is moved over the framework element.
        /// This property allows custom cursors to be specified.  For system-defined cursors,
        /// SystemCursor can be used instead.
        /// </summary>
        public static void SetCursor(DependencyObject d, CoreCursor value) => d.SetValue(CursorProperty, value);

        private static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldCursor = (CoreCursor)e.OldValue;
            var newCursor = (CoreCursor)d.GetValue(CursorProperty);

            if (oldCursor == null)
            {
                var handler = new CursorDisplayHandler();
                handler.Attach((FrameworkElement)d);
                SetCursorDisplayHandler(d, handler);
            }
            else
            {
                var handler = GetCursorDisplayHandler(d);

                if (newCursor == null)
                {
                    handler.Detach();
                    SetCursorDisplayHandler(d, null);
                }
                else
                {
                    handler.UpdateCursor();
                }
            }
        }

        #endregion

        #region CursorDisplayHandler
        public static readonly DependencyProperty CursorDisplayHandlerProperty =
            DependencyProperty.RegisterAttached(
                "CursorDisplayHandler",
                typeof(CursorDisplayHandler),
                typeof(MouseOverCursor),
                new PropertyMetadata(null));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static CursorDisplayHandler GetCursorDisplayHandler(DependencyObject d) =>
            (CursorDisplayHandler)d.GetValue(CursorDisplayHandlerProperty);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetCursorDisplayHandler(DependencyObject d, CursorDisplayHandler value) =>
            d.SetValue(CursorDisplayHandlerProperty, value);

        /// <summary>
        /// Handles the Cursor attached behavior defined by the attached property
        /// of the <see cref="MouseOverCursor"/> class.
        /// </summary>
        public class CursorDisplayHandler
        {
            private FrameworkElement _frameworkElement;
            private bool _isHovering;

            private static CoreCursor _defaultCursor;
            private static CoreCursor DefaultCursor => 
                _defaultCursor ?? (_defaultCursor = Window.Current.CoreWindow.PointerCursor);

            /// <summary>
            /// Attaches to the specified framework element.
            /// </summary>
            public void Attach(FrameworkElement frameworkElement)
            {
                _frameworkElement = frameworkElement;
                _frameworkElement.PointerEntered += OnPointerEntered;
                _frameworkElement.PointerExited += OnPointerExited;
            }

            /// <summary>
            /// Detaches this instance.
            /// </summary>
            public void Detach()
            {
                _frameworkElement.PointerEntered -= OnPointerEntered;
                _frameworkElement.PointerExited -= OnPointerExited;

                if (_isHovering)
                {
                    Window.Current.CoreWindow.PointerCursor = DefaultCursor;
                }
            }

            private void OnPointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
            {
                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                {
                    _isHovering = true;
                    UpdateCursor();
                }
            }

            private void OnPointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
            {
                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
                {
                    _isHovering = false;
                    Window.Current.CoreWindow.PointerCursor = DefaultCursor;
                }
            }

            internal void UpdateCursor()
            {
                if (_defaultCursor == null)
                {
                    _defaultCursor = Window.Current.CoreWindow.PointerCursor;
                }

                var cursor = MouseOverCursor.GetCursor(_frameworkElement);

                if (_isHovering)
                {
                    Window.Current.CoreWindow.PointerCursor = cursor ?? DefaultCursor;
                }
            }
        }

        #endregion

    }
}
