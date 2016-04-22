using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kaxaml.Documents;
using Kaxaml.Plugins.Default;
using KaxamlPlugins;
using Microsoft.Win32;
using PixelLab.Common;
using System.Windows.Markup;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace Kaxaml
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------


        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            KaxamlInfo.MainWindow = this;

            // initialize commands

            CommandBinding binding = new CommandBinding(ParseCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Parse_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Parse_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.F5)));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(NewWPFTabCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.NewWPFTab_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.NewWPFTab_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.T, ModifierKeys.Control, "Ctrl+T")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(NewAgTabCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.NewAgTab_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.NewAgTab_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.L, ModifierKeys.Control, "Ctrl+L")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(CloseTabCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.CloseTab_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.CloseTab_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.W, ModifierKeys.Control, "Ctrl+W")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(SaveCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Save_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Save_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(SaveAsCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.SaveAs_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.SaveAs_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt, "Ctrl+Alt+S")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(OpenCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Open_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Open_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(SaveAsImageCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.SaveAsImage_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.SaveAsImage_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.I, ModifierKeys.Control, "Ctrl+I")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(ExitCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Exit_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Exit_CanExecute);
            //this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.O, ModifierKeys.Control, "Ctrl+O")));
            this.CommandBindings.Add(binding);

            // Zoom Commands

            binding = new CommandBinding(ZoomInCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.ZoomIn_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.ZoomIn_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl++")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(ZoomOutCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.ZoomOut_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.ZoomOut_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl+-")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(ActualSizeCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.ActualSize_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.ActualSize_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.D1, ModifierKeys.Control, "Ctrl+1")));
            this.CommandBindings.Add(binding);

            // Edit Commands 
            // We have an unusual situation here where we need to handle Copy/Paste/etc. from the menu
            // separately from the built in keyboard commands that you have in control itself.  This
            // is because of some difficulty in getting commands to go accross the WPF/WinForms barrier.
            // One artifact of this is the fact that we want to create the commands without InputGestures
            // because the WinForms controls will handle the keyboards stuff--so this is for Menu only.

            binding = new CommandBinding(CopyCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Copy_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Copy_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(CutCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Cut_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Cut_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(PasteCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Paste_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Paste_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(PasteImageCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.PasteImage_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.PasteImage_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+V")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(DeleteCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Delete_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Delete_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(UndoCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Undo_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Undo_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(RedoCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Redo_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Redo_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(FindCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Find_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Find_CanExecute);
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(FindNextCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.FindNext_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.FindNext_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.F3, ModifierKeys.None, "F3")));
            this.CommandBindings.Add(binding);

            binding = new CommandBinding(ReplaceCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Replace_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Replace_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.H, ModifierKeys.Control, "F3")));
            this.CommandBindings.Add(binding);

            this.PreviewKeyDown += new KeyEventHandler(MainWindow_PreviewKeyDown);

            // load or create startup documents

            ParseArgs(App.StartupArgs);

            InitAssemblyResolve();

            AddResources(App.Current);

            if (XamlDocuments.Count == 0)
            {
                WpfDocument doc = new WpfDocument(System.IO.Directory.GetCurrentDirectory());
                XamlDocuments.Add(doc);
            }

        }

        private void ParseArgs(string[] args)
        {
            if (args == null)
                return;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg == "-i")
                {
                    string nextArg = (i < args.Length - 1) ? args[i + 1] : null;
                    if (nextArg != null && System.IO.File.Exists(nextArg))
                    {
                        try
                        {
                            if (nextArg.EndsWith(".xaml", StringComparison.InvariantCultureIgnoreCase))
                            {
                                XamlResources.Add(nextArg);
                            }
                            else if (nextArg.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Assembly.LoadFile(nextArg);

                                string dir = System.IO.Path.GetDirectoryName(nextArg);
                                AssemblySearchDirs.Add(dir);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Debug.Fail("Could not load: " + args + " " + nextArg);
                        }
                    }

                    i++;
                    continue;
                }

                if (System.IO.File.Exists(arg))
                {
                    XamlDocument doc = XamlDocument.FromFile(arg);
                    XamlDocuments.Add(doc);
                }
            }
        }

        private void InitAssemblyResolve()
        {
            if (AssemblySearchDirs.Count > 0)
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += AssemblyResolveEventHandler;
            }
        }

        private static Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            string[] fields = args.Name.Split(',');
            string assemblyName = fields[0];
            string assemblyCulture;
            if (fields.Length < 2)
                assemblyCulture = null;
            else
                assemblyCulture = fields[2].Substring(fields[2].IndexOf('=') + 1);
            // Do the search
            string assemblyFilePath = null;
            foreach (string directory in AssemblySearchDirs)
            {
                string path = System.IO.Path.Combine(directory, assemblyName + ".dll");
                if (System.IO.File.Exists(path))
                {
                    assemblyFilePath = path;
                    break;
                }
            }
            // Load the assembly from the specified path
            Assembly assembly = null;
            if (!string.IsNullOrEmpty(assemblyFilePath))
                assembly = Assembly.LoadFrom(assemblyFilePath);
            return assembly;
        }

        private static readonly List<string> AssemblySearchDirs = new List<string>();
        private static readonly List<string> XamlResources = new List<string>();

        public static void AddResources(object element)
        {
            foreach (string filePath in XamlResources)
            {
                ResourceDictionary resDict = XamlReader.Load(System.IO.File.Open(filePath, System.IO.FileMode.Open)) as ResourceDictionary;
                if (resDict != null)
                {
                    if (element is FrameworkElement)
                    {
                        FrameworkElement fe = element as FrameworkElement;
                        fe.Resources.MergedDictionaries.Add(resDict);
                    }
                    else if (element is Application)
                    {
                        Application app = element as Application;
                        app.Resources.MergedDictionaries.Add(resDict);
                    }
                }
            }
        }

        void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            PluginView.OpenPlugin(e.Key, Keyboard.Modifiers);
        }

        #endregion        //-------------------------------------------------------------------
        //
        //  Dependency Properties
        //
        //-------------------------------------------------------------------


        #region XamlDocuments (DependencyProperty)

        /// <summary>
        /// The collection of XamlDocuments that are currently actively being edited.
        /// </summary>
        public ObservableCollection<XamlDocument> XamlDocuments
        {
            get { return (ObservableCollection<XamlDocument>)GetValue(XamlDocumentsProperty); }
            set { SetValue(XamlDocumentsProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for XamlDocuments
        /// </summary>
        public static readonly DependencyProperty XamlDocumentsProperty =
            DependencyProperty.Register("XamlDocuments", typeof(ObservableCollection<XamlDocument>), typeof(MainWindow), new FrameworkPropertyMetadata(new ObservableCollection<XamlDocument>(), new PropertyChangedCallback(XamlDocumentsChanged)));

        /// <summary>
        /// PropertyChangedCallback for XamlDocuments
        /// </summary>
        private static void XamlDocumentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is MainWindow)
            {
                MainWindow owner = (MainWindow)obj;
                // handle changed event here
            }
        }

        #endregion        //-------------------------------------------------------------------
        //
        //  Commands
        //
        //-------------------------------------------------------------------


        #region ParseCommand

        public readonly static RoutedUICommand ParseCommand = new RoutedUICommand("_Parse", "ParseCommand", typeof(MainWindow));

        void Parse_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                if (this.DocumentsView.SelectedView != null)
                {
                    this.DocumentsView.SelectedView.Parse();
                }
            }
        }

        void Parse_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                if (this.DocumentsView.SelectedView != null)
                {
                    args.CanExecute = true;
                }
                else
                {
                    args.CanExecute = false;
                }
            }
        }

        #endregion

        #region NewWPFTabCommand

        public readonly static RoutedUICommand NewWPFTabCommand = new RoutedUICommand("New WPF Tab", "NewWPFTabCommand", typeof(MainWindow));

        void NewWPFTab_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                WpfDocument doc = new WpfDocument(System.IO.Directory.GetCurrentDirectory());
                XamlDocuments.Add(doc);

                this.DocumentsView.SelectedDocument = doc;

            }
        }

        void NewWPFTab_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region NewAgTabCommand

        public readonly static RoutedUICommand NewAgTabCommand = new RoutedUICommand("New Silverlight Tab", "NewAgTabCommand", typeof(MainWindow));

        void NewAgTab_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                AgDocument doc = new AgDocument(System.IO.Directory.GetCurrentDirectory());
                XamlDocuments.Add(doc);

                this.DocumentsView.SelectedDocument = doc;

            }
        }

        void NewAgTab_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region CloseTabCommand

        public readonly static RoutedUICommand CloseTabCommand = new RoutedUICommand("Close Tab", "CloseTabCommand", typeof(MainWindow));

        void CloseTab_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                XamlDocument document = null;

                if (args.Parameter != null)
                {
                    document = args.Parameter as XamlDocument;
                }
                else if (this.DocumentsView.SelectedView != null)
                {
                    document = this.DocumentsView.SelectedView.XamlDocument;
                }

                if (document != null)
                {
                    if (document.NeedsSave)
                    {
                        MessageBoxResult result = MessageBox.Show("The document " + document.Filename + " has not been saved. Would you like to save it before closing?", "Save Document", MessageBoxButton.YesNoCancel);

                        if (result == MessageBoxResult.Yes) document.Save();
                        if (result == MessageBoxResult.Cancel) return;
                    }

                    this.DocumentsView.XamlDocuments.Remove(document);
                }
            }
        }

        void CloseTab_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                if (this.DocumentsView.XamlDocuments.Count > 1)
                {
                    args.CanExecute = true;
                }
                else
                {
                    args.CanExecute = false;
                }
            }
        }


        #endregion

        #region SaveCommand

        public readonly static RoutedUICommand SaveCommand = new RoutedUICommand("_Save", "SaveCommand", typeof(MainWindow));

        void Save_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                if (this.DocumentsView.SelectedView != null)
                {
                    XamlDocument document = this.DocumentsView.SelectedView.XamlDocument;
                    Save(document);
                }
            }
        }

        void Save_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                if (this.DocumentsView.SelectedView != null)
                {
                    args.CanExecute = true;
                }
                else
                {
                    args.CanExecute = false;
                }
            }
        }

        #endregion

        #region SaveAsCommand

        public readonly static RoutedUICommand SaveAsCommand = new RoutedUICommand("Save As... ", "SaveAsCommand", typeof(MainWindow));

        void SaveAs_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                if (this.DocumentsView.SelectedView != null)
                {
                    XamlDocument document = this.DocumentsView.SelectedView.XamlDocument;
                    SaveAs(document);
                }
            }
        }

        void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                if (this.DocumentsView.SelectedView != null)
                {
                    args.CanExecute = true;
                }
                else
                {
                    args.CanExecute = false;
                }
            }
        }

        #endregion

        #region SaveAsImageCommand

        public readonly static RoutedUICommand SaveAsImageCommand = new RoutedUICommand("_SaveAsImage", "SaveAsImageCommand", typeof(MainWindow));

        void SaveAsImage_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.Filter = "Image files (*.png)|*.png|All files (*.*)|*.*";

                if (sfd.ShowDialog(KaxamlInfo.MainWindow) == true)
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                    {
                        BitmapSource rtb = RenderContent();
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(rtb));
                        encoder.Save(fs);
                    }
                }
            }
        }

        void SaveAsImage_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region OpenCommand

        public readonly static RoutedUICommand OpenCommand = new RoutedUICommand("_Open", "OpenCommand", typeof(MainWindow));

        void Open_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.Open();
            }
        }

        void Open_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region ExitCommand

        public readonly static RoutedUICommand ExitCommand = new RoutedUICommand("_Exit", "ExitCommand", typeof(MainWindow));

        void Exit_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                Application.Current.Shutdown();
            }
        }

        void Exit_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region ZoomInCommand

        public readonly static RoutedUICommand ZoomInCommand = new RoutedUICommand("Zoom In", "ZoomInCommand", typeof(MainWindow));

        void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.StatusView.ZoomIn();
            }
        }

        void ZoomIn_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region ZoomOutCommand

        public readonly static RoutedUICommand ZoomOutCommand = new RoutedUICommand("Zoom Out", "ZoomOutCommand", typeof(MainWindow));

        void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.StatusView.ZoomOut();
            }
        }

        void ZoomOut_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region ActualSizeCommand

        public readonly static RoutedUICommand ActualSizeCommand = new RoutedUICommand("Zoom to Actual Size", "ActualSizeCommand", typeof(MainWindow));

        void ActualSize_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.StatusView.ActualSize();
            }
        }

        void ActualSize_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region PasteCommand

        public readonly static RoutedUICommand PasteCommand = new RoutedUICommand("Paste", "PasteCommand", typeof(MainWindow));

        void Paste_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.DocumentsView.SelectedView.TextEditor.InsertStringAtCaret(Clipboard.GetText());
            }
        }

        void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = Clipboard.ContainsText();
            }
        }

        #endregion

        #region PasteImageCommand

        public readonly static RoutedUICommand PasteImageCommand = new RoutedUICommand("Paste _Image", "PasteImageCommand", typeof(MainWindow));

        void PasteImage_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                if (Clipboard.ContainsImage())
                {
                    BitmapSource src = Clipboard.GetImage();

                    if (src != null)
                    {
                        try
                        {
                            // find and/or create the folder

                            string folder = Kaxaml.Properties.Settings.Default.PasteImageFolder;
                            string absfolder = "";

                            if (!folder.Contains(":"))
                            {
                                absfolder = System.IO.Path.Combine(DocumentsView.SelectedDocument.Folder, folder);
                            }
                            else
                            {
                                absfolder = folder;
                            }

                            // create the folder if it doesn't exist

                            if (!System.IO.Directory.Exists(absfolder))
                            {
                                System.IO.Directory.CreateDirectory(absfolder);
                            }

                            // create a unique filename

                            string filename = Kaxaml.Properties.Settings.Default.PasteImageFile;
                            string tempfile = System.IO.Path.Combine(absfolder, filename);
                            int number = 1;

                            string absfilename = tempfile.Replace("$number$", number.ToString());
                            while (System.IO.File.Exists(absfilename))
                            {
                                number++;
                                absfilename = tempfile.Replace("$number$", number.ToString());
                            }

                            // save the image from the clipboard

                            using (System.IO.FileStream fs = new System.IO.FileStream(absfilename, System.IO.FileMode.Create))
                            {
                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                encoder.QualityLevel = 100;

                                //PngBitmapEncoder encoder = new PngBitmapEncoder();
                                //encoder.Interlace = PngInterlaceOption.Off;
                                encoder.Frames.Add(BitmapFrame.Create(src));
                                encoder.Save(fs);
                            }

                            // get a relative version of the file name

                            string rfilename = absfilename.Replace(DocumentsView.SelectedDocument.Folder + "\\", "");

                            // create and insert the xaml

                            string xaml = Kaxaml.Properties.Settings.Default.PasteImageXaml;
                            xaml = xaml.Replace("$source$", rfilename);
                            this.DocumentsView.SelectedView.TextEditor.InsertStringAtCaret(xaml);
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
            }
        }

        void PasteImage_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = Clipboard.ContainsImage();
            }
        }

        #endregion

        #region CopyCommand

        public readonly static RoutedUICommand CopyCommand = new RoutedUICommand("_Copy", "CopyCommand", typeof(MainWindow));

        void Copy_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                Clipboard.SetText(this.DocumentsView.SelectedView.TextEditor.SelectedText);
            }
        }

        void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = !string.IsNullOrEmpty(this.DocumentsView.SelectedView.TextEditor.SelectedText);
            }
        }

        #endregion

        #region CutCommand

        public readonly static RoutedUICommand CutCommand = new RoutedUICommand("Cu_t", "CutCommand", typeof(MainWindow));

        void Cut_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                Clipboard.SetText(this.DocumentsView.SelectedView.TextEditor.SelectedText);
                this.DocumentsView.SelectedView.TextEditor.ReplaceSelectedText("");
            }
        }

        void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = !string.IsNullOrEmpty(this.DocumentsView.SelectedView.TextEditor.SelectedText);
            }
        }

        #endregion

        #region DeleteCommand

        public readonly static RoutedUICommand DeleteCommand = new RoutedUICommand("_Delete", "DeleteCommand", typeof(MainWindow));

        void Delete_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.DocumentsView.SelectedView.TextEditor.ReplaceSelectedText("");
            }
        }

        void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = !string.IsNullOrEmpty(this.DocumentsView.SelectedView.TextEditor.SelectedText);
            }
        }

        #endregion

        #region RedoCommand

        public readonly static RoutedUICommand RedoCommand = new RoutedUICommand("_Redo", "RedoCommand", typeof(MainWindow));

        void Redo_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.DocumentsView.SelectedView.TextEditor.Redo();
            }
        }

        void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region UndoCommand

        public readonly static RoutedUICommand UndoCommand = new RoutedUICommand("_Undo", "UndoCommand", typeof(MainWindow));

        void Undo_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.DocumentsView.SelectedView.TextEditor.Undo();
            }
        }

        void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region FindCommand

        public readonly static RoutedUICommand FindCommand = new RoutedUICommand("_Find", "FindCommand", typeof(MainWindow));

        void Find_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.PluginView.SelectedPlugin = this.PluginView.GetFindPlugin();
            }
        }

        void Find_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region FindNextCommand

        public readonly static RoutedUICommand FindNextCommand = new RoutedUICommand("_FindNext", "FindNextCommand", typeof(MainWindow));

        void FindNext_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                Plugin findPlugin = this.PluginView.GetFindPlugin();
                if (findPlugin.Root is Find)
                {
                    string findText = (findPlugin.Root as Find).FindText.Text;
                    KaxamlInfo.Editor.Find(findText);
                }
            }
        }

        void FindNext_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion

        #region ReplaceCommand

        public readonly static RoutedUICommand ReplaceCommand = new RoutedUICommand("_Replace", "ReplaceCommand", typeof(MainWindow));

        void Replace_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.PluginView.SelectedPlugin = this.PluginView.GetFindPlugin();
            }
        }

        void Replace_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion        //-------------------------------------------------------------------
        //
        //  Methods
        //
        //-------------------------------------------------------------------


        #region Public Methods


        OpenFileDialog _OpenDialog;

        public bool Open()
        {
            if (_OpenDialog == null)
            {
                _OpenDialog = new OpenFileDialog();
                _OpenDialog.AddExtension = true;
                _OpenDialog.DefaultExt = ".xaml";
                _OpenDialog.Filter = "XAML files|*.xaml|Backup files|*.backup|All files|*.*";
                _OpenDialog.Multiselect = true;
                _OpenDialog.CheckFileExists = true;
                _OpenDialog.CheckPathExists = true;
                _OpenDialog.RestoreDirectory = true;
            }

            if ((bool)_OpenDialog.ShowDialog())
            {
                XamlDocument first = null;

                foreach (string s in _OpenDialog.FileNames)
                {
                    XamlDocument doc = XamlDocument.FromFile(s);

                    if (doc != null)
                    {
                        DocumentsView.XamlDocuments.Add(doc);
                        if (first == null) first = doc;
                    }
                }

                DocumentsView.SelectedDocument = first;

                return true;
            }

            return false;
        }

        SaveFileDialog _SaveDialog;

        public bool Save(XamlDocument document)
        {
            if (document.UsingTemporaryFilename)
            {
                return SaveAs(document);
            }
            else
            {
                return document.Save();
            }
        }

        public bool SaveAs(XamlDocument document)
        {
            if (_SaveDialog == null)
            {
                _SaveDialog = new SaveFileDialog();
                _SaveDialog.AddExtension = true;
                _SaveDialog.DefaultExt = ".xaml";
                _SaveDialog.Filter = "XAML file|*.xaml|All files|*.*";
            }

            _SaveDialog.FileName = document.Filename;

            if ((bool)_SaveDialog.ShowDialog())
            {
                if (document.SaveAs(_SaveDialog.FileName))
                {
                    return true;
                }

                MessageBox.Show("The file could not be saved as " + _SaveDialog.FileName + ".");
                return false;
            }

            return false;
        }

        public void Close(XamlDocument document)
        {

        }

        #endregion

        #region Private Methods

        private BitmapSource RenderContent()
        {
            FrameworkElement element = null;

            if (KaxamlInfo.Frame.Content is FrameworkElement)
            {
                element = KaxamlInfo.Frame.Content as FrameworkElement;
            }
            else
            {
                element = KaxamlInfo.Frame;
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(element);

            return rtb;
        }

        #endregion

        #region Overrides

        protected override void OnDrop(DragEventArgs e)
        {
            string[] filenames = (string[])e.Data.GetData("FileDrop", true);

            if ((null != filenames) &&
                (filenames.Length > 0))
            {
                XamlDocument first = null;
                foreach (string f in filenames)
                {
                    string ext = System.IO.Path.GetExtension(f).ToLower();
                    if (ext.Equals(".png") || ext.Equals(".jpg") || ext.Equals(".jpeg") || ext.Equals(".bmp") || ext.Equals(".gif"))
                    {
                        // get a relative version of the file name
                        string rfilename = f.Replace(DocumentsView.SelectedDocument.Folder + "\\", "");

                        // create and insert the xaml
                        string xaml = Kaxaml.Properties.Settings.Default.PasteImageXaml;
                        xaml = xaml.Replace("$source$", rfilename);
                        this.DocumentsView.SelectedView.TextEditor.InsertStringAtCaret(xaml);
                    }
                    else
                    {
                        XamlDocument doc = XamlDocument.FromFile(f);

                        if (doc != null)
                        {
                            DocumentsView.XamlDocuments.Add(doc);
                            if (first == null) first = doc;
                        }
                    }
                }

                if (first != null)
                {
                    DocumentsView.SelectedDocument = first;
                }
            }
        }

        #endregion
    }

    public class WindowTitleConverter : IMultiValueConverter
    {


        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                if (values[0] is string && values[1] is bool)
                {

                    string title = (string)values[0];
                    if ((bool)values[1]) title = title + "*";
                    title = title + " - Kaxaml";

                    return title;
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {

            return null;
        }

        #endregion
    }
}