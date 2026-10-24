using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Autodesk.Revit.UI;

namespace SGTools.Dialogs
{
    // The banner shown while a tool waits for a click. It only has to
    // place itself at the top centre of Revit's drawing area.
    public partial class PickBanner : Window
    {
        // Where Revit's drawing area is on the screen, in pixels.
        private readonly Autodesk.Revit.DB.Rectangle drawingArea;

        public PickBanner(string message, UIApplication revit)
        {
            InitializeComponent();
            MessageText.Text = message;
            drawingArea = revit.DrawingAreaExtents;

            // Make Revit the banner's owner, so it stays in front of Revit
            // and disappears with it.
            new WindowInteropHelper(this).Owner = revit.MainWindowHandle;

            // Start off-screen. WindowLoaded moves it into place once its
            // size is known, so it never flashes in the wrong spot.
            Left = -10000;
            Top = -10000;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Revit measures in screen pixels, WPF in its own units. On a
            // screen scaled to 150%, one WPF unit is 1.5 pixels.
            DpiScale scale = VisualTreeHelper.GetDpi(this);
            double areaLeft = drawingArea.Left / scale.DpiScaleX;
            double areaRight = drawingArea.Right / scale.DpiScaleX;
            double areaTop = drawingArea.Top / scale.DpiScaleY;

            Left = (areaLeft + areaRight) / 2 - ActualWidth / 2;
            Top = areaTop;
        }
    }
}