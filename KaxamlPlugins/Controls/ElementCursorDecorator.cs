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

namespace Kaxaml.Plugins.Controls
{
    public class ElementCursorDecorator : Decorator
    {
        CursorAdorner _CursorAdorner;

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            // setup the adorner layer
            AdornerLayer _AdornerLayer = AdornerLayer.GetAdornerLayer(this);

            if (_AdornerLayer == null)
            {
                return;
            }

            if (_CursorAdorner == null)
            {
                _CursorAdorner = new CursorAdorner(this, this.CursorElement);
            }

            _AdornerLayer.Add(_CursorAdorner);

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (_CursorAdorner != null)
            {
                AdornerLayer layer = VisualTreeHelper.GetParent(_CursorAdorner) as AdornerLayer;
                layer.Remove(_CursorAdorner);
            }

            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _CursorAdorner.Offset = e.GetPosition(this);
            base.OnMouseMove(e);
        }

        static ElementCursorDecorator()
        {
            CursorProperty.OverrideMetadata(typeof(ElementCursorDecorator), new FrameworkPropertyMetadata(Cursors.None));
            ForceCursorProperty.OverrideMetadata(typeof(ElementCursorDecorator), new FrameworkPropertyMetadata(true));
        }

        public UIElement CursorElement
        {
            get { return (UIElement)GetValue(CursorElementProperty); }
            set { SetValue(CursorElementProperty, value); }
        }
        public static readonly DependencyProperty CursorElementProperty =
            DependencyProperty.Register("CursorElement", typeof(UIElement), typeof(ElementCursorDecorator), new UIPropertyMetadata(null));

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
        }
    }

    internal sealed class CursorAdorner : Adorner
    {
        UIElement _Cursor;

        private Point _Offset;
        public Point Offset
        {
            get { return _Offset; }
            set
            {
                _Offset = value;
                InvalidateArrange();
            }
        }

        public CursorAdorner(ElementCursorDecorator Owner, UIElement Cursor)
            : base(Owner)
        {
            _Cursor = Cursor;
            this.AddVisualChild(_Cursor);
        }

        protected override Visual GetVisualChild(int index)
        {
            return _Cursor;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _Cursor.Arrange(new Rect(Offset, _Cursor.DesiredSize));
            return finalSize;
        }

    }
}
