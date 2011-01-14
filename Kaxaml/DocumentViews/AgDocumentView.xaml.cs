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
using Kaxaml.Documents;
using System.Diagnostics;
using System.Windows.Threading;
using Kaxaml.Properties;
using System.IO;
using System.Windows.Markup;
using Kaxaml.Controls;
using System.Windows.Media.Animation;
using Kaxaml.CodeCompletion;
using KaxamlPlugins;
using Kaxaml.Silverlight;
using System.Windows.Forms;
using System.Threading;

namespace Kaxaml.DocumentViews
{
    /// <summary>
    /// Interaction logic for AgDocumentView.xaml
    /// </summary>

    public partial class AgDocumentView : System.Windows.Controls.UserControl, IXamlDocumentView
    {

		#region Constructors 

        public AgDocumentView()
        {

            InitializeComponent();


            string dir = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;
            ContentArea.Url = new Uri("file://" + dir.Replace("\\", "/") + "/AgHost/AgHost.html");
            ContentArea.Refresh();

            KaxamlInfo.Frame = null;

            string schemafile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(App.StartupPath + "\\"), Kaxaml.Properties.Settings.Default.AgSchema);
            XmlCompletionDataProvider.LoadSchema(schemafile);

            Parse();
        }

		#endregion Constructors 

		#region Event Handlers 

        void ContentArea_ContentRendered(object sender, EventArgs e)
        {
            KaxamlInfo.RaiseContentLoaded();
        }

		#endregion Event Handlers 


        #region Private Fields

        private static DispatcherTimer dispatcherTimer;
        private bool UnhandledExceptionRaised;

        #endregion        //-------------------------------------------------------------------
        //
        //  Properties
        //
        //-------------------------------------------------------------------


        #region IsValidXaml (DependencyProperty)

        /// <summary>
        /// description of IsValidXaml
        /// </summary>
        public bool IsValidXaml
        {
            get { return (bool)GetValue(IsValidXamlProperty); }
            set { SetValue(IsValidXamlProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for IsValidXaml
        /// </summary>
        public static readonly DependencyProperty IsValidXamlProperty =
            DependencyProperty.Register("IsValidXaml", typeof(bool), typeof(AgDocumentView), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(IsValidXamlChanged)));

        /// <summary>
        /// PropertyChangedCallback for IsValidXaml
        /// </summary>
        private static void IsValidXamlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AgDocumentView)
            {
                AgDocumentView owner = (AgDocumentView)obj;

                if ((bool)args.NewValue)
                {
                    owner.HideErrorUI();
                }
                else
                {
                    owner.ShowErrorUI();
                }
            }
        }

        #endregion

        #region ErrorText (DependencyProperty)

