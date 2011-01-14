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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Collections;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System.Windows.Media.Animation;

namespace Kaxaml.CodeCompletion
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>

    public partial class StringBox : System.Windows.Controls.UserControl
    {

		#region Constructors 

        public StringBox()
        {
            InitializeComponent();

            StringHostItems = new ObservableCollection<StringHost>();

            StringHostItems.Add(new StringHost("")); //1
            StringHostItems.Add(new StringHost("")); //2
            StringHostItems.Add(new StringHost("")); //3
            StringHostItems.Add(new StringHost("")); //4
            StringHostItems.Add(new StringHost("")); //5
            StringHostItems.Add(new StringHost("")); //6
            StringHostItems.Add(new StringHost("")); //7
            StringHostItems.Add(new StringHost("")); //8
            StringHostItems.Add(new StringHost("")); //9
            StringHostItems.Add(new StringHost("")); //10
        }

		#endregion Constructors 


        #region Private Fields

        StringHost _SelectedItem = null;
        int _SelectedIndexInView = -1;
        int _TopOffset = 0;

        const int _ItemsInView = 10;
        //int _SelectedIndexInCollection = -1;

        #endregion

        #region SelectedIndex

        public int SelectedIndex
        {
            get 
            {
                return _TopOffset + _SelectedIndexInView;
            }
            set 
            {
                if (value >= 0 && value < CompletionItems.Count)
                {

                    // if index is greater than 10 or less than count - 10, then
                    // we can just set the top offset and select the top item

                    if (value > 10 - 1 && value < CompletionItems.Count - 10)
                    {
                        SetTopOffset(value);
                        SelectItemByIndex(0);
                    }
                    else
                    {
                        if (value <= 10 - 1)
                        {
                            SetTopOffset(0);
                            SelectItemByIndex(value);
                        }
                        else if (value > CompletionItems.Count - 10)
                        {
                            SetTopOffset(CompletionItems.Count - 10);
                            SelectItemByIndex(value - _TopOffset);
                        }
                    }
                }
            }
        }

        #endregion

        #region SelectedItem

        public object SelectedItem
        {
            get 
            {
                return CompletionItems[SelectedIndex];
            }
        }


        #endregion

        #region StringHostItems (DependencyProperty)

        /// <summary>
        /// description of the property
        /// </summary>
        public ObservableCollection<StringHost> StringHostItems
        {
            get { return (ObservableCollection<StringHost>)GetValue(StringHostItemsProperty); }
            set { SetValue(StringHostItemsProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for StringHostItems
        /// </summary>
        public static readonly DependencyProperty StringHostItemsProperty =
            DependencyProperty.Register("StringHostItems", typeof(ObservableCollection<StringHost>), typeof(StringBox),
            new FrameworkPropertyMetadata(default(ObservableCollection<StringHost>)));

        #endregion

        #region CompletionItems (DependencyProperty)

        /// <summary>
        /// description of the property
        /// </summary>
        public ArrayList CompletionItems
        {
            get { return (ArrayList)GetValue(CompletionItemsProperty); }
            set { SetValue(CompletionItemsProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for CompletionItems
        /// </summary>
        public static readonly DependencyProperty CompletionItemsProperty =
            DependencyProperty.Register("CompletionItems", typeof(ArrayList), typeof(StringBox), new FrameworkPropertyMetadata(default(ArrayList), new PropertyChangedCallback(CompletionItemsChanged)));

        /// <summary>
        /// PropertyChangedCallback for CompletionItems
        /// </summary>
        private static void CompletionItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is StringBox)
            {
                StringBox owner = (StringBox)obj;
                ArrayList items = (args.NewValue as ArrayList);

                if (items != null)
                {
                    int max = Math.Min(items.Count, 10);

                    // add the new items
                    for (int i = 0; i < max; i++)
                    {
                        owner.StringHostItems[i].Value = ((ICompletionData)items[i]).Text;
                        owner.StringHostItems[i].Tooltip = ((ICompletionData)items[i]).Description;
                        owner.StringHostItems[i]._IsSelectable = true;
                    }

                    // clear the remaining items
                    for (int i = items.Count; i < 10; i++)
                    {
                        owner.StringHostItems[i].Value = String.Empty;
                        owner.StringHostItems[i].Tooltip = String.Empty;
                        owner.StringHostItems[i]._IsSelectable = false;
                    }

                    // show or hide the ScrollerSlider based on need and update its max and min values
                    if (items.Count > (10 - 1))
                    {
                        owner.ScrollerSlider.Visibility = Visibility.Visible;
                        owner.ScrollerSlider.Minimum = 0;
                        owner.ScrollerSlider.Maximum = items.Count - 10;
                    }
                    else
                    {
                        owner.ScrollerSlider.Visibility = Visibility.Collapsed;
                    }


                }
            }
        }

        #endregion

        #region Overrides

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Mouse.Capture(this, CaptureMode.SubTree);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            this.ReleaseMouseCapture();

            if (_SelectNextTimer != null)
            {
                _SelectNextTimer.Stop();
            }

            if (_SelectPreviousTimer != null)
            {
                _SelectPreviousTimer.Stop();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                SelectNext();
            }

            base.OnMouseLeave(e);
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetTopOffset((int)e.NewValue, false);

            // the scrolltext feature has been disabled (because it looked goofy!), but all the
            // code is still available if we ever want to revisit it

            //if (ScrollTextTimer == null)
            //{
            //    ScrollTextTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Background, ScrollTextTimer_Tick, this.Dispatcher);
            //}

            //if (StringHostItems[0] != null && ! String.IsNullOrEmpty(StringHostItems[0].Value))
            //{
            //    ScrollTextTimer.Stop();
            //    ShowScrollText(StringHostItems[0].Value[0].ToString());
            //    ScrollTextTimer.Start();
            //}

        }

        void ScrollTextTimer_Tick(object sender, EventArgs e)
        {
            HideScrollText();   
        }

        private void ShowScrollText(string s)
        {
            ScrollText.Text = s;
            
            Storyboard sb = this.FindResource("ShowScrollText") as Storyboard;
            if (sb != null)
            {
                this.BeginStoryboard(sb);
            }

        }

        private void HideScrollText()
        {
            Storyboard sb = this.FindResource("HideScrollText") as Storyboard;
            if (sb != null)
            {
                this.BeginStoryboard(sb);
            }
        }

        #endregion

        #region Private Methods

        private void SetTopOffset(int offset)
        {
            SetTopOffset(offset, true);
        }

        private void SetTopOffset(int offset, bool UpdateScrollBar)
        {
            // coerce the value of offset

            if (offset > CompletionItems.Count - 10) offset = CompletionItems.Count - 10;
            if (offset < 0) offset = 0;

            int max = Math.Min(CompletionItems.Count, 10);


            for (int i = 0; i < max; i++)
            {
                StringHostItems[i].Value = ((ICompletionData) CompletionItems[i + offset]).Text;
                StringHostItems[i].Tooltip = ((ICompletionData)CompletionItems[i + offset]).Description;
            }
            
            if (UpdateScrollBar)
            {
                ScrollerSlider.Value = offset;
            }

            _TopOffset = offset;
        }

        private void SelectItem(StringHost item)
        {
            if (item == null)
            {
                // clearn current selection
                if (_SelectedItem != null) _SelectedItem.IsSelected = false;
            }
            else if (item != null && item._IsSelectable)
            {
                // clearn current selection
                if (_SelectedItem != null) _SelectedItem.IsSelected = false;

                // select the new item
                _SelectedItem = item;
                item.IsSelected = true;

                // udpate the index
                _SelectedIndexInView = StringHostItems.IndexOf(item);
            }
        }

        private int SelectItemByIndex(int index)
        {
            // coerce the index property
            if (index >= CompletionItems.Count) index = CompletionItems.Count - 1;
            if (index < 0) index = 0;

            StringHost item = StringHostItems[index];
            SelectItem(item);

            return _SelectedIndexInView;
        }

        #endregion

        #region Public Methods

        public void SelectNext()
        {
            if (_SelectedIndexInView == (10 - 1))
            {
                // update the top offset
                SetTopOffset(_TopOffset + 1);
            }
            else
            {
                SelectItemByIndex(_SelectedIndexInView + 1);
            }
        }

        public void SelectPrevious()
        {
            if (_SelectedIndexInView == 0)
            {
                // update the top offset
                SetTopOffset(_TopOffset - 1);
            }
            else
            {
                SelectItemByIndex(_SelectedIndexInView - 1);
            }
        }

        public void PageDown()
        {
            if (_SelectedIndexInView == (10 - 1))
            {
                SetTopOffset(_TopOffset + 10);
            }
            else
            {
                SelectItemByIndex(10 - 1);
            }
        }

        public void PageUp()
        {
            if (_SelectedIndexInView == 0)
            {
                SetTopOffset(_TopOffset - 10);
            }
            else
            {
                SelectItemByIndex(0);
            }
        }

        #endregion

        #region Event Handlers

        private void ItemMouseDown(object sender, MouseEventArgs e)
        {
            FrameworkElement element = (sender as FrameworkElement);
            if (element != null)
            {
                StringHost item = (element.DataContext as StringHost);
                SelectItem(item);
            }
        }

        private void ItemMouseUp(object sender, MouseEventArgs e)
        {
            if (_SelectNextTimer != null)
            {
                _SelectNextTimer.Stop();
            }

            if (_SelectPreviousTimer != null)
            {
                _SelectPreviousTimer.Stop();
            }
        }

        private void ItemMouseEnter(object sender, MouseEventArgs e)
        {
            if (_SelectNextTimer != null)
            {
                _SelectNextTimer.Stop();
            }

            if (_SelectPreviousTimer != null)
            {
                _SelectPreviousTimer.Stop();
            }

            if (this.IsMouseCaptured)
            {
                FrameworkElement element = (sender as FrameworkElement);
                if (element != null)
                {
                    StringHost item = (element.DataContext as StringHost);
                    SelectItem(item);
                }
            }
        }

        private DispatcherTimer _SelectNextTimer = null;
        private DispatcherTimer _SelectPreviousTimer = null;

        private void ItemMouseLeave(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                FrameworkElement element = (sender as FrameworkElement);
                if (element != null)
                {
                    StringHost item = (element.DataContext as StringHost);
                    int index = StringHostItems.IndexOf(item);

                    if (index == 9)
                    {
                        Point p = e.GetPosition(element);

                        if (p.Y > 0)
                        {
                            if (_SelectNextTimer == null)
                            {
                                _SelectNextTimer = new DispatcherTimer();
                                _SelectNextTimer.Tick += new EventHandler(_SelectNextTimer_Tick);
                                _SelectNextTimer.Interval = TimeSpan.FromMilliseconds(100);
                            }

                            _SelectNextTimer.Start();
                        }
                    }

                    if (index == 0)
                    {
                        Point p = e.GetPosition(element);

                        if (p.Y < 0)
                        {
                            if (_SelectPreviousTimer == null)
                            {
                                _SelectPreviousTimer = new DispatcherTimer();
                                _SelectPreviousTimer.Tick += new EventHandler(_SelectPreviousTimer_Tick);
                                _SelectPreviousTimer.Interval = TimeSpan.FromMilliseconds(100);
                            }

                            _SelectPreviousTimer.Start();
                        }
                    }

                }
            }
        }

        void _SelectNextTimer_Tick(object sender, EventArgs e)
        {
            SelectNext();
        }

        void _SelectPreviousTimer_Tick(object sender, EventArgs e)
        {
            SelectPrevious();
        }

        #endregion
    }

    public class StringHost : INotifyPropertyChanged
    {

		#region Fields 


        private string _Value;
        private string _Tooltip;

        internal bool _IsSelectable = true;
        private bool _IsSelected = false;

		#endregion Fields 

		#region Constructors 

        public StringHost(string value)
        {
            Value = value;
        }

		#endregion Constructors 

		#region Properties 


        public string Value
        {
            get { return _Value; }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }

        public string Tooltip
        {
            get { return _Tooltip; }
            set
            {
                if (value != _Tooltip)
                {
                    _Tooltip = value;
                    NotifyPropertyChanged("Tooltip");
                }
            }
        }


        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }


		#endregion Properties 

		#region Overridden Methods 

        public override string ToString()
        {
            return Value;
        }

		#endregion Overridden Methods 


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }

    public class ContentItemsControl : ItemsControl
    {

		#region Overridden Methods 

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ContentControl();
        }

		#endregion Overridden Methods 

    }


}