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
using Kaxaml.Controls;
using Microsoft.Win32;

namespace Kaxaml.Plugins
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>

    public partial class Settings : System.Windows.Controls.UserControl
    {

		#region Constructors 

        public Settings()
        {
            InitializeComponent();
        }

		#endregion Constructors 

		#region Private Methods 

        private void EditAgDefaultXaml(object sender, RoutedEventArgs e)
        {
            Kaxaml.Properties.Settings.Default.AgDefaultXaml = XamlEditorDialog.ShowModal(Kaxaml.Properties.Settings.Default.AgDefaultXaml, "Default Silverlight Xaml", Application.Current.MainWindow);
        }

        private void EditWPFDefaultXaml(object sender, RoutedEventArgs e)
        {
            Kaxaml.Properties.Settings.Default.WPFDefaultXaml = XamlEditorDialog.ShowModal(Kaxaml.Properties.Settings.Default.WPFDefaultXaml, "Default WPF Xaml", Application.Current.MainWindow);
        }

		#endregion Private Methods 

    }
}