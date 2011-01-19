using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KaxamlPlugins;

namespace Kaxaml.Plugins.Default
{
    [Plugin(
        Name = "Snapshot",
        Icon = "Images\\picture.png",
        Description = "Capture and render content as an image (Ctrl+I)",
        ModifierKeys = ModifierKeys.Control,
        Key = Key.I
     )]

    public partial class SnapshotPlugin : System.Windows.Controls.UserControl
    {

		#region Constructors 

        public SnapshotPlugin()
        {
            InitializeComponent();
            KaxamlInfo.ContentLoaded += new KaxamlInfo.ContentLoadedDelegate(KaxamlInfo_ContentLoaded);
        }

		#endregion Constructors 

		#region Event Handlers 

        void KaxamlInfo_ContentLoaded()
        {
            RenderImage.Source = RenderContent();
        }

		#endregion Event Handlers 

		#region Private Methods 

        private void Copy(object sender, RoutedEventArgs e)
        {
            var bitmap = this.RenderContent();
            if (null != bitmap)
            {
                Clipboard.SetImage(bitmap);
            }
        }

        private BitmapSource RenderContent()
        {
            FrameworkElement element = null;

            if (KaxamlInfo.Frame != null && KaxamlInfo.Frame.Content is FrameworkElement)
            {
                element = KaxamlInfo.Frame.Content as FrameworkElement;
            }
            else
            {
                element = KaxamlInfo.Frame;
            }


            if (element != null && element.ActualWidth > 0 && element.ActualHeight > 0)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(element);
                return rtb;
            }
            else
            {
                return null;
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            BitmapSource rtb = RenderContent();
            if (null != rtb)
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.Filter = "Image files (*.png)|*.png|All files (*.*)|*.*";

                if (sfd.ShowDialog(KaxamlInfo.MainWindow) == true)
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                    {
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(rtb));
                        encoder.Save(fs);
                    }
                }
            }
        }

		#endregion Private Methods 

    }
}