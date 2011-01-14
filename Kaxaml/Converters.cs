using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Kaxaml.Controls
{
    #region RemoveLineBreaksConverter

    public class RemoveLineBreaksConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;

            if (s != null)
            {
                s = s.Replace("\n", "");
                s = s.Replace("\r", "");

                return s;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

    #region GreaterThanConverter

    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double m = 0;

            if (parameter is string)
            {
                m = double.Parse((string)parameter);
            }

            if (typeof(double).IsAssignableFrom(value.GetType()))
            {
                double v = (double)value;
                return v > m;
            }
            else
            {
                try
                {
                    double v = double.Parse(value.ToString());
                    return v > m;
                }
                catch
                {
                    return value;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

    #region AddConverter

    public class AddConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double m = 0;

            if (parameter is string)
            {
                m = double.Parse((string)parameter);
            }

            if (typeof(double).IsAssignableFrom(value.GetType()))
            {
                double v = (double)value;
                return v + m;
            }
            else 
            {
                try
                {
                    double v = double.Parse(value.ToString());
                    return v + m;
                }
                catch 
                {
                    return value;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

    #region MultiplyConverter

    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double m = 1;

            if (parameter is string)
            {
                m = double.Parse((string)parameter);
            }

            if (typeof(double).IsAssignableFrom(value.GetType()))
            {
                double v = (double)value;
                return v * m;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

    #region NotConverter

    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (typeof(bool).IsAssignableFrom(value.GetType()))
            {
                bool b = (bool)value;
                return !b;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

    #region ElementToBitmapConverter

    public class ElementToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FrameworkElement)
            {
                FrameworkElement e = (FrameworkElement)value;
                if (e.ActualHeight == 0 || e.ActualWidth == 0) return null;

                RenderTargetBitmap src = new RenderTargetBitmap((int)e.ActualWidth, (int)e.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                src.Render(e);

                if (ConvertToGrayscale)
                {

                    byte[] pixels = new byte[(int)src.Width * (int)src.Height * 4];
                    src.CopyPixels(pixels, ((int)src.Width * 4), 0);

                    for (int p = 0; p < pixels.Length; p += 4)
                    {
                        double val = (((double)pixels[p + 0] * _RedDistribution) + ((double)pixels[p + 1] * _GreenDistribution) + ((double)pixels[p + 2] * _BlueDistribution));
                        val = (val * Compression) + (256 * ((1 - Compression) / 2));

                        byte v = (byte)val;

                        pixels[p + 0] = v;
                        pixels[p + 1] = v;
                        pixels[p + 2] = v;
                    }

                    return BitmapSource.Create((int)src.Width, (int)src.Height, 96, 96, PixelFormats.Bgr32, null, pixels, (int)src.Width * 4);
                }
                else
                {
                    return src;
                }
            }

            return null;
        }

        private bool _ConvertToGrayscale = false;
        public bool ConvertToGrayscale
        {
            get { return _ConvertToGrayscale; }
            set { _ConvertToGrayscale = value; }
        }

        private double _RedDistribution = 0.30;
        public double RedDistribution
        {
            get { return _RedDistribution; }
            set { _GreenDistribution = Math.Max(0, Math.Min(1, value)); }
        }

        private double _GreenDistribution = 0.59;
        public double GreenDistribution
        {
            get { return _GreenDistribution; }
            set { _GreenDistribution = Math.Max(0, Math.Min(1, value)); }
        }

        private double _BlueDistribution;
        public double BlueDistribution
        {
            get { return _BlueDistribution = 0.11; }
            set { _BlueDistribution = Math.Max(0, Math.Min(1, value)); }
        }

        private double _Compression = 0.8;
        public double Compression
        {
            get { return _Compression; }
            set { _Compression = Math.Max(0, Math.Min(1, value)); }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

    #region GrayscaleBitmapConverter

    public class GrayscaleBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (typeof(BitmapSource).IsAssignableFrom(value.GetType()))
            {
                BitmapSource src = (BitmapSource)value;

                byte[] pixels = new byte[(int)src.Width * (int)src.Height * 4];
                src.CopyPixels(pixels, ((int)src.Width * 4), 0);

                for (int p = 0; p < pixels.Length; p += 4)
                {
                    double val = (((double)pixels[p + 0] * _RedDistribution) + ((double)pixels[p + 1] * _GreenDistribution) + ((double)pixels[p + 2] * _BlueDistribution));
                    val = (val * Compression) + (256 * ((1 - Compression) / 2));

                    byte v = (byte)val;

                    pixels[p + 0] = v;
                    pixels[p + 1] = v;
                    pixels[p + 2] = v;
                }

                return BitmapSource.Create((int)src.Width, (int)src.Height, 96, 96, PixelFormats.Bgr32, null, pixels, (int)src.Width * 4);
            }

            return null;
        }

        private double _RedDistribution = 0.30;
        public double RedDistribution
        {
            get { return _RedDistribution; }
            set { _GreenDistribution = Math.Min(0, Math.Max(1, value)); }
        }

        private double _GreenDistribution = 0.59;
        public double GreenDistribution
        {
            get { return _GreenDistribution; }
            set { _GreenDistribution = Math.Min(0, Math.Max(1, value)); }
        }

        private double _BlueDistribution;
        public double BlueDistribution
        {
            get { return _BlueDistribution = 0.11; }
            set { _BlueDistribution = Math.Min(0, Math.Max(1, value)); }
        }

        private double _Compression = 0.8;
        public double Compression
        {
            get { return _Compression; }
            set { _Compression = Math.Min(0, Math.Max(1, value)); }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }




    #endregion

    #region AppendTextConverter

    public class AppendTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && parameter is string)
            {
                string text = value as string;
                string append = parameter as string;

                return text + append;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

}
