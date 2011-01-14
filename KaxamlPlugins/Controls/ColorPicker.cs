using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Kaxaml.Plugins.Controls
{
    public class DropDownColorPicker : ColorPicker
    {
        static DropDownColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownColorPicker), new FrameworkPropertyMetadata(typeof(DropDownColorPicker)));
        }
    }


    public class ColorPicker : Control
    {
        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        /// <summary>
        /// ColorBrush Property
        /// </summary>

        public SolidColorBrush ColorBrush
        {
            get { return (SolidColorBrush)GetValue(ColorBrushProperty); }
            set { SetValue(ColorBrushProperty, value); }
        }
        public static readonly DependencyProperty ColorBrushProperty =
            DependencyProperty.Register("ColorBrush", typeof(SolidColorBrush), typeof(ColorPicker), new UIPropertyMetadata(Brushes.Black));



        /// <summary>
        /// Color Property
        /// </summary>

        bool HSBSetInternally = false;
        bool RGBSetInternally = false;

        public static void OnColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker c = (ColorPicker)o;


            if (e.NewValue is Color)
            {
                Color color = (Color)e.NewValue;

                if (!c.HSBSetInternally)
                {
                    // update HSB value based on new value of color

                    double H = 0;
                    double S = 0;
                    double B = 0;
                    ColorPickerUtil.HSBFromColor(color, ref H, ref S, ref B);

                    c.HSBSetInternally = true;

                    c.Alpha = (double)(color.A / 255);
                    c.Hue = H;
                    c.Saturation = S;
                    c.Brightness = B;

                    c.HSBSetInternally = false;
                }

                if (!c.RGBSetInternally)
                {
                    // update RGB value based on new value of color

                    c.RGBSetInternally = true;

                    c.A = color.A;
                    c.R = color.R;
                    c.G = color.G;
                    c.B = color.B;

                    c.RGBSetInternally = false;
                }

                c.RaiseColorChangedEvent((Color) e.NewValue);

            }
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set
            {
                SetValue(ColorProperty, value);

            }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorPicker), new UIPropertyMetadata(Colors.Black, OnColorChanged));

        /// <summary>
        /// Hue Property
        /// </summary>

        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof(double), typeof(ColorPicker),
            new FrameworkPropertyMetadata(0.0,
            new PropertyChangedCallback(UpdateColorHSB),
            new CoerceValueCallback(HueCoerce)));

        public static object HueCoerce(DependencyObject d, object Hue)
        {
            double v = (double)Hue;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }

        /// <summary>
        /// Brightness Property
        /// </summary>

        public double Brightness
        {
            get { return (double)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double), typeof(ColorPicker),
            new FrameworkPropertyMetadata(0.0,
            new PropertyChangedCallback(UpdateColorHSB),
            new CoerceValueCallback(BrightnessCoerce)));

        public static object BrightnessCoerce(DependencyObject d, object Brightness)
        {
            double v = (double)Brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }

        /// <summary>
        /// Saturation Property
        /// </summary>

        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register("Saturation", typeof(double), typeof(ColorPicker),
            new FrameworkPropertyMetadata(0.0,
            new PropertyChangedCallback(UpdateColorHSB),
            new CoerceValueCallback(SaturationCoerce)));

        public static object SaturationCoerce(DependencyObject d, object Saturation)
        {
            double v = (double)Saturation;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }

        /// <summary>
        /// Alpha Property
        /// </summary>

        public double Alpha
        {
            get { return (double)GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
        }

        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register("Alpha", typeof(double), typeof(ColorPicker),
            new FrameworkPropertyMetadata(1.0,
            new PropertyChangedCallback(UpdateColorHSB),
            new CoerceValueCallback(AlphaCoerce)));

        public static object AlphaCoerce(DependencyObject d, object Alpha)
        {
            double v = (double)Alpha;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }


        /// <summary>
        /// Shared property changed callback to update the Color property
        /// </summary>

        public static void UpdateColorHSB(object o, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker c = (ColorPicker)o;
            Color n = ColorPickerUtil.ColorFromAHSB(c.Alpha, c.Hue, c.Saturation, c.Brightness);

            c.HSBSetInternally = true;

            c.Color = n;
            c.ColorBrush = new SolidColorBrush(n);

            c.HSBSetInternally = false;
        }

        /// <summary>
        /// R Property
        /// </summary>

        public int R
        {
            get { return (int)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }

        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register("R", typeof(int), typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(int),
            new PropertyChangedCallback(UpdateColorRGB),
            new CoerceValueCallback(RGBCoerce)));


        /// <summary>
        /// G Property
        /// </summary>

        public int G
        {
            get { return (int)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }

        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register("G", typeof(int), typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(int),
            new PropertyChangedCallback(UpdateColorRGB),
            new CoerceValueCallback(RGBCoerce)));

        /// <summary>
        /// B Property
        /// </summary>

        public int B
        {
            get { return (int)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }

        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register("B", typeof(int), typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(int),
            new PropertyChangedCallback(UpdateColorRGB),
            new CoerceValueCallback(RGBCoerce)));


        /// <summary>
        /// A Property
        /// </summary>

        public int A
        {
            get { return (int)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        public static readonly DependencyProperty AProperty =
            DependencyProperty.Register("A", typeof(int), typeof(ColorPicker),
            new FrameworkPropertyMetadata(255,
            new PropertyChangedCallback(UpdateColorRGB),
            new CoerceValueCallback(RGBCoerce)));



        public static object RGBCoerce(DependencyObject d, object value)
        {
            int v = (int)value;
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }

        /// <summary>
        /// Shared property changed callback to update the Color property
        /// </summary>

        public static void UpdateColorRGB(object o, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker c = (ColorPicker)o;
            Color n = Color.FromArgb((byte)c.A, (byte)c.R, (byte)c.G, (byte)c.B);

            c.RGBSetInternally = true;
            
            c.Color = n;
            c.ColorBrush = new SolidColorBrush(n);

            c.RGBSetInternally = false;
        }

        #region ColorChanged Event

        public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);

        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Bubble, typeof(ColorChangedEventHandler), typeof(ColorPicker));

        public event ColorChangedEventHandler ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        void RaiseColorChangedEvent(Color color)
        {
            ColorChangedEventArgs newEventArgs = new ColorChangedEventArgs(ColorPicker.ColorChangedEvent, color);
            RaiseEvent(newEventArgs);
        }

        #endregion

    }

    public class ColorChangedEventArgs : RoutedEventArgs
    {
        public ColorChangedEventArgs(RoutedEvent routedEvent, Color color)
        {
            this.RoutedEvent = routedEvent;
            this.Color = color;
        }

        private Color _Color;
        public Color Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
    }


    public class ColorTextBox : TextBox
    {

        #region Properties

        /// <summary>
        /// ColorBrush Property
        /// </summary>

        public SolidColorBrush ColorBrush
        {
            get { return (SolidColorBrush)GetValue(ColorBrushProperty); }
            set { SetValue(ColorBrushProperty, value); }
        }
        public static readonly DependencyProperty ColorBrushProperty =
            DependencyProperty.Register("ColorBrush", typeof(SolidColorBrush), typeof(ColorTextBox), new UIPropertyMetadata(Brushes.Black));

        /// <summary>
        /// Color Property
        /// </summary>
        bool ColorSetInternally = false;

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set
            {
                SetValue(ColorProperty, value);

                if (!ColorSetInternally)
                {
                    SetValue(TextProperty, value.ToString());
                }
            }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorTextBox), new UIPropertyMetadata(Colors.Black));

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Updates the Color property any time the text changes
        /// </summary>

        protected override void OnTextChanged(System.Windows.Controls.TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            ColorSetInternally = true;
            Color = ColorPickerUtil.ColorFromString(this.Text);
            ColorBrush = new SolidColorBrush(Color);
            ColorSetInternally = false;
        }

        /// <summary>
        /// Restricts input to chacters that are valid for defining a color
        /// </summary>

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0)
            {
                char c = e.Text[0];

                bool IsValid = false;

                if (c >= 'a' && c <= 'f') IsValid = true;
                if (c >= 'A' && c <= 'F') IsValid = true;
                if (c >= '0' && c <= '9' && Keyboard.Modifiers != ModifierKeys.Shift) IsValid = true;

                if (!IsValid)
                {
                    e.Handled = true;
                }

                if (this.Text.Length >= 8)
                {
                    e.Handled = true;
                }
            }

            base.OnPreviewTextInput(e);
        }

        #endregion

    }

    public class HueChooser : FrameworkElement
    {
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this);

                if (Orientation == Orientation.Vertical)
                {
                    Hue = 1 - (p.Y / this.ActualHeight);
                }
                else
                {
                    Hue = 1 - (p.X / this.ActualWidth);
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this);

                if (Orientation == Orientation.Vertical)
                {
                    Hue = 1 - (p.Y / this.ActualHeight);
                }
                else
                {
                    Hue = 1 - (p.X / this.ActualWidth);
                }
            }

            Mouse.Capture(this);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            base.OnMouseUp(e);
        }


        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(HueChooser), new UIPropertyMetadata(Orientation.Vertical));


        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof(double), typeof(HueChooser),
            new FrameworkPropertyMetadata(0.0,
            new PropertyChangedCallback(HueChanged),
            new CoerceValueCallback(HueCoerce)));

        public static void HueChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            HueChooser h = (HueChooser)o;
            h.UpdateHueOffset();
            h.UpdateColor();
        }

        public static object HueCoerce(DependencyObject d, object Brightness)
        {
            double v = (double)Brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }

        public double HueOffset
        {
            get { return (double)GetValue(HueOffsetProperty); }
            private set { SetValue(HueOffsetProperty, value); }
        }
        public static readonly DependencyProperty HueOffsetProperty =
            DependencyProperty.Register("HueOffset", typeof(double), typeof(HueChooser), new UIPropertyMetadata(0.0));

        private void UpdateHueOffset()
        {
            double length = ActualHeight;
            if (Orientation == Orientation.Horizontal) length = ActualWidth;

            HueOffset = length - (length * Hue);
        }

        private void UpdateColor()
        {
            Color = ColorPickerUtil.ColorFromHSB(Hue, 1, 1);
            ColorBrush = new SolidColorBrush(Color);
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            private set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(HueChooser), new UIPropertyMetadata(Colors.Red));

        public SolidColorBrush ColorBrush
        {
            get { return (SolidColorBrush)GetValue(ColorBrushProperty); }
            private set { SetValue(ColorBrushProperty, value); }
        }
        public static readonly DependencyProperty ColorBrushProperty =
            DependencyProperty.Register("ColorBrush", typeof(SolidColorBrush), typeof(HueChooser), new UIPropertyMetadata(Brushes.Red));

        protected override void OnRender(DrawingContext dc)
        {
            LinearGradientBrush lb = new LinearGradientBrush();

            lb.StartPoint = new Point(0, 0);

            if (Orientation == Orientation.Vertical)
                lb.EndPoint = new Point(0, 1);
            else
                lb.EndPoint = new Point(1, 0);

            lb.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0x00, 0x00), 1.00));
            lb.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0xFF, 0x00), 0.85));
            lb.GradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0xFF, 0x00), 0.76));
            lb.GradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0xFF, 0xFF), 0.50));
            lb.GradientStops.Add(new GradientStop(Color.FromRgb(0x00, 0x00, 0xFF), 0.33));
            lb.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0x00, 0xFF), 0.16));
            lb.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0x00, 0x00), 0.00));

            dc.DrawRectangle(lb, null, new Rect(0, 0, ActualWidth, ActualHeight));

            UpdateHueOffset();

        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateHueOffset();
            return base.ArrangeOverride(finalSize);
        }
    }

    public class AlphaChooser : FrameworkElement
    {
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this);

                if (Orientation == Orientation.Vertical)
                {
                    Alpha = 1 - (p.Y / this.ActualHeight);
                }
                else
                {
                    Alpha = 1 - (p.X / this.ActualWidth);
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this);

                if (Orientation == Orientation.Vertical)
                {
                    Alpha = 1 - (p.Y / this.ActualHeight);
                }
                else
                {
                    Alpha = 1 - (p.X / this.ActualWidth);
                }
            }

            Mouse.Capture(this);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            base.OnMouseUp(e);
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AlphaChooser), new UIPropertyMetadata(Orientation.Vertical));


        public double Alpha
        {
            get { return (double)GetValue(AlphaProperty); }
            set { SetValue(AlphaProperty, value); }
        }

        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register("Alpha", typeof(double), typeof(AlphaChooser),
            new FrameworkPropertyMetadata(1.0,
            new PropertyChangedCallback(AlphaChanged),
            new CoerceValueCallback(AlphaCoerce)));

        public static void AlphaChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            AlphaChooser h = (AlphaChooser)o;
            h.UpdateAlphaOffset();
            h.UpdateColor();
        }

        public static object AlphaCoerce(DependencyObject d, object Brightness)
        {
            double v = (double)Brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }

        public double AlphaOffset
        {
            get { return (double)GetValue(AlphaOffsetProperty); }
            private set { SetValue(AlphaOffsetProperty, value); }
        }
        public static readonly DependencyProperty AlphaOffsetProperty =
            DependencyProperty.Register("AlphaOffset", typeof(double), typeof(AlphaChooser), new UIPropertyMetadata(0.0));

        private void UpdateAlphaOffset()
        {
            double length = ActualHeight;
            if (Orientation == Orientation.Horizontal) length = ActualWidth;

            AlphaOffset = length - (length * Alpha);
        }

        private void UpdateColor()
        {
            Color = Color.FromArgb((byte)Math.Round(Alpha * 255), 0, 0, 0);
            ColorBrush = new SolidColorBrush(Color);
        }

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            private set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(AlphaChooser), new UIPropertyMetadata(Colors.Red));

        public SolidColorBrush ColorBrush
        {
            get { return (SolidColorBrush)GetValue(ColorBrushProperty); }
            private set { SetValue(ColorBrushProperty, value); }
        }
        public static readonly DependencyProperty ColorBrushProperty =
            DependencyProperty.Register("ColorBrush", typeof(SolidColorBrush), typeof(AlphaChooser), new UIPropertyMetadata(Brushes.Red));

        protected override void OnRender(DrawingContext dc)
        {
            LinearGradientBrush lb = new LinearGradientBrush();

            lb.StartPoint = new Point(0, 0);

            if (Orientation == Orientation.Vertical)
                lb.EndPoint = new Point(0, 1);
            else
                lb.EndPoint = new Point(1, 0);

            lb.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x00, 0x00), 0.00));
            lb.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0x00, 0x00, 0x00), 1.00));

            dc.DrawRectangle(lb, null, new Rect(0, 0, ActualWidth, ActualHeight));

            UpdateAlphaOffset();

        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateAlphaOffset();
            return base.ArrangeOverride(finalSize);
        }

    }

    public class SaturationBrightnessChooser : FrameworkElement
    {
        public Thickness OffsetPadding
        {
            get { return (Thickness)GetValue(OffsetPaddingProperty); }
            set { SetValue(OffsetPaddingProperty, value); }
        }
        public static readonly DependencyProperty OffsetPaddingProperty =
            DependencyProperty.Register("OffsetPadding", typeof(Thickness), typeof(SaturationBrightnessChooser), new UIPropertyMetadata(new Thickness(0.0)));

        public double Hue
        {
            private get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }
        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof(double), typeof(SaturationBrightnessChooser), new
            FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(HueChanged)));


        public static void HueChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            SaturationBrightnessChooser h = (SaturationBrightnessChooser)o;
            h.UpdateColor();
        }

        public double SaturationOffset
        {
            get { return (double)GetValue(SaturationOffsetProperty); }
            set { SetValue(SaturationOffsetProperty, value); }
        }
        public static readonly DependencyProperty SaturationOffsetProperty =
            DependencyProperty.Register("SaturationOffset", typeof(double), typeof(SaturationBrightnessChooser), new UIPropertyMetadata(0.0));

        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register("Saturation", typeof(double), typeof(SaturationBrightnessChooser),
            new FrameworkPropertyMetadata(0.0,
            new PropertyChangedCallback(SaturationChanged),
            new CoerceValueCallback(SaturationCoerce)));

        public static void SaturationChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            SaturationBrightnessChooser h = (SaturationBrightnessChooser)o;
            h.UpdateSaturationOffset();
        }

        public static object SaturationCoerce(DependencyObject d, object Brightness)
        {
            double v = (double)Brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }




        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            private set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(SaturationBrightnessChooser), new UIPropertyMetadata(Colors.Red));

        public SolidColorBrush ColorBrush
        {
            get { return (SolidColorBrush)GetValue(ColorBrushProperty); }
            private set { SetValue(ColorBrushProperty, value); }
        }
        public static readonly DependencyProperty ColorBrushProperty =
            DependencyProperty.Register("ColorBrush", typeof(SolidColorBrush), typeof(SaturationBrightnessChooser), new UIPropertyMetadata(Brushes.Red));


        public double BrightnessOffset
        {
            get { return (double)GetValue(BrightnessOffsetProperty); }
            set { SetValue(BrightnessOffsetProperty, value); }
        }
        public static readonly DependencyProperty BrightnessOffsetProperty =
            DependencyProperty.Register("BrightnessOffset", typeof(double), typeof(SaturationBrightnessChooser), new UIPropertyMetadata(0.0));

        public double Brightness
        {
            get { return (double)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double), typeof(SaturationBrightnessChooser),
            new FrameworkPropertyMetadata(0.0,
            new PropertyChangedCallback(BrightnessChanged),
            new CoerceValueCallback(BrightnessCoerce)));

        public static void BrightnessChanged(object o, DependencyPropertyChangedEventArgs e)
        {
            SaturationBrightnessChooser h = (SaturationBrightnessChooser)o;
            h.UpdateBrightnessOffset();
        }

        public static object BrightnessCoerce(DependencyObject d, object Brightness)
        {
            double v = (double)Brightness;
            if (v < 0) return 0.0;
            if (v > 1) return 1.0;
            return v;
        }

        private void UpdateSaturationOffset()
        {
            SaturationOffset = OffsetPadding.Left + ((ActualWidth - (OffsetPadding.Right + OffsetPadding.Left)) * Saturation);
        }

        private void UpdateBrightnessOffset()
        {
            BrightnessOffset = OffsetPadding.Top + ((ActualHeight - (OffsetPadding.Bottom + OffsetPadding.Top)) - ((ActualHeight - (OffsetPadding.Bottom + OffsetPadding.Top)) * Brightness));
        }

        protected override void OnRender(DrawingContext dc)
        {
            LinearGradientBrush h = new LinearGradientBrush();
            h.StartPoint = new Point(0, 0);
            h.EndPoint = new Point(1, 0);
            h.GradientStops.Add(new GradientStop(Colors.White, 0.00));
            h.GradientStops.Add(new GradientStop(ColorPickerUtil.ColorFromHSB(Hue, 1, 1), 1.0));
            dc.DrawRectangle(h, null, new Rect(0, 0, ActualWidth, ActualHeight));

            LinearGradientBrush v = new LinearGradientBrush();
            v.StartPoint = new Point(0, 0);
            v.EndPoint = new Point(0, 1);
            v.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0, 0, 0), 1.00));
            v.GradientStops.Add(new GradientStop(Color.FromArgb(0x80, 0, 0, 0), 0.50));
            v.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0, 0, 0), 0.00));
            dc.DrawRectangle(v, null, new Rect(0, 0, ActualWidth, ActualHeight));

            UpdateSaturationOffset();
            UpdateBrightnessOffset();
        }

        public void UpdateColor()
        {
            Color = ColorPickerUtil.ColorFromHSB(Hue, Saturation, Brightness);
            ColorBrush = new SolidColorBrush(Color);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(this);
                Saturation = (p.X / (this.ActualWidth - OffsetPadding.Right));
                Brightness = (((this.ActualHeight - OffsetPadding.Bottom) - p.Y) / (this.ActualHeight - OffsetPadding.Bottom));
                UpdateColor();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            Saturation = (p.X / (this.ActualWidth - OffsetPadding.Right));
            Brightness = (((this.ActualHeight - OffsetPadding.Bottom) - p.Y) / (this.ActualHeight - OffsetPadding.Bottom));
            UpdateColor();

            Mouse.Capture(this);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            base.OnMouseUp(e);
        }
    }



    public static class ColorPickerUtil
    {

        public static string MakeValidColorString(string S)
        {
            string s = S;

            // remove invalid characters (this is a very forgiving function)
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (!(c >= 'a' && c <= 'f') &&
                    !(c >= 'A' && c <= 'F') &&
                    !(c >= '0' && c <= '9'))
                {
                    s = s.Remove(i, 1);
                    i--;
                }
            }

            // trim if too long
            if (s.Length > 8) s = s.Substring(0, 8);

            // pad with zeroes until a valid length is found
            while (s.Length <= 8 && s.Length != 3 && s.Length != 4 && s.Length != 6 && s.Length != 8)
            {
                s = s + "0";
            }

            return s;
        }

        public static Color ColorFromString(string S)
        {
            //ColorConverter converter = new ColorConverter();
            Color c = (Color) ColorConverter.ConvertFromString(S);

            return c;
            /*
            string s = MakeValidColorString(S);

            byte A = 255;
            byte R = 0;
            byte G = 0;
            byte B = 0;

            // interpret 3 characters as RRGGBB (where R, G, and B are each repeated)
            if (s.Length == 3)
            {
                R = byte.Parse(s.Substring(0, 1) + s.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                G = byte.Parse(s.Substring(1, 1) + s.Substring(1, 1), System.Globalization.NumberStyles.HexNumber);
                B = byte.Parse(s.Substring(2, 1) + s.Substring(2, 1), System.Globalization.NumberStyles.HexNumber);
            }

            // interpret 4 characters as AARRGGBB (where A, R, G, and B are each repeated)
            if (s.Length == 4)
            {
                A = byte.Parse(s.Substring(0, 1) + s.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
                R = byte.Parse(s.Substring(1, 1) + s.Substring(1, 1), System.Globalization.NumberStyles.HexNumber);
                G = byte.Parse(s.Substring(2, 1) + s.Substring(2, 1), System.Globalization.NumberStyles.HexNumber);
                B = byte.Parse(s.Substring(3, 1) + s.Substring(3, 1), System.Globalization.NumberStyles.HexNumber);
            }

            // interpret 6 characters as RRGGBB
            if (s.Length == 6)
            {
                R = byte.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                G = byte.Parse(s.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                B = byte.Parse(s.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            // interpret 8 characters as AARRGGBB
            if (s.Length == 8)
            {
                A = byte.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                R = byte.Parse(s.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                G = byte.Parse(s.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                B = byte.Parse(s.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return Color.FromArgb(A, R, G, B);
             */ 
        }

        static char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static string StringFromColor(Color c)
        {
            byte[] bytes = new byte[4];
            bytes[0] = c.A;
            bytes[1] = c.R;
            bytes[2] = c.G;
            bytes[3] = c.B;

            char[] chars = new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }

            return new string(chars);
        }

        public static void HSBFromColor(Color C, ref double H, ref double S, ref double B)
        {
            // standard algorithm from nearly any graphics textbook

            byte _red = C.R;
            byte _green = C.G;
            byte _blue = C.B;

            int imax = _red, imin = _red;

            if (_green > imax) imax = _green; else if (_green < imin) imin = _green;
            if (_blue > imax) imax = _blue; else if (_blue < imin) imin = _blue;
            double max = imax / 255.0, min = imin / 255.0;

            double value = max;
            double saturation = (max > 0) ? (max - min) / max : 0.0;
            double hue = 0;

            if (imax > imin)
            {
                double f = 1.0 / ((max - min) * 255.0);
                hue = (imax == _red) ? 0.0 + f * (_green - _blue)
                    : (imax == _green) ? 2.0 + f * (_blue - _red)
                    : 4.0 + f * (_red - _green);
                hue = hue * 60.0;
                if (hue < 0.0)
                    hue += 360.0;
            }

            H = hue / 360;
            S = saturation;
            B = value;
        }

        public static Color ColorFromAHSB(double A, double H, double S, double B)
        {
            Color r = ColorFromHSB(H, S, B);
            r.A = (byte)Math.Round(A * 255);
            return r;
        }

        public static Color ColorFromHSB(double H, double S, double B)
        {
            // standard algorithm from nearly any graphics textbook

            double red = 0.0, green = 0.0, blue = 0.0;

            if (S == 0.0)
            {
                red = green = blue = B;
            }
            else
            {
                double h = H * 360;
                while (h >= 360.0)
                    h -= 360.0;

                h = h / 60.0;
                int i = (int)h;

                double f = h - i;
                double r = B * (1.0 - S);
                double s = B * (1.0 - S * f);
                double t = B * (1.0 - S * (1.0 - f));

                switch (i)
                {
                    case 0: red = B; green = t; blue = r; break;
                    case 1: red = s; green = B; blue = r; break;
                    case 2: red = r; green = B; blue = t; break;
                    case 3: red = r; green = s; blue = B; break;
                    case 4: red = t; green = r; blue = B; break;
                    case 5: red = B; green = r; blue = s; break;
                }
            }

            byte iRed = (byte)(red * 255.0), iGreen = (byte)(green * 255.0), iBlue = (byte)(blue * 255.0);
            return Color.FromRgb(iRed, iGreen, iBlue);
        }
    }

}
