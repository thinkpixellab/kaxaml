using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KaxamlPlugins
{
    public static class KaxamlInfo 
    {
        public delegate void EditSelectionChangedDelegate(string SelectedText);
        public static event EditSelectionChangedDelegate EditSelectionChanged;

        public delegate void ParseRequestedDelegate();
        public static event ParseRequestedDelegate ParseRequested;

        public delegate void ContentLoadedDelegate();
        public static event ContentLoadedDelegate ContentLoaded;

        private static IKaxamlInfoTextEditor _Editor;
        public static IKaxamlInfoTextEditor Editor
        {
            get { return _Editor; }
            set 
            {
                // remove current event handler
                if (_Editor != null)
                {
                    _Editor.TextSelectionChanged -= new RoutedEventHandler(_Editor_TextSelectionChanged);
                }

                _Editor = value;

                // add new event handler
                if (_Editor != null)
                {
                    _Editor.TextSelectionChanged += new RoutedEventHandler(_Editor_TextSelectionChanged);
                }
            }
        }

        static void _Editor_TextSelectionChanged(object sender, RoutedEventArgs e)
        {
            var handler = EditSelectionChanged;
            if (handler != null)
                handler(_Editor.SelectedText);
        }

        private static Window _MainWindow;
        public static Window MainWindow
        {
            get { return _MainWindow; }
            set 
            {
                _MainWindow = value;
                NotifyPropertyChanged("MainWindow");
            }
        }

        private static Frame _Frame;
        public static Frame Frame
        {
            get { return _Frame; }
            set 
            {
                if (_Frame != value)
                {
                    _Frame = value;
                    NotifyPropertyChanged("Frame");
                }
            }
        }

        public static void Parse()
        {
            var handler = ParseRequested;
            if (null != handler)
            {
                handler();
            }
        }

        public static void RaiseContentLoaded()
        {
            var handler = ContentLoaded;
            if (null != handler)
            {
                handler();
            }
        }


        #region INotifyPropertyChanged

        public static event PropertyChangedEventHandler PropertyChanged;

        private static void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(null, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