        /// <summary>
        /// description of ErrorText
        /// </summary>
        public string ErrorText
        {
            get { return (string)GetValue(ErrorTextProperty); }
            set { SetValue(ErrorTextProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for ErrorText
        /// </summary>
        public static readonly DependencyProperty ErrorTextProperty =
            DependencyProperty.Register("ErrorText", typeof(string), typeof(AgDocumentView), new FrameworkPropertyMetadata(default(string), new PropertyChangedCallback(ErrorTextChanged)));

        /// <summary>
        /// PropertyChangedCallback for ErrorText
        /// </summary>
        private static void ErrorTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AgDocumentView)
            {
                AgDocumentView owner = (AgDocumentView)obj;
                // handle changed event here
            }
        }

        #endregion

        #region ErrorLineNumber (DependencyProperty)

        /// <summary>
        /// description of ErrorLineNumber
        /// </summary>
        public int ErrorLineNumber
        {
            get { return (int)GetValue(ErrorLineNumberProperty); }
            set { SetValue(ErrorLineNumberProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for ErrorLineNumber
        /// </summary>
        public static readonly DependencyProperty ErrorLineNumberProperty =
            DependencyProperty.Register("ErrorLineNumber", typeof(int), typeof(AgDocumentView), new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(ErrorLineNumberChanged)));

        /// <summary>
        /// PropertyChangedCallback for ErrorLineNumber
        /// </summary>
        private static void ErrorLineNumberChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AgDocumentView)
            {
                AgDocumentView owner = (AgDocumentView)obj;
                // handle changed event here
            }
        }

        #endregion

        #region ErrorLinePosition (DependencyProperty)

        /// <summary>
        /// description of ErrorLinePosition
        /// </summary>
        public int ErrorLinePosition
        {
            get { return (int)GetValue(ErrorLinePositionProperty); }
            set { SetValue(ErrorLinePositionProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for ErrorLinePosition
        /// </summary>
        public static readonly DependencyProperty ErrorLinePositionProperty =
            DependencyProperty.Register("ErrorLinePosition", typeof(int), typeof(AgDocumentView), new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(ErrorLinePositionChanged)));

        /// <summary>
        /// PropertyChangedCallback for ErrorLinePosition
        /// </summary>
        private static void ErrorLinePositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AgDocumentView)
            {
                AgDocumentView owner = (AgDocumentView)obj;
                // handle changed event here
            }
        }

        #endregion

        #region PreviewImage (DependencyProperty)

        public ImageSource PreviewImage
        {
            get { return (ImageSource)GetValue(PreviewImageProperty); }
            private set { SetValue(PreviewImagePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey PreviewImagePropertyKey =
            DependencyProperty.RegisterReadOnly("PreviewImage", typeof(ImageSource), typeof(AgDocumentView), new UIPropertyMetadata(default(ImageSource)));
        public static readonly DependencyProperty PreviewImageProperty = PreviewImagePropertyKey.DependencyProperty;

        #endregion

        #region Scale (DependencyProperty)

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(AgDocumentView), new FrameworkPropertyMetadata(1.0));

        #endregion        //-------------------------------------------------------------------
        //
        //  Event Handlers
        //
        //-------------------------------------------------------------------


        #region Event Handlers

        private void EditorTextChanged(object sender, Kaxaml.Controls.TextChangedEventArgs e)
        {
            if (XamlDocument != null)
            {
                if (_IsInitializing)
                {
                    _IsInitializing = false;
                }
                else
                {
                    ClearDispatcherTimer();
                    AttemptParse();
                }
            }
        }

        private void LineNumberClick(object sender, RoutedEventArgs e)
        {
            Editor.SelectLine(ErrorLineNumber - 1);
            Editor.Focus();
            Editor.TextEditor.Focus();
        }

        #endregion        //-------------------------------------------------------------------
        //
        //  Private Methods
        //
        //-------------------------------------------------------------------


        #region Private Methods

        private static void ClearDispatcherTimer()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                dispatcherTimer = null;
            }
        }

        private void AttemptParse()
        {
            if (Settings.Default.EnableAutoParse)
            {
                ClearDispatcherTimer();

                TimeSpan timeout = new TimeSpan(0, 0, 0, Settings.Default.AutoParseTimeout);

                dispatcherTimer =
                    new DispatcherTimer(
                        timeout,
                        DispatcherPriority.ApplicationIdle,
                        new EventHandler(ParseCallback),
                        Dispatcher.CurrentDispatcher);
            }
        }

        private void ParseCallback(object sender, EventArgs args)
        {
            Parse(false);
        }

        private void Parse(bool IsExplicit)
        {
            ClearDispatcherTimer();

            if (XamlDocument != null && !CodeCompletionPopup.IsOpenSomewhere)
            {
                if (XamlDocument.SourceText != null)
                {
                    HtmlDocument doc = ContentArea.Document;
                    bool parseSuccess = (bool)doc.InvokeScript("ParseXaml", new object[] { XamlDocument.SourceText });

                    if (parseSuccess)
                    {
                        IsValidXaml = true;
                        ErrorText = null;
                        ErrorLineNumber = 0;
                        ErrorLinePosition = 0;
                        UnhandledExceptionRaised = false;

                        if (Kaxaml.Properties.Settings.Default.EnableAutoBackup)
                        {
                            XamlDocument.SaveBackup();
                        }

                        DelayedSnapshot();
                    }
                    else
                    {

                        ErrorText = (string)doc.InvokeScript("GetLastErrorMessage", null);
                        ErrorLineNumber = int.Parse((string)doc.InvokeScript("GetLastErrorLineNumber", null));
                        ErrorLinePosition = int.Parse((string)doc.InvokeScript("GetLastErrorLinePos", null));

                        ErrorText = ErrorText.Replace("\r", "");
                        ErrorText = ErrorText.Replace("\n", "");
                        ErrorText = ErrorText.Replace("\t", "");

                        // get rid of everything after "Line" if it is in the last 30 characters 
                        int pos = ErrorText.LastIndexOf("[Line");
                        if (pos > 0 && pos > (ErrorText.Length - 50))
                        {
                            ErrorText = ErrorText.Substring(0, pos);
                        }

                        IsValidXaml = false;
                    }
                }
            }
        }

        DispatcherTimer snapTimer;
        private void DelayedSnapshot()
        {
            if (snapTimer == null)
            {
                snapTimer = new DispatcherTimer();
                snapTimer.Interval = TimeSpan.FromMilliseconds(1000);
                snapTimer.Tick += new EventHandler(snapTimer_Tick);
            }

            snapTimer.Start();
        }

        void snapTimer_Tick(object sender, EventArgs e)
        {
            snapTimer.Stop();
            this.XamlDocument.PreviewImage = RenderHelper.HwndToBitmap(ContentArea.Handle);
        }


        private void ReportError(Exception e)
        {
            IsValidXaml = false;

            if (e is XamlParseException)
            {
                XamlParseException x = (XamlParseException)e;
                ErrorLineNumber = x.LineNumber;
                ErrorLinePosition = x.LinePosition;
            }
            else
            {
                ErrorLineNumber = 0;
                ErrorLinePosition = 0;
            }

            Exception inner = e;

            while (inner.InnerException != null) inner = inner.InnerException;

            ErrorText = inner.Message;
            ErrorText = ErrorText.Replace("\r", "");
            ErrorText = ErrorText.Replace("\n", "");
            ErrorText = ErrorText.Replace("\t", "");

            // get rid of everything after "Line" if it is in the last 30 characters 
            int pos = ErrorText.LastIndexOf("Line");
            if (pos > 0 && pos > (ErrorText.Length - 50))
            {
                ErrorText = ErrorText.Substring(0, pos);
            }

        }

        private void ShowErrorUI()
        {
            ImageSource src = null;

            if (_IsInitializing)
            {
                src = XamlDocument.PreviewImage;
            }
            else
            {
                // update the error image
                ColorOverlayImage.Source = RenderHelper.HwndToBitmap(ContentArea.Handle);
                this.XamlDocument.PreviewImage = RenderHelper.HwndToGrayscaleBitmap(ContentArea.Handle);
                src = XamlDocument.PreviewImage;
            }

            Color c = Color.FromArgb(255, 216, 216, 216);

            if (src is BitmapSource)
            {
                CroppedBitmap croppedSrc = new CroppedBitmap((BitmapSource)src, new Int32Rect(0, 0, 1, 1));
                byte[] pixels = new byte[4];
                croppedSrc.CopyPixels(pixels, 4, 0);
                c = Color.FromArgb(255, pixels[0], pixels[1], pixels[2]);
            }

            ErrorOverlayImage.Source = src;
            ErrorOverlay.Background = new SolidColorBrush(c);


            ColorOverlayImage.Visibility = Visibility.Visible;
            FormsHost.Visibility = Visibility.Hidden;

            DoubleAnimation d = (DoubleAnimation)this.FindResource("ShowErrorOverlay");
            //d.Completed += new EventHandler(ErrorOverlayAnimationCompleted);
            if (d != null)
            {
                ErrorOverlay.BeginAnimation(OpacityProperty, d);
            }
        }


        private void ErrorOverlayAnimationCompleted(object sender, EventArgs e)
        {
            // once we're done fading into the "snapshot", we want to 
            // get rid of the existing content so that any really bad 
            // error (like one that is consuming memory) isn't persisted

            // ContentArea.Content = null;
        }


        private void HideErrorUI()
        {
            DoubleAnimation d = (DoubleAnimation)this.FindResource("HideErrorOverlay");
            d.Completed += new EventHandler(d_Completed);
            if (d != null)
            {
                ErrorOverlay.BeginAnimation(OpacityProperty, d);
            }
        }

        void d_Completed(object sender, EventArgs e)
        {
            ColorOverlayImage.Visibility = Visibility.Hidden;
            FormsHost.Visibility = Visibility.Visible;
        }

        #endregion

        #region IXamlDocumentView Members

        public void Parse()
        {
            ClearDispatcherTimer();
            Parse(true);
        }

        bool _IsInitializing = false;

        public void Initialize()
        {
            IsValidXaml = true;
            _IsInitializing = true;
            //ContentArea.Content = null;



            Parse();
        }

        int i = 0;
        public void OnActivate()
        {
            //ContentArea.Refresh();
            Debug.WriteLine("ACTIVATE " + (i++).ToString());
            //this.Parse();

        }


        public XamlDocument XamlDocument
        {
            get
            {
                if (this.DataContext is AgDocument)
                {
                    return (AgDocument)this.DataContext;
                }
                else
                {
                    return null;
                }
            }
        }

        public IKaxamlInfoTextEditor TextEditor
        {
            get
            {
                if (this.Editor != null && this.Editor is IKaxamlInfoTextEditor)
                {
                    return this.Editor;
                }
                return null;
            }
        }

        #endregion
    }
}
