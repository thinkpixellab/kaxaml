using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Kaxaml.CodeCompletion;
using Kaxaml.Controls;
using Kaxaml.Documents;
using Kaxaml.Properties;
using KaxamlPlugins;
using PixelLab.Common;

namespace Kaxaml.DocumentViews
{
    /// <summary>
    /// Interaction logic for WPFDocumentView.xaml
    /// </summary>

    public partial class WPFDocumentView : System.Windows.Controls.UserControl, IXamlDocumentView
    {

        #region Static Fields

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------
        private static DispatcherTimer dispatcherTimer;

        #endregion Static Fields

        #region Fields


        private bool UnhandledExceptionRaised;

        #endregion Fields

        #region Constructors

        public WPFDocumentView()
        {
            InitializeComponent();

            KaxamlInfo.Frame = ContentArea;
            ContentArea.ContentRendered += new EventHandler(ContentArea_ContentRendered);

            Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(Dispatcher_UnhandledException);

            string schemafile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(App.StartupPath + "\\"), Kaxaml.Properties.Settings.Default.WPFSchema);
            XmlCompletionDataProvider.LoadSchema(schemafile);
        }

        #endregion Constructors

        #region Event Handlers

        void ContentArea_ContentRendered(object sender, EventArgs e)
        {
            KaxamlInfo.RaiseContentLoaded();
        }

        void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // try to shut this down by killing the content and showing an
            // error page.  if it gets raised more than once between parses, then assume
            // it's fatal and shutdown the app

