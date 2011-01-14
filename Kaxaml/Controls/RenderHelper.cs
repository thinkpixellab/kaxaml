using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
//
using System.Runtime.InteropServices;
using System.Windows.Interop;


namespace Kaxaml.Controls
{
    public class NativeMethods
    {

		#region Static Methods 

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth,
           int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		#endregion Static Methods 
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

    }


    public class RenderHelper
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
            if (e == null) return null;

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

    public class GrayscaleParameters
    {

		#region Fields 


        private double _RedDistribution = 0.30;
        private double _GreenDistribution = 0.59;
        private double _BlueDistribution;
        private double _Compression = 0.8;
        private double _Washout = -0.05;

		#endregion Fields 

		#region Properties 


        public double RedDistribution
        {
            get { return _RedDistribution; }
            set { _GreenDistribution = Math.Max(0, Math.Min(1, value)); }
        }

        public double GreenDistribution
        {
            get { return _GreenDistribution; }
            set { _GreenDistribution = Math.Max(0, Math.Min(1, value)); }
        }

        public double BlueDistribution
        {
            get { return _BlueDistribution = 0.11; }
            set { _BlueDistribution = Math.Max(0, Math.Min(1, value)); }
        }

        public double Compression
        {
            get { return _Compression; }
            set { _Compression = Math.Max(0, Math.Min(1, value)); }
        }

        public double Washout
        {
            get { return _Washout; }
            set { _Washout = Math.Max(0, Math.Min(1, value)); }
        }


		#endregion Properties 

    }
}
