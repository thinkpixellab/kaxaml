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
using System.Windows.Controls.Primitives;
using System.Diagnostics;


namespace Kaxaml.Controls
{
    public class ZoomFrame : Frame
    {
        static ZoomFrame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomFrame), new FrameworkPropertyMetadata(typeof(ZoomFrame)));
        }

        #region Private Fields

        private Thumb PART_Thumb;

        #endregion

        #region IsDraggable (DependencyProperty)

        public bool IsDraggable
        {
            get { return (bool)GetValue(IsDraggableProperty); }
            set { SetValue(IsDraggableProperty, value); }
        }
        public static readonly DependencyProperty IsDraggableProperty =
            DependencyProperty.Register("IsDraggable", typeof(bool), typeof(ZoomFrame), new FrameworkPropertyMetadata(default(bool)));

        #endregion

        #region IsDragUIVisible (DependencyProperty)

        public bool IsDragUIVisible
        {
            get { return (bool)GetValue(IsDragUIVisibleProperty); }
            set { SetValue(IsDragUIVisibleProperty, value); }
        }
        public static readonly DependencyProperty IsDragUIVisibleProperty  =
            DependencyProperty.Register("IsDragUIVisible", typeof(bool), typeof(ZoomFrame), new FrameworkPropertyMetadata(false));

        #endregion


        #region Scale (DependencyProperty)

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(ZoomFrame), new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(ScaleChanged)));

        private static void ScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ZoomFrame)
            {
                ZoomFrame owner = (ZoomFrame)obj;
                if (owner.Scale > 1.001)
                {
                    owner.IsDragUIVisible = true;
                }
                else
                {
                    owner.IsDragUIVisible = false;
                    owner.IsDraggable = false;
                    //owner.ScaleOrigin = new Point(0.5, 0.5);
                }
            }
        }

        #endregion


        #region ScaleOrigin (DependencyProperty)

        public Point ScaleOrigin
        {
            get { return (Point)GetValue(ScaleOriginProperty); }
            set { SetValue(ScaleOriginProperty, value); }
        }
        public static readonly DependencyProperty ScaleOriginProperty =
            DependencyProperty.Register("ScaleOrigin", typeof(Point), typeof(ZoomFrame), new FrameworkPropertyMetadata(new Point(0.5, 0.5)));

        #endregion

        #region overrides

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.IsDown && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (Scale > 1)
                {
                    IsDraggable = true;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            IsDraggable = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_Thumb = this.Template.FindName("PART_Thumb", this) as Thumb;

            if (PART_Thumb != null)
            {
                PART_Thumb.DragStarted += new DragStartedEventHandler(PART_Thumb_DragStarted);
                PART_Thumb.DragDelta += new DragDeltaEventHandler(PART_Thumb_DragDelta);
                PART_Thumb.DragCompleted += new DragCompletedEventHandler(PART_Thumb_DragCompleted);
            }
        }

        private bool _isDragging = false;

        void PART_Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isDragging = true;
        }

        void PART_Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_isDragging)
            {
                if (Scale > 1.0)
                {
                    double x = ScaleOrigin.X - (e.HorizontalChange / (this.ActualWidth * Scale));
                    if (x > 1) x = 1;
                    if (x < 0) x = 0;

                    double y = ScaleOrigin.Y - (e.VerticalChange / (this.ActualHeight * Scale));
                    if (y > 1) y = 1;
                    if (y < 0) y = 0;

                    ScaleOrigin = new Point(x, y);
                }
            }
        }

        void PART_Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDragging = false;
        }





        #endregion
    }
}
