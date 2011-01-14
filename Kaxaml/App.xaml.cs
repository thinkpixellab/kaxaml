using System;
using System.Windows;
using System.Data;
using System.Xml;
using System.Configuration;
using System.Collections;
using System.Text.RegularExpressions;
using Kaxaml.Properties;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;
using System.IO;
using Kaxaml.Plugins.Default;

namespace Kaxaml
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        private Snippets _Snippets;
        public Snippets Snippets
        {
            get { return _Snippets; }
            set { _Snippets = value; }
        }

        void app_Startup(object sender, StartupEventArgs e)
        {
            //if (e.Args.Length > 0)
            //{
            //    StartupArgs = e.Args[0];
            //}

            _startupArgs = e.Args;
        }


        [PreserveSig()]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetModuleFileName
        (
            [In]
            HandleRef module,

            [Out]
            StringBuilder buffer,

            [In]
            [MarshalAs(UnmanagedType.U4)]int capacity
        );

        private static String[] _startupArgs;
        public static String[] StartupArgs
        {
            get
            {
                return _startupArgs;
            }
        }

        private static String _startupPath;
        public static String StartupPath
        {
            get
            {
                // Only retrieve startup path when
                // it wans’t known.
                if (_startupPath == null)
                {
                    HandleRef nullHandle =
                        new HandleRef(null, IntPtr.Zero);

                    StringBuilder buffer =
                        new StringBuilder(260);

                    int lenght = GetModuleFileName(
                        nullHandle,
                        buffer,
                        buffer.Capacity);

                    if (lenght == 0)
                    {
                        // This ctor overload uses 
                        // GetLastWin32Error to
                        //get its error code.
                        throw new Win32Exception();
                    }

                    String moduleFilename =
                        buffer.ToString(0, lenght);
                    _startupPath =
                        Path.GetDirectoryName(moduleFilename);
                }

                return _startupPath;
            }
        }
    }
}