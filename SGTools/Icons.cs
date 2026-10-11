using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace SGTools
{
    // Loads button pictures. The png files in the Resources folder are
    // packed inside the dll when it is built, so there are no loose
    // picture files to install.
    public static class Icons
    {
        // A large ribbon button picture is 32 units wide. Ours are 96
        // pixels wide, three times as many, so they stay sharp on
        // high-resolution screens.
        private const double RibbonIconSize = 32;

        public static ImageSource Load(string name)
        {
            // Revit has a light and a dark theme. Use the matching picture.
            bool dark = UIThemeManager.CurrentTheme == UITheme.Dark;
            string file = name + (dark ? ".dark.png" : ".png");

            Assembly dll = Assembly.GetExecutingAssembly();
            using (Stream stream = dll.GetManifestResourceStream("SGTools.Resources." + file))
            {
                // Read the png as plain pixels.
                BitmapSource picture = new FormatConvertedBitmap(
                    BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad),
                    PixelFormats.Bgra32, null, 0);
                int bytesPerRow = picture.PixelWidth * 4;
                byte[] pixels = new byte[bytesPerRow * picture.PixelHeight];
                picture.CopyPixels(pixels, bytesPerRow, 0);

                // Rebuild it with a higher dots-per-inch value, which tells
                // Revit "these 96 pixels fill 32 units".
                double dpi = 96 * picture.PixelWidth / RibbonIconSize;
                BitmapSource icon = BitmapSource.Create(
                    picture.PixelWidth, picture.PixelHeight, dpi, dpi,
                    PixelFormats.Bgra32, null, pixels, bytesPerRow);
                icon.Freeze();
                return icon;
            }
        }
    }
}
