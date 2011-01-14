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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System.Windows.Threading;
using System.Diagnostics;
using Kaxaml.Controls;
using System.Collections;

namespace Kaxaml.CodeCompletion
{
    /// <summary>
    /// Interaction logic for CodeCompletionPopup.xaml
    /// </summary>

    public partial class CodeCompletionPopup : System.Windows.Controls.Primitives.Popup
    {

        bool _OverrideForceAccept = false;

        public CodeCompletionPopup()
        {
            InitializeComponent();
            
        }

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
            DependencyProperty.Register("CompletionItems", typeof(ArrayList), typeof(CodeCompletionPopup), new FrameworkPropertyMetadata(null));

        #endregion

        #region Methods

        public void SelectNext()
        {
            CompletionListBox.SelectNext();
            _OverrideForceAccept = true;
        }

        public void SelectPrevious()
        {
            CompletionListBox.SelectPrevious();
            _OverrideForceAccept = true;
        }

        public void PageDown()
        {
            CompletionListBox.PageDown();
            _OverrideForceAccept = true;
        }

        public void PageUp()
        {
            CompletionListBox.PageUp();
            _OverrideForceAccept = true;
        }

        public void Cancel()
        {
            RaiseResultProvidedEvent(null, "", false, true);
            Hide();
        }

        public static bool IsOpenSomewhere
        {
            get
            {
                if (popup != null)
                {
                    return popup.IsOpen;
                }
                else
                {
                    return false;
                }
            }
        }

        public ICompletionData SelectedItem
        {
            get
            {
                if (this.CompletionListBox.SelectedIndex > -1 && this.CompletionListBox.SelectedItem is ICompletionData)
                {
                    return this.CompletionListBox.SelectedItem as ICompletionData;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Accept(bool force)
        {
            if (CompletionListBox.SelectedItem != null)
            {
                ICompletionData item = CompletionListBox.SelectedItem as ICompletionData;

                if (item != null)
                {
                    RaiseResultProvidedEvent(item, item.Text, (force || _OverrideForceAccept), false);
                    Hide();
                }
            }
        }

        public void DoSearch(string Prefix)
        {
            if (IsOpen)
            {
                int index = SearchForItem(Prefix);
                if (index >= 0)
                {
                    CompletionListBox.SelectedIndex = index;
                    //FrameworkElement fe = CompletionListBox.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
                    //if (fe != null) fe.BringIntoView();
                }
            }

        }

        private string _CuePrefix = "";

        public void CueSearch(string Prefix)
        {
            _CuePrefix = Prefix;
            this.Opened += new EventHandler(CodeCompletionPopup_Opened);
        }

        void CodeCompletionPopup_Opened(object sender, EventArgs e)
        {
            this.Opened -= new EventHandler(CodeCompletionPopup_Opened);
            if (_CuePrefix.Length > 0)
            {
                DoSearch(_CuePrefix);
            }
            _CuePrefix = "";
        }

        int _LastIndex = 0;
        int SearchForItem(string prefix)
        {
            int indexOfItem = -1;
            for (int i = 0; i < CompletionItems.Count; i++)
            {
                if ((CompletionItems[i] as ICompletionData).Text.Length >= prefix.Length)
                {
                    if ((CompletionItems[i] as ICompletionData).Text.Substring(0, prefix.Length).ToLower().Equals(prefix.ToLower()))
                    {
                        indexOfItem = i;
                        break;
                    }
                }
            }

            //if (indexOfItem == -1)
            //{
            //    for (int i = 0; i < _LastIndex; i++)
            //    {
            //        if ((CompletionItems[i] as XmlCompletionData).Text.Length >= prefix.Length)
            //        {
            //            if ((CompletionItems[i] as XmlCompletionData).Text.Substring(0, prefix.Length).ToLowerInvariant().Equals(prefix.ToLowerInvariant()))
            //            {
            //                indexOfItem = i;
            //                break;
            //            }
            //        }
            //    }
            //}

            return indexOfItem;
        }

        public void Show()
        {
            //CurrentPrefix = "";
            IsOpen = true;
        }

        public void Hide()
        {
            IsOpen = false;
        }

        #endregion

        #region Events

        #region ResultProvidedEvent

        public delegate void ResultProvidedeventHandler(object sender, ResultProvidedEventArgs e);

        public static readonly RoutedEvent ResultProvidedEvent = EventManager.RegisterRoutedEvent("ResultProvided", RoutingStrategy.Bubble, typeof(ResultProvidedeventHandler), typeof(CodeCompletionPopup));

        public event ResultProvidedeventHandler ResultProvided
        {
            add { AddHandler(ResultProvidedEvent, value); }
            remove { RemoveHandler(ResultProvidedEvent, value); }
        }

        void RaiseResultProvidedEvent(ICompletionData item, string text, bool forcedAccept, bool cancelled)
        {
            ResultProvidedEventArgs newEventArgs = new ResultProvidedEventArgs(CodeCompletionPopup.ResultProvidedEvent, item, text, forcedAccept, cancelled);
            RaiseEvent(newEventArgs);
        }

        #endregion

        #endregion

        #region Static Show Methods (and Support Types)

        private static CodeCompletionPopup popup = null;

        public static CodeCompletionPopup Show(ArrayList items, Point p)
        {
            if (popup == null)
            {
                popup = new CodeCompletionPopup();
            }

            //popup.VerticalOffset = p.Y;
            //popup.HorizontalOffset = p.X;

            double font = Kaxaml.Properties.Settings.Default.EditorFontSize;

            popup.PlacementRectangle = new Rect(p.X, p.Y - font, 1, font);

            popup.CompletionItems = items;

            if (items == null || items.Count == 0)
            {
                return popup;
            }

            popup.CompletionListBox.SelectedIndex = 0;
            popup._OverrideForceAccept = false;
            popup.Show();

            return popup;
        }


        #endregion

        private void DoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Accept(true);
        }

    }


    public class ResultProvidedEventArgs : RoutedEventArgs
    {
        public ResultProvidedEventArgs(RoutedEvent routedEvent, ICompletionData item, string text, bool forcedAccept, bool cancelled)
        {
            this.Item = item;
            this.RoutedEvent = routedEvent;
            this.ForcedAccept = forcedAccept;
            this.Text = text;
            this.Cancelled = cancelled;
        }

        private bool _ForcedAccept;
        public bool ForcedAccept
        {
            get { return _ForcedAccept; }
            set { _ForcedAccept = value; }
        }

        private ICompletionData _Item;
        public ICompletionData Item
        {
            get { return _Item; }
            set { _Item = value; }
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        private bool _Cancelled;
        public bool Cancelled
        {
            get { return _Cancelled; }
            set { _Cancelled = value; }
        }
    }


}