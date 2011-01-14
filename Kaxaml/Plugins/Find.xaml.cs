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
using KaxamlPlugins;

namespace Kaxaml.Plugins.Default
{
    /// <summary>
    /// Interaction logic for Find.xaml
    /// </summary>

    public partial class Find : System.Windows.Controls.UserControl
    {
        public Find()
        {
            InitializeComponent();
        }

        private void DoFind(object sender, RoutedEventArgs e)
        {
            KaxamlInfo.Editor.Find(FindText.Text);
        }

        private void DoReplace(object sender, RoutedEventArgs e)
        {
            KaxamlInfo.Editor.Replace(FindText.Text, ReplaceText.Text, (bool)Selection.IsChecked);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t != null)
            {
                if (!string.IsNullOrEmpty(t.Text))
                {
                    t.SelectAll();
                }
            }
        }
    }
}