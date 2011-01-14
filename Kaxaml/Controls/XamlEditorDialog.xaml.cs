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

namespace Kaxaml.Controls
{
    /// <summary>
    /// Interaction logic for XamlEditorDialog.xaml
    /// </summary>
    public partial class XamlEditorDialog : Window
    {
        #region fields

        static string _returnText = "";

        #endregion

        public XamlEditorDialog()
        {
            InitializeComponent();
        }

        #region Text (DependencyProperty)

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(XamlEditorDialog), new FrameworkPropertyMetadata(default(string)));

        #endregion

        private static XamlEditorDialog instance = null;
        public static string ShowModal(string text, string title, Window owner)
        {
            if (instance == null)
            {
                instance = new XamlEditorDialog();
            }

            instance.Owner = owner;
            instance.Text = text;
            instance.Title = title;

            bool result = (bool) instance.ShowDialog();

            if (result)
            {
                return _returnText;
            }
            else
            {
                return text;
            }
        }

        bool _closedFromButton = false;

        private void DoDone(object sender, RoutedEventArgs e)
        {
            _returnText = Text;
            _closedFromButton = true;

            DialogResult = true;
            this.Close();
            instance = null;
        }

        private void DoCancel(object sender, RoutedEventArgs e)
        {
            _closedFromButton = true;

            DialogResult = false;
            this.Close();
            instance = null;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_closedFromButton)
            {
                MessageBoxResult r = MessageBox.Show("Do you want to keep the changes you made?", "Keep Changes?", MessageBoxButton.YesNoCancel);

                if (r == MessageBoxResult.Yes)
                {
                    _returnText = Text;
                }
                else if (r == MessageBoxResult.No)
                {
                    // do nothing
                }
                else
                {
                    e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            instance = null;
        }
    }
}
