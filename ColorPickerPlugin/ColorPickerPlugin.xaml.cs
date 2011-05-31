using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Kaxaml.Plugins.Controls;
using KaxamlPlugins;
using PixelLab.Common;

namespace Kaxaml.Plugins.ColorPicker
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>

    [Plugin(
        Name = "Color Picker",
        Icon = "Images\\icon.png",
        Description = "Generate colors and save palletes (Ctrl+P)",
        ModifierKeys = ModifierKeys.Control,
        Key = Key.P
     )]

    public partial class ColorPickerPlugin : UserControl
    {
        public ColorPickerPlugin()
        {
            InitializeComponent();
            Colors = new ObservableCollection<Color>();
            ColorString = Kaxaml.Plugins.ColorPicker.Properties.Settings.Default.Colors;

            KaxamlInfo.EditSelectionChanged += new KaxamlInfo.EditSelectionChangedDelegate(KaxamlInfo_EditSelectionChanged);
        }

        #region Sync Interaction Logic

        ColorConverter converter = new ColorConverter();
        void KaxamlInfo_EditSelectionChanged(string SelectedText)
        {
            // wish we could do this without a try catch!
            try
            {
                Color c = (Color)ColorConverter.ConvertFromString(KaxamlInfo.Editor.SelectedText);
                SyncButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                if (ex.IsCriticalException())
                {
                    throw;
                }

                SyncButton.IsEnabled = false;
                SyncButton.IsChecked = false;
            }
        }

        private void SyncButtonChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Color c = (Color)ColorConverter.ConvertFromString(KaxamlInfo.Editor.SelectedText);
                C.Color = c;

                C.ColorChanged += C_ColorChanged;
            }
            catch (Exception ex)
            {
                if (ex.IsCriticalException())
                {
                    throw;
                }

                SyncButton.IsEnabled = false;
            }
        }

        private void SyncButtonUnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                C.ColorChanged -= C_ColorChanged;
            }
            catch (Exception ex)
            {
                if (ex.IsCriticalException())
                {
                    throw;
                }

                SyncButton.IsEnabled = false;
            }
        }

        DispatcherTimer _ColorChangedTimer;
        Color _ColorChangedColor;

        void C_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            if ((bool)SyncButton.IsChecked)
            {
                try
                {
                    if (_ColorChangedTimer == null)
                    {
                        _ColorChangedTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Background, _ColorChangedTimer_Tick, this.Dispatcher);
                    }

                    _ColorChangedTimer.Stop();
                    _ColorChangedTimer.Start();

                    _ColorChangedColor = e.Color;
                }
                catch (Exception ex)
                {
                    if (ex.IsCriticalException())
                    {
                        throw;
                    }
                }
            }
        }

        void _ColorChangedTimer_Tick(object sender, EventArgs e)
        {
            _ColorChangedTimer.Stop();

            KaxamlInfo.Editor.ReplaceSelectedText(_ColorChangedColor.ToString());
            KaxamlInfo.Parse();
        }

        #endregion

        #region Event Handlers

        private void CopyColor(object o, EventArgs e)
        {
            Clipboard.SetText(C.Color.ToString());
        }

        private void SaveColor(object o, EventArgs e)
        {
            Colors.Add(C.Color);
        }

        private void RemoveColor(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            ListBoxItem lbi = (ListBoxItem)cm.PlacementTarget;
            Colors.Remove((Color)lbi.Content);
        }

        private void RemoveAllColors(object o, EventArgs e)
        {
            Colors.Clear();
        }

        private void SwatchMouseDown(object o, MouseEventArgs e)
        {
            Color c = (Color)(o as FrameworkElement).DataContext;
            C.Color = c;
        }

        #endregion

        #region Colors (DependencyProperty)

        /// <summary>
        /// description of the property
        /// </summary>
        public ObservableCollection<Color> Colors
        {
            get { return (ObservableCollection<Color>)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for Colors
        /// </summary>
        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register("Colors", typeof(ObservableCollection<Color>), typeof(ColorPickerPlugin), new FrameworkPropertyMetadata(default(ObservableCollection<Color>), new PropertyChangedCallback(ColorsChanged)));

        /// <summary>
        /// PropertyChangedCallback for Colors
        /// </summary>
        private static void ColorsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ColorPickerPlugin)
            {
                ColorPickerPlugin owner = (ColorPickerPlugin)obj;

                ObservableCollection<Color> c = args.NewValue as ObservableCollection<Color>;
                if (c != null)
                {
                    c.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(owner.c_CollectionChanged);
                }

            }
        }

        private void c_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!updateinternal)
            {
                updateinternal = true;

                string s = "";
                foreach (Color c in Colors)
                {
                    s = s + c.ToString() + DELIMITER;
                }
                ColorString = s;

                updateinternal = false;
            }
        }

        #endregion

        #region ColorString (DependencyProperty)

        /// <summary>
        /// description of the property
        /// </summary>
        public string ColorString
        {
            get { return (string)GetValue(ColorStringProperty); }
            set { SetValue(ColorStringProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for ColorString
        /// </summary>
        public static readonly DependencyProperty ColorStringProperty =
            DependencyProperty.Register("ColorString", typeof(string), typeof(ColorPickerPlugin), new FrameworkPropertyMetadata(
                default(string),
                new PropertyChangedCallback(ColorStringChanged)));

        /// <summary>
        /// PropertyChangedCallback for ColorString
        /// </summary>
        private static void ColorStringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ColorPickerPlugin)
            {
                ColorPickerPlugin owner = (ColorPickerPlugin)obj;
                Kaxaml.Plugins.ColorPicker.Properties.Settings.Default.Colors = args.NewValue as string;

                if (!owner.updateinternal)
                {
                    owner.updateinternal = true;

                    owner.Colors.Clear();
                    string[] colors = (args.NewValue as string).Split(owner.DELIMITER);

                    foreach (string s in colors)
                    {
                        try
                        {
                            if (s.Length > 3)
                            {
                                Color c = ColorPickerUtil.ColorFromString(s);
                                owner.Colors.Add(c);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.IsCriticalException())
                            {
                                throw;
                            }
                        }
                    }

                    owner.updateinternal = false;
                }
            }
        }

        #endregion

        private char DELIMITER = '|';
        bool updateinternal = false;
    }
}