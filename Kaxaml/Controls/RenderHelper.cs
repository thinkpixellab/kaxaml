using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelLab.Contracts;

namespace Kaxaml.Controls
{
    public static class RenderHelper
    {
        #region Static Methods

        public static BitmapSource ElementToBitmap(FrameworkElement e)
        {
            if (e.ActualWidth == 0 || e.ActualHeight == 0) return null;
            return VisualToBitmap(e, (int)e.ActualWidth, (int)e.ActualHeight, null);
        }

        public static BitmapSource ElementToBitmap(FrameworkElement e, GrayscaleParameters parameters)
        {
            if (e.ActualWidth == 0 || e.ActualHeight == 0) return null;
            return VisualToBitmap(e, (int)e.ActualWidth, (int)e.ActualHeight, parameters);
        }

        public static BitmapSource ElementToGrayscaleBitmap(FrameworkElement e)
        {
            if (e.ActualWidth == 0 || e.ActualHeight == 0) return null;
            return VisualToBitmap(e, (int)e.ActualWidth, (int)e.ActualHeight, new GrayscaleParameters());
        }

        public static BitmapSource HwndToBitmap(IntPtr hwnd)
        {
            return HwndToBitmap(hwnd, null);
        }

        public static BitmapSource HwndToBitmap(IntPtr hwnd, GrayscaleParameters parameters)
        {
            IntPtr dc = NativeMethods.GetWindowDC(hwnd);
            IntPtr memDC = NativeMethods.CreateCompatibleDC(dc);
            int l = 0;
            int t = 0;

            Kaxaml.Controls.NativeMethods.RECT rct;
            NativeMethods.GetWindowRect(hwnd, out rct);

            int w = rct.Right - rct.Left;
            int h = rct.Bottom - rct.Top;

            IntPtr hbm = NativeMethods.CreateCompatibleBitmap(dc, w, h);
            IntPtr oldbm = NativeMethods.SelectObject(memDC, hbm);

            bool b = NativeMethods.BitBlt(memDC, l, t, w, h, dc, l, t, 0x00CC0020);
            BitmapSource src = Imaging.CreateBitmapSourceFromHBitmap(hbm, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            NativeMethods.SelectObject(memDC, oldbm);
            NativeMethods.DeleteObject(hbm);
            NativeMethods.DeleteDC(memDC);
            NativeMethods.ReleaseDC(hwnd, dc);

            if (parameters != null)
            {
                int Width = (int)src.Width;
                int Height = (int)src.Height;

                byte[] pixels = new byte[Width * Height * 4];
                src.CopyPixels(pixels, (Width * 4), 0);

                for (int p = 0; p < pixels.Length; p += 4)
                {
                    // compute grayscale
                    double pixelvalue =
                        (((double)pixels[p + 0] * parameters.RedDistribution) +
                        ((double)pixels[p + 1] * parameters.GreenDistribution) +
                        ((double)pixels[p + 2] * parameters.BlueDistribution));

                    // compute compression
                    pixelvalue = (pixelvalue * parameters.Compression) + (256 * ((1 - parameters.Compression) / 2));

                    // compute washout
                    pixelvalue = Math.Min(256, pixelvalue + (256 * parameters.Washout));

                    byte v = (byte)pixelvalue;
                    pixels[p + 0] = v;
                    pixels[p + 1] = v;
                    pixels[p + 2] = v;
                }

                return BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Bgr32, null, pixels, Width * 4);
            }
            else
            {
                return src;
            }
        }

        public static BitmapSource HwndToGrayscaleBitmap(IntPtr hwnd)
        {
            return HwndToBitmap(hwnd, new GrayscaleParameters());
        }

        public static BitmapSource VisualToBitmap(Visual e, int Width, int Height)
        {
            return VisualToBitmap(e, Width, Height, null);
        }

        public static BitmapSource VisualToBitmap(Visual e, int Width, int Height, GrayscaleParameters parameters)
        {
            Contract.Requires(e != null);
            e.VerifyAccess();

            RenderTargetBitmap src = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            src.Render(e);

            if (parameters != null)
            {
                byte[] pixels = new byte[Width * Height * 4];
                src.CopyPixels(pixels, (Width * 4), 0);

                for (int p = 0; p < pixels.Length; p += 4)
                {

                    // compute grayscale
                    double pixelvalue =
                        (((double)pixels[p + 0] * parameters.RedDistribution) +
                        ((double)pixels[p + 1] * parameters.GreenDistribution) +
                        ((double)pixels[p + 2] * parameters.BlueDistribution));

                    // compute compression
                    pixelvalue = (pixelvalue * parameters.Compression) + (256 * ((1 - parameters.Compression) / 2));

                    // compute washout
                    pixelvalue = Math.Min(256, pixelvalue + (256 * parameters.Washout));

                    byte v = (byte)pixelvalue;
                    pixels[p + 0] = v;
                    pixels[p + 1] = v;
                    pixels[p + 2] = v;
                }

                return BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Bgr32, null, pixels, Width * 4);
            }
            else
            {
                return src;
            }
        }

        public static string VisualToFile(Visual e, int Width, int Height, GrayscaleParameters parameters, string Filename)
        {
            BitmapSource src = VisualToBitmap(e, Width, Height, parameters);

            using (System.IO.FileStream fs = new System.IO.FileStream(Filename, System.IO.FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(src));
                encoder.Save(fs);
            }

            return Filename;
        }

        #endregion Static Methods
    }
}
