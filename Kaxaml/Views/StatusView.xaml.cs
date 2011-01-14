using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kaxaml.Views
{
    /// <summary>
    /// Interaction logic for StatusView.xaml
    /// </summary>
    public partial class StatusView : UserControl
    {
        public StatusView()
        {
            InitializeComponent();
        }

        #region CurrentLineNumber (DependencyProperty)

        public int CurrentLineNumber
        {
            get { return (int)GetValue(CurrentLineNumberProperty); }
            set { SetValue(CurrentLineNumberProperty, value); }
        }
        public static readonly DependencyProperty CurrentLineNumberProperty =
            DependencyProperty.Register("CurrentLineNumber", typeof(int), typeof(StatusView), new FrameworkPropertyMetadata(1));

        #endregion

        #region CurrentLinePosition (DependencyProperty)

        public int CurrentLinePosition
        {
            get { return (int)GetValue(CurrentLinePositionProperty); }
            set { SetValue(CurrentLinePositionProperty, value); }
        }
        public static readonly DependencyProperty CurrentLinePositionProperty =
            DependencyProperty.Register("CurrentLinePosition", typeof(int), typeof(StatusView), new FrameworkPropertyMetadata(1));

        #endregion


        #region Zoom (DependencyProperty)

        /// <summary>
        /// A description of the property.
        /// </summary>
        public int Zoom
        {
            get { return (int)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(int), typeof(StatusView), new FrameworkPropertyMetadata(100, new PropertyChangedCallback(ZoomChanged)));

        private static void ZoomChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is StatusView)
            {
                StatusView owner = (StatusView)obj;
                owner.Scale = (double.Parse(args.NewValue.ToString())) / 100.0;
            }
        }

        #endregion

        #region Scale (DependencyProperty)

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(StatusView), new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(ScaleChanged)));

        private static void ScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is StatusView)
            {
                StatusView owner = (StatusView)obj;
                owner.Zoom = (int) ((double) args.NewValue * 100.0);

            }
        }

        #endregion


        #region Public Methods

        public void ZoomIn()
        {
            // find the index closest to the current zoom

            int index = -1;

            for (int i = 0; i < ZoomSlider.Ticks.Count; i++)
            {
                double v = ZoomSlider.Ticks[i];

                if (v <= ZoomSlider.Value)
                {
                    index = ZoomSlider.Ticks.IndexOf(v);
                }
            }

            if (index >= 0 && index < ZoomSlider.Ticks.Count - 1)
            {
                ZoomSlider.Value = ZoomSlider.Ticks[index + 1];
            }

        }

        public void ZoomOut()
        {
            // find the index closest to the current zoom

            int index = -1;

            for (int i = ZoomSlider.Ticks.Count-1; i > 0; i--)
            {
                double v = ZoomSlider.Ticks[i];

                if (v >= ZoomSlider.Value)
                {
                    index = ZoomSlider.Ticks.IndexOf(v);
                }
            }

            if (index > 0 && index <= ZoomSlider.Ticks.Count-1)
            {
                ZoomSlider.Value = ZoomSlider.Ticks[index - 1];
            }

        }

        public void ActualSize()
        {
            ZoomSlider.Value = 100;
        }

        #endregion



    }
}