            if (!UnhandledExceptionRaised)
            {
                UnhandledExceptionRaised = true;
                ContentArea.Content = null;
                ReportError(e.Exception);
                e.Handled = true;
            }
            else
            {
                Application.Current.Shutdown();
                e.Handled = true;
            }
        }

        #endregion Event Handlers
        //-------------------------------------------------------------------
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
            DependencyProperty.Register("IsValidXaml", typeof(bool), typeof(WPFDocumentView), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(IsValidXamlChanged)));

        /// <summary>
        /// PropertyChangedCallback for IsValidXaml
        /// </summary>
        private static void IsValidXamlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is WPFDocumentView)
            {
                WPFDocumentView owner = (WPFDocumentView)obj;

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
            DependencyProperty.Register("ErrorText", typeof(string), typeof(WPFDocumentView), new FrameworkPropertyMetadata(default(string), new PropertyChangedCallback(ErrorTextChanged)));

        /// <summary>
        /// PropertyChangedCallback for ErrorText
        /// </summary>
        private static void ErrorTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is WPFDocumentView)
            {
                WPFDocumentView owner = (WPFDocumentView)obj;
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
            DependencyProperty.Register("ErrorLineNumber", typeof(int), typeof(WPFDocumentView), new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(ErrorLineNumberChanged)));

        /// <summary>
        /// PropertyChangedCallback for ErrorLineNumber
        /// </summary>
        private static void ErrorLineNumberChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is WPFDocumentView)
            {
                WPFDocumentView owner = (WPFDocumentView)obj;
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
            DependencyProperty.Register("ErrorLinePosition", typeof(int), typeof(WPFDocumentView), new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(ErrorLinePositionChanged)));

        /// <summary>
        /// PropertyChangedCallback for ErrorLinePosition
        /// </summary>
        private static void ErrorLinePositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is WPFDocumentView)
            {
                WPFDocumentView owner = (WPFDocumentView)obj;
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
            DependencyProperty.RegisterReadOnly("PreviewImage", typeof(ImageSource), typeof(WPFDocumentView), new UIPropertyMetadata(default(ImageSource)));
        public static readonly DependencyProperty PreviewImageProperty = PreviewImagePropertyKey.DependencyProperty;

        #endregion

        #region Scale (DependencyProperty)

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(WPFDocumentView), new FrameworkPropertyMetadata(1.0));

        #endregion        //-------------------------------------------------------------------
        //
        //  Event Handlers
        //
        //-------------------------------------------------------------------


        #region Event Handlers

        private const int WM_PRINT = 0x0317;

        private void SplitterDragStarted(object sender, RoutedEventArgs e)
        {

        }

        private void SplitterDragCompleted(object sender, RoutedEventArgs e)
        {

        }

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

        private void ErrorOverlayAnimationCompleted(object sender, EventArgs e)
        {
            // once we're done fading into the "snapshot", we want to 
            // get rid of the existing content so that any really bad 
            // error (like one that is consuming memory) isn't persisted
            ContentArea.Content = null;
        }

        private void ContentAreaRendered(object sender, EventArgs e)
        {
            try
            {
                if (IsValidXaml)
                {
                    this.XamlDocument.PreviewImage = RenderHelper.VisualToBitmap(ContentArea, (int)ContentArea.ActualWidth, (int)ContentArea.ActualHeight, null);
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

        private void LineNumberClick(object sender, RoutedEventArgs e)
        {
            Editor.SelectLine(ErrorLineNumber - 1);
            Editor.Focus();
            Editor.TextEditor.Focus();
        }

        #endregion
        //-------------------------------------------------------------------
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
                    // handle the in place preparsing (this actually updates the source in the editor)
                    int index = TextEditor.CaretIndex;
                    XamlDocument.SourceText = PreParse(XamlDocument.SourceText);
                    TextEditor.CaretIndex = index;

                    string str = XamlDocument.SourceText;

                    // handle the in memory preparsing (this happens behind the scenes all in memory)
                    str = DeSilverlight(str);


                    try
                    {
                        object content = null;
                        using (var ms = new MemoryStream(str.Length))
                        {
                            using (var sw = new StreamWriter(ms))
                            {
                                sw.Write(str);
                                sw.Flush();

                                ms.Seek(0, SeekOrigin.Begin);

                                ParserContext pc = new ParserContext();
                                pc.BaseUri = new Uri(XamlDocument.Folder != null
                                    ? XamlDocument.Folder + "/"
                                    : System.Environment.CurrentDirectory + "/");
                                //pc.BaseUri = new Uri(XamlDocument.Folder + "/");
                                //pc.BaseUri = new Uri(System.Environment.CurrentDirectory + "/");

                                ContentArea.JournalOwnership = System.Windows.Navigation.JournalOwnership.UsesParentJournal;
                                content = XamlReader.Load(ms, pc);
                            }
                        }

                        if (content != null && content is Window)
                        {
                            var window = (Window)content;
                            window.Owner = Application.Current.MainWindow;

                            if (!IsExplicit)
                            {
                                Border bd = new Border()
                                {
                                    Background = Color.FromRgb(0x40, 0x40, 0x40).ToCachedBrush()
                                };

                                TextBlock tb = new TextBlock()
                                {
                                    FontFamily = new FontFamily("Segoe, Segoe UI, Verdana"),
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 14,
                                    Foreground = Brushes.White,
                                    MaxWidth = 320,
                                    Margin = new Thickness(50),
                                    VerticalAlignment = VerticalAlignment.Center,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Text = "The root element of this content is of type Window.  Press F5 to show the content in a new window."
                                };

                                bd.Child = tb;
                                ContentArea.Content = bd;
                            }
                            else
                            {
                                window.Show();
                            }
                        }
                        else
                        {
                            ContentArea.Content = content;
                        }

                        IsValidXaml = true;
                        ErrorText = null;
                        ErrorLineNumber = 0;
                        ErrorLinePosition = 0;
                        UnhandledExceptionRaised = false;

                        if (Kaxaml.Properties.Settings.Default.EnableAutoBackup)
                        {
                            XamlDocument.SaveBackup();
                            //if (!Editor.Text.Equals(DefaultXaml))
                            //{
                            //    File.WriteAllText(XamlDocument.BackupPath, Editor.Text);
                            //}
                        }
                    }

                    catch (Exception ex)
                    {
                        if (ex.IsCriticalException())
                        {
                            throw;
                        }
                        else
                        {
                            ReportError(ex);
                        }
                    }
                }
            }
        }

        private string DeSilverlight(string str)
        {
            if (Kaxaml.Properties.Settings.Default.EnablePseudoSilverlight)
            {
                str = str.Replace("http://schemas.microsoft.com/client/2007", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            }

            return str;
        }

        private string PreParse(string str)
        {
            while (str.Contains("?COLOR"))
            {
                str = ReplaceOnce(str, "?COLOR", GetRandomColor().ToString());
            }

            while (str.Contains("?NAMEDCOLOR"))
            {
                str = ReplaceOnce(str, "?NAMEDCOLOR", GetRandomColorName().ToString());
            }

            return str;
        }

        private string ReplaceOnce(string str, string oldValue, string newValue)
        {
            int index = str.IndexOf(oldValue);
            string s = str;

            s = s.Remove(index, oldValue.Length);
            s = s.Insert(index, newValue);

            return s;
        }

        Random R = new Random();

        private Color GetRandomColor()
        {
            return Color.FromRgb((byte)R.Next(0, 255), (byte)R.Next(0, 255), (byte)R.Next(0, 255));
        }

        private string GetRandomColorName()
        {
            string[] colors = new string[] { "AliceBlue", "Aquamarine", "Azure", "Bisque", "BlanchedAlmond", "Burlywood", "CadetBlue", "Chartreuse", "Chocolate", "Coral", "CornflowerBlue", "Cornsilk", "DodgerBlue", "FloralWhite", "Gainsboro", "Ghostwhite", "Honeydew", "HotPink", "IndianRed", "LightSalmon", "Mintcream", "MistyRose", "Moccasin", "NavajoWhite", "Oldlace", "PapayaWhip", "PeachPuff", "Peru", "SaddleBrown", "Seashell", "Thistle", "Tomato", "WhiteSmoke" };
            return colors[R.Next(0, colors.Length - 1)];
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
                src = RenderHelper.ElementToGrayscaleBitmap(ContentArea);
                XamlDocument.PreviewImage = src;
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

            DoubleAnimation d = (DoubleAnimation)this.FindResource("ShowErrorOverlay");
            d.Completed += new EventHandler(ErrorOverlayAnimationCompleted);
            if (d != null)
            {
                ErrorOverlay.BeginAnimation(OpacityProperty, d);
            }
        }

        private void HideErrorUI()
        {
            DoubleAnimation d = (DoubleAnimation)this.FindResource("HideErrorOverlay");
            if (d != null)
            {
                ErrorOverlay.BeginAnimation(OpacityProperty, d);
            }
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
            ContentArea.Content = null;

            Parse();
        }

        public void OnActivate()
        {
            KaxamlInfo.Frame = this.ContentArea;
        }

        public XamlDocument XamlDocument
        {
            get
            {
                if (this.DataContext is WpfDocument)
                {
                    return (WpfDocument)this.DataContext;
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