using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using Kaxaml.CodeCompletion;
using Kaxaml.Plugins.Default;
using KaxamlPlugins;

namespace Kaxaml.Controls
{
    /// <summary>
    /// Interaction logic for KaxamlTextEditor.xaml
    /// </summary>

    public partial class KaxamlTextEditor : System.Windows.Controls.UserControl, IKaxamlInfoTextEditor
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors

        public static int counter = 0;

        public KaxamlTextEditor()
        {
            counter++;
            Debug.WriteLine("counter: " + counter);

            // enable the system theme for WinForms controls
            System.Windows.Forms.Application.EnableVisualStyles();

            InitializeComponent();

            // capture text changed events from the editor
            TextEditor.Document.DocumentChanged += new ICSharpCode.TextEditor.Document.DocumentEventHandler(TextEditorDocumentChanged);

            // create a key handler that we will use to activate code completion
            TextEditor.ActiveTextAreaControl.TextArea.KeyEventHandler += new ICSharpCode.TextEditor.KeyEventHandler(ProcessText);

            // register to process keys for our code completion dialog
            TextEditor.ActiveTextAreaControl.TextArea.DoProcessDialogKey += new DialogKeyProcessor(ProcessKeys);

            // register to get an event when selection changed
            TextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectionChanged += new EventHandler(SelectionManager_SelectionChanged);

            // register to get an event when the carent position changed
            TextEditor.ActiveTextAreaControl.Caret.PositionChanged += new EventHandler(Caret_PositionChanged);

            // register for an event when the WinFormsHost gets deactivated
            //FormsHost.MessageHook += new System.Windows.Interop.HwndSourceHook(FormsHost_MessageHook);

            // register the ShowSnippets command
            CommandBinding binding = new CommandBinding(ShowSnippetsCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.ShowSnippets_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.ShowSnippets_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.Down, ModifierKeys.Alt)));
            this.CommandBindings.Add(binding);

        }


        //IntPtr FormsHost_MessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{

        //}

        void Caret_PositionChanged(object sender, EventArgs e)
        {
            this.LineNumber = TextEditor.ActiveTextAreaControl.Caret.Position.Y;
            this.LinePosition = TextEditor.ActiveTextAreaControl.Caret.Position.X;
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Dependency Properties
        //
        //-------------------------------------------------------------------

        #region LineNumber (DependencyProperty)

        /// <summary>
        /// The current current line number of the caret.
        /// </summary>
        public int LineNumber
        {
            get { return (int)GetValue(LineNumberProperty); }
            set { SetValue(LineNumberProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for LineNumber
        /// </summary>
        public static readonly DependencyProperty LineNumberProperty =
            DependencyProperty.Register("LineNumber", typeof(int), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(LineNumberChanged)));

        /// <summary>
        /// PropertyChangedCallback for LineNumber
        /// </summary>
        private static void LineNumberChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;
                owner.TextEditor.ActiveTextAreaControl.Caret.Line = (int)args.NewValue;
            }
        }
        #endregion

        #region LinePosition (DependencyProperty)

        /// <summary>
        /// The current current line position of the caret.
        /// </summary>
        public int LinePosition
        {
            get { return (int)GetValue(LinePositionProperty); }
            set { SetValue(LinePositionProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for LinePosition
        /// </summary>
        public static readonly DependencyProperty LinePositionProperty =
            DependencyProperty.Register("LinePosition", typeof(int), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(LinePositionChanged)));

        /// <summary>
        /// PropertyChangedCallback for LinePosition
        /// </summary>
        private static void LinePositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;
                owner.TextEditor.ActiveTextAreaControl.Caret.Position = new System.Drawing.Point((int)args.NewValue, owner.TextEditor.ActiveTextAreaControl.Caret.Position.Y);
            }
        }

        #endregion

        #region ShowLineNumbers (DependencyProperty)

        /// <summary>
        /// description of ShowLineNumbers
        /// </summary>
        public bool ShowLineNumbers
        {
            get { return (bool)GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for ShowLineNumbers
        /// </summary>
        public static readonly DependencyProperty ShowLineNumbersProperty =
            DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(default(bool), new PropertyChangedCallback(ShowLineNumbersChanged)));

        /// <summary>
        /// PropertyChangedCallback for ShowLineNumbers
        /// </summary>
        private static void ShowLineNumbersChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;
                owner.TextEditor.ShowLineNumbers = (bool)args.NewValue;
            }
        }

        #endregion

        #region FontFamily (DependencyProperty)

        /// <summary>
        /// The FontFamily associated with text in the TextEditor.
        /// </summary>
        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for FontFamily
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(string), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata("Courier New", new PropertyChangedCallback(FontFamilyChanged)));

        /// <summary>
        /// PropertyChangedCallback for FontFamily
        /// </summary>
        private static void FontFamilyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;
                owner.TextEditor.Font = new System.Drawing.Font((string)args.NewValue, (float)owner.FontSize);
            }
        }

        #endregion

        #region FontSize (DependencyProperty)

        /// <summary>
        /// The size of the text in the TextEditor.
        /// </summary>
        public float FontSize
        {
            get { return (float)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for FontSize
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(float), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata((float)1, new PropertyChangedCallback(FontSizeChanged)));

        /// <summary>
        /// PropertyChangedCallback for FontSize
        /// </summary>
        private static void FontSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;
                owner.TextEditor.Font = new System.Drawing.Font(owner.FontFamily, (float)args.NewValue);
            }
        }

        #endregion

        #region Text (DependencyProperty)

        /// <summary>
        /// description of Text
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for Text
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(default(string), new PropertyChangedCallback(TextPropertyChanged)));

        /// <summary>
        /// PropertyChangedCallback for Text
        /// </summary>
        private static void TextPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;

                string newValue = " ";
                if (!String.IsNullOrEmpty((string)args.NewValue)) newValue = (string)args.NewValue;

                if (!owner._SetTextInternal)
                {
                    owner._ResetTextInternal = true;
                    owner.TextEditor.ResetText();
                    owner.TextEditor.Refresh();
                    owner.TextEditor.Text = newValue;
                    owner._ResetTextInternal = false;
                }

                if (!owner._ResetTextInternal)
                {
                    owner.RaiseTextChangedEvent(newValue);
                }
            }
        }

        #endregion

        #region IsCodeCompletionEnabled (DependencyProperty)

        /// <summary>
        /// description of the property
        /// </summary>
        public bool IsCodeCompletionEnabled
        {
            get { return (bool)GetValue(IsCodeCompletionEnabledProperty); }
            set { SetValue(IsCodeCompletionEnabledProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for IsCodeCompletionEnabled
        /// </summary>
        public static readonly DependencyProperty IsCodeCompletionEnabledProperty =
            DependencyProperty.Register("IsCodeCompletionEnabled", typeof(bool), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(true));

        #endregion

        #region ConvertTabs (DependencyProperty)

        /// <summary>
        /// If true then tabs will be converted to spaces.
        /// </summary>
        public bool ConvertTabs
        {
            get { return (bool)GetValue(ConvertTabsProperty); }
            set { SetValue(ConvertTabsProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for ConvertTabs
        /// </summary>
        public static readonly DependencyProperty ConvertTabsProperty =
            DependencyProperty.Register("ConvertTabs", typeof(bool), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(default(bool), new PropertyChangedCallback(ConvertTabsChanged)));

        /// <summary>
        /// PropertyChangedCallback for ConvertTabs
        /// </summary>
        private static void ConvertTabsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;
                owner.TextEditor.ConvertTabsToSpaces = (bool)args.NewValue;
            }
        }

        #endregion

        #region EnableXmlFolding (DependencyProperty)

        DispatcherTimer FoldingTimer = null;

        /// <summary>
        /// Enabled XML nodes to be collapsed when true
        /// </summary>
        public bool EnableXmlFolding
        {
            get { return (bool)GetValue(EnableXmlFoldingProperty); }
            set { SetValue(EnableXmlFoldingProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for EnableXmlFolding
        /// </summary>
        public static readonly DependencyProperty EnableXmlFoldingProperty =
            DependencyProperty.Register("EnableXmlFolding", typeof(bool), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(default(bool), new PropertyChangedCallback(EnableXmlFoldingChanged)));

        /// <summary>
        /// PropertyChangedCallback for EnableXmlFolding
        /// </summary>
        private static void EnableXmlFoldingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;

                if ((bool)args.NewValue)
                {
                    // set the folding strategy
                    if (owner.TextEditor.Document.FoldingManager.FoldingStrategy == null)
                    {
                        owner.TextEditor.Document.FoldingManager.FoldingStrategy = new XmlFoldingStrategy();
                    }

                    // create a timer to update the folding every second
                    if (owner.FoldingTimer == null)
                    {
                        owner.FoldingTimer = new DispatcherTimer();
                        owner.FoldingTimer.Interval = TimeSpan.FromMilliseconds(1000);
                        owner.FoldingTimer.Tick += new EventHandler(owner.FoldingTimerTick);
                    }

                    // enable folding and start the timer
                    owner.TextEditor.EnableFolding = true;
                    owner.FoldingTimer.Start();
                }
                else
                {
                    // disable folding and start the timer
                    owner.TextEditor.EnableFolding = false;
                    owner.FoldingTimer.Stop();
                }
            }
        }

        #endregion

        #region EnableSyntaxHighlighting (DependencyProperty)

        /// <summary>
        /// Enables syntax highlighting when true.
        /// </summary>
        public bool EnableSyntaxHighlighting
        {
            get { return (bool)GetValue(EnableSyntaxHighlightingProperty); }
            set { SetValue(EnableSyntaxHighlightingProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for EnableSyntaxHighlighting
        /// </summary>
        public static readonly DependencyProperty EnableSyntaxHighlightingProperty =
            DependencyProperty.Register("EnableSyntaxHighlighting", typeof(bool), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(default(bool), new PropertyChangedCallback(EnableSyntaxHighlightingChanged)));

        /// <summary>
        /// PropertyChangedCallback for EnableSyntaxHighlighting
        /// </summary>
        private static void EnableSyntaxHighlightingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;
                if ((bool)args.NewValue)
                {
                    // set the highlighting strategy
                    owner.TextEditor.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("XML");
                }
                else
                {
                    owner.TextEditor.Document.HighlightingStrategy = HighlightingManager.Manager.FindHighlighter("None");
                }
            }
        }

        #endregion

        #region ConvertTabsCount (DependencyProperty)

        /// <summary>
        /// The width of a tab in spaces.
        /// </summary>
        public int ConvertTabsCount
        {
            get { return (int)GetValue(ConvertTabsCountProperty); }
            set { SetValue(ConvertTabsCountProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for ConvertTabsCount
        /// </summary>
        public static readonly DependencyProperty ConvertTabsCountProperty =
            DependencyProperty.Register("ConvertTabsCount", typeof(int), typeof(KaxamlTextEditor), new FrameworkPropertyMetadata(default(int), new PropertyChangedCallback(ConvertTabsCountChanged)));

        /// <summary>
        /// PropertyChangedCallback for ConvertTabsCount
        /// </summary>
        private static void ConvertTabsCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is KaxamlTextEditor)
            {
                KaxamlTextEditor owner = (KaxamlTextEditor)obj;
                owner.TextEditor.TabIndent = (int)args.NewValue;
                owner.TextEditor.TextEditorProperties.IndentationSize = (int)args.NewValue; ;
            }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Commands
        //
        //-------------------------------------------------------------------

        #region Commands

        #region ShowSnippetsCommand

        public readonly static RoutedUICommand ShowSnippetsCommand = new RoutedUICommand("Show Snippets", "ShowSnippetsCommand", typeof(KaxamlTextEditor));

        void ShowSnippets_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                ShowCompletionWindow(char.MaxValue);
            }
        }

        void ShowSnippets_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
            else
            {
                args.CanExecute = false;
            }
        }

        #endregion

        #endregion

        //-------------------------------------------------------------------
        //
        //  Properties
        //
        //-------------------------------------------------------------------

        #region Properties

        public string[] Lines
        {
            get
            {
                return Text.Split(new string[4] { "\r", "\n", "\r\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public int CaretIndex
        {
            get
            {
                return TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset;
            }
            set
            {
                TextEditor.ActiveTextAreaControl.Caret.Position = TextEditor.Document.OffsetToPosition(value);
            }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Events
        //
        //-------------------------------------------------------------------

        #region TextChangedEvent

        public static readonly RoutedEvent TextChangedEvent =
            EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(EventHandler<TextChangedEventArgs>), typeof(KaxamlTextEditor));

        public event EventHandler<TextChangedEventArgs> TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        void RaiseTextChangedEvent(string Text)
        {
            TextChangedEventArgs newEventArgs = new TextChangedEventArgs(KaxamlTextEditor.TextChangedEvent, Text);
            RaiseEvent(newEventArgs);
        }

        #endregion

        #region TextSelectionChangedEvent

        void SelectionManager_SelectionChanged(object sender, EventArgs e)
        {
            this.RaiseTextSelectionChangedEvent();
        }

        public static readonly RoutedEvent TextSelectionChangedEvent = EventManager.RegisterRoutedEvent("TextSelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(KaxamlTextEditor));

        public event RoutedEventHandler TextSelectionChanged
        {
            add { AddHandler(TextSelectionChangedEvent, value); }
            remove { RemoveHandler(TextSelectionChangedEvent, value); }
        }

        void RaiseTextSelectionChangedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(KaxamlTextEditor.TextSelectionChangedEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Event Handlers
        //
        //-------------------------------------------------------------------

        #region EventHandlers

        bool _SetTextInternal = false;
        bool _ResetTextInternal = false;

        void TextEditorDocumentChanged(object sender, ICSharpCode.TextEditor.Document.DocumentEventArgs e)
        {
            _SetTextInternal = true;
            Text = e.Document.TextContent;
            _SetTextInternal = false;
        }

        void FoldingTimerTick(object sender, EventArgs e)
        {
            //Dispatcher.BeginInvoke(DispatcherPriority.Background, new delegate(UpdateFolding));
            UpdateFolding();
        }


        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Methods
        //
        //-------------------------------------------------------------------

        //-------------------------------------------------------------------
        //
        //  Private Methods
        //
        //-------------------------------------------------------------------

        #region Private Methods

        private void UpdateFolding()
        {
            TextEditor.Document.FoldingManager.UpdateFoldings(String.Empty, null);
            TextArea area = TextEditor.ActiveTextAreaControl.TextArea;
            area.Refresh(area.FoldMargin);
        }

        /// <summary>
        /// Checks whether the caret is inside a set of quotes (" or ').
        /// </summary>
        bool IsInsideQuotes(TextArea textArea)
        {
            bool inside = false;

            ICSharpCode.TextEditor.Document.LineSegment line = textArea.Document.GetLineSegment(textArea.Document.GetLineNumberForOffset(textArea.Caret.Offset));
            if (line != null)
            {
                if ((line.Offset + line.Length > textArea.Caret.Offset) &&
                    (line.Offset < textArea.Caret.Offset))
                {

                    char charAfter = textArea.Document.GetCharAt(textArea.Caret.Offset);
                    char charBefore = textArea.Document.GetCharAt(textArea.Caret.Offset - 1);

                    if (((charBefore == '\'') && (charAfter == '\'')) ||
                        ((charBefore == '\"') && (charAfter == '\"')))
                    {
                        inside = true;
                    }
                }
            }

            return inside;
        }


        public void InsertCharacter(char ch)
        {
            TextEditor.ActiveTextAreaControl.TextArea.MotherTextEditorControl.BeginUpdate();

            switch (TextEditor.ActiveTextAreaControl.TextArea.Caret.CaretMode)
            {
                case CaretMode.InsertMode:
                    TextEditor.ActiveTextAreaControl.TextArea.InsertChar(ch);
                    break;
                case CaretMode.OverwriteMode:
                    TextEditor.ActiveTextAreaControl.TextArea.ReplaceChar(ch);
                    break;
            }
            int currentLineNr = TextEditor.ActiveTextAreaControl.TextArea.Caret.Line;
            int delta = TextEditor.Document.FormattingStrategy.FormatLine(TextEditor.ActiveTextAreaControl.TextArea, currentLineNr, TextEditor.Document.PositionToOffset(TextEditor.ActiveTextAreaControl.TextArea.Caret.Position), ch);

            TextEditor.ActiveTextAreaControl.TextArea.MotherTextEditorControl.EndUpdate();
        }

        public void InsertStringAtCaret(string s)
        {
            TextEditor.ActiveTextAreaControl.TextArea.MotherTextEditorControl.BeginUpdate();

            switch (TextEditor.ActiveTextAreaControl.TextArea.Caret.CaretMode)
            {
                case CaretMode.InsertMode:
                    TextEditor.ActiveTextAreaControl.TextArea.InsertString(s);
                    break;
                case CaretMode.OverwriteMode:
                    TextEditor.ActiveTextAreaControl.TextArea.InsertString(s);
                    break;
            }

            TextEditor.ActiveTextAreaControl.TextArea.MotherTextEditorControl.EndUpdate();
        }

        public void InsertString(string s, int offset)
        {
            TextEditor.ActiveTextAreaControl.TextArea.MotherTextEditorControl.BeginUpdate();

            TextEditor.ActiveTextAreaControl.TextArea.Document.Insert(offset, s);

            TextEditor.ActiveTextAreaControl.TextArea.MotherTextEditorControl.EndUpdate();
        }

        public void RemoveString(int beginIndex, int count)
        {
            TextEditor.ActiveTextAreaControl.TextArea.MotherTextEditorControl.BeginUpdate();

            TextEditor.ActiveTextAreaControl.TextArea.Document.Remove(beginIndex, count);

            if (this.CaretIndex > beginIndex)
            {
                TextEditor.ActiveTextAreaControl.TextArea.Caret.Column -= count;
            }

            TextEditor.ActiveTextAreaControl.TextArea.MotherTextEditorControl.EndUpdate();
        }

        #endregion

        #region Code Completion

        ICSharpCode.TextEditor.Gui.CompletionWindow.CodeCompletionWindow codeCompletionWindow;
        //static CompletionWindow window;
        static CodeCompletionPopup popup;

        void CompleteTag()
        {
            int caret = TextEditor.ActiveTextAreaControl.Caret.Offset - 1;
            int begin = XmlParser.GetActiveElementStartIndex(this.Text, caret);
            int end = begin + 1;

            if (Text[caret - 1] == '/')
            {
                return;
            }
            else
            {
                int start = XmlParser.GetActiveElementStartIndex(this.Text, caret);

                // bail if we are either in a comment or if we are completing a "closing" tag
                if (Text[start + 1] == '/' || Text[start + 1] == '!') return;


                begin++;
                while (end < Text.Length && !char.IsWhiteSpace(Text[end]) && end < caret) end++;

                int column = TextEditor.ActiveTextAreaControl.Caret.Column;
                InsertStringAtCaret("</" + Text.Substring(begin, end - begin) + ">");

                TextEditor.ActiveTextAreaControl.Caret.Column = column;
                TextEditor.ActiveTextAreaControl.Caret.UpdateCaretPosition();
            }
        }

        bool _SpaceIsValid = true;

        bool ProcessText(char ch)
        {
            if (!this.IsCodeCompletionEnabled) return false;

            int _CurrCaretIndex = TextEditor.ActiveTextAreaControl.Caret.Offset;

            if (XmlCompletionDataProvider.IsSchemaLoaded)
            {
                if (CodeCompletionPopup.IsOpenSomewhere)
                {
                    if (char.IsLetterOrDigit(ch))
                    {
                        //popup.AppendChar(ch);
                        InsertCharacter(ch);
                        _CurrCaretIndex = TextEditor.ActiveTextAreaControl.Caret.Offset;

                        if (_CurrCaretIndex > _BeginCaretIndex)
                        {
                            string prefix = Text.Substring(_BeginCaretIndex, (_CurrCaretIndex - _BeginCaretIndex));
                            popup.DoSearch(prefix);
                        }

                        return true;
                    }

                    else
                    {
                        switch (ch)
                        {
                            case '>':
                                popup.Accept(false);
                                InsertCharacter(ch);
                                CompleteTag();
                                return true;

                            case ' ':
                                if (_SpaceIsValid)
                                {
                                    popup.Accept(false);
                                    InsertCharacter(ch);
                                    ShowCompletionWindow(ch);
                                    return true;
                                }
                                return false;

                            case '=':
                                popup.Accept(false);
                                InsertCharacter(ch);
                                int column = TextEditor.ActiveTextAreaControl.TextArea.Caret.Column + 1;
                                InsertStringAtCaret("\"\"");
                                TextEditor.ActiveTextAreaControl.TextArea.Caret.Column = column;
                                return true;

                            case '/':
                                popup.Cancel();
                                InsertCharacter(ch);
                                return true;

                        }
                    }
                }
                else
                {

                    switch (ch)
                    {
                        case '<':
                            InsertCharacter(ch);
                            ShowCompletionWindow(ch);
                            return true;

                        case ' ':
                            InsertCharacter(ch);
                            ShowCompletionWindow(ch);
                            return true;

                        case '>':
                            InsertCharacter(ch);
                            CompleteTag();
                            return true;

                        case '.':
                            if (XmlParser.IsInsideXmlTag(Text, _CurrCaretIndex))
                            {
                                if (popup != null)
                                {
                                    int startColumn = TextEditor.ActiveTextAreaControl.TextArea.Caret.Column;
                                    int restoreColumn = startColumn + 1;
                                    int lineOffset = TextEditor.ActiveTextAreaControl.TextArea.Caret.Offset - startColumn;

                                    while (true)
                                    {
                                        startColumn--;

                                        if (Text[lineOffset + startColumn] == '<') break;
                                        if (startColumn <= 0) return false;
                                        if (!char.IsLetterOrDigit(Text[lineOffset + startColumn])) return false;
                                    }

                                    InsertCharacter(ch);
                                    _CurrCaretIndex++;

                                    TextEditor.ActiveTextAreaControl.TextArea.Caret.Column = startColumn + 1;
                                    ShowCompletionWindow('<', XmlParser.GetActiveElementStartIndex(Text, _CurrCaretIndex) + 1);

                                    TextEditor.ActiveTextAreaControl.TextArea.Caret.Column = restoreColumn;

                                    string prefix = Text.Substring(_BeginCaretIndex, _CurrCaretIndex - _BeginCaretIndex);
                                    popup.CueSearch(prefix);

                                    return true;
                                }

                            }
                            break;

                        case '=':
                            if (!XmlParser.IsInsideAttributeValue(Text, _CurrCaretIndex))
                            {
                                InsertCharacter(ch);
                                int column = TextEditor.ActiveTextAreaControl.TextArea.Caret.Column + 1;
                                InsertStringAtCaret("\"\"");
                                TextEditor.ActiveTextAreaControl.TextArea.Caret.Column = column;
                                ShowCompletionWindow('\"');
                                return true;
                            }
                            break;


                        default:
                            if (XmlParser.IsAttributeValueChar(ch))
                            {
                                if (IsInsideQuotes(TextEditor.ActiveTextAreaControl.TextArea))
                                {
                                    // Have to insert the character ourselves since
                                    // it is not actually inserted yet.  If it is not
                                    // inserted now the code completion will not work
                                    // since the completion data provider attempts to
                                    // include the key typed as the pre-selected text.
                                    InsertCharacter(ch);
                                    ShowCompletionWindow(ch);
                                    return true;
                                }
                            }
                            break;
                    }
                }
            }
            return false;
        }

        private bool ProcessKeys(Keys keyData)
        {
            if (!this.IsCodeCompletionEnabled) return false;
            // return true to suppress the keystroke before the TextArea can handle it

            if (CodeCompletionPopup.IsOpenSomewhere)
            {
                if (keyData != Keys.Space) _SpaceIsValid = true;

                if (keyData == Keys.Down)
                {
                    popup.SelectNext();
                    return true;
                }
                if (keyData == Keys.PageDown)
                {
                    popup.PageDown();
                    return true;
                }
                if (keyData == Keys.Up)
                {
                    popup.SelectPrevious();
                    return true;
                }
                if (keyData == Keys.PageUp)
                {
                    popup.PageUp();
                    return true;
                }
                if (keyData == Keys.Enter || keyData == Keys.Return || keyData == Keys.Tab)
                {
                    // if the selected item is an attribute, then we want to automatically insert the equals sign and quotes
                    //if (popup.SelectedItem.

                    popup.Accept(false);

                    return true;
                }
                if (keyData == Keys.Escape || keyData == Keys.Left || keyData == Keys.Delete)
                {
                    popup.Cancel();
                    return true;
                }
                if (keyData == Keys.Back)
                {

                    int _CurrCaretIndex = TextEditor.ActiveTextAreaControl.Caret.Offset;

                    if (_CurrCaretIndex > _BeginCaretIndex)
                    {
                        string prefix = Text.Substring(_BeginCaretIndex, _CurrCaretIndex - _BeginCaretIndex);
                        popup.DoSearch(prefix);
                    }
                    else
                    {
                        popup.Cancel();
                    }
                }
            }

            return false;
        }

        int _BeginCaretIndex = 0;

        private delegate void OneArgDelegate(object arg);
        public void ShowCompletionWindowUI(object param)
        {
            _SpaceIsValid = false;
            if (param is ArrayList)
            {
                ArrayList items = (ArrayList)param;

                Window mainWindow = System.Windows.Application.Current.MainWindow;

                double borderX = 0; // mainWindow.ActualWidth - (mainWindow.Content as FrameworkElement).ActualWidth;
                double borderY = mainWindow.ActualHeight - (mainWindow.Content as FrameworkElement).ActualHeight;

                System.Drawing.Point editorPoint = TextEditor.PointToScreen(new System.Drawing.Point(0, 0));
                System.Drawing.Point caretPoint = TextEditor.ActiveTextAreaControl.Caret.ScreenPosition;

                popup = CodeCompletionPopup.Show(items, new Point(editorPoint.X + caretPoint.X + borderX, editorPoint.Y + caretPoint.Y + (FontSize * 1.3) + 3));
                popup.ResultProvided += w_ResultProvided;
            }
        }

        void ShowCompletionWindow(object param)
        {
            ShowCompletionWindow(param, TextEditor.ActiveTextAreaControl.Caret.Offset);
        }

        void ShowCompletionWindow(object param, int beginCaretIndex)
        {
            if (!Kaxaml.Properties.Settings.Default.EnableCodeCompletion) return;

            if (!CodeCompletionPopup.IsOpenSomewhere)
            {
                _BeginCaretIndex = beginCaretIndex;

                if (param is char)
                {

                    char ch = (char)param;

                    if (IsCodeCompletionEnabled)
                    {
                        if (ch == char.MaxValue)
                        {
                            if ((App.Current as App).Snippets != null)
                            {
                                ArrayList s = (App.Current as App).Snippets.GetSnippetCompletionItems();

                                if (s.Count > 0)
                                {
                                    this.Dispatcher.BeginInvoke(
                                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                                        new OneArgDelegate(ShowCompletionWindowUI),
                                        s);
                                }
                            }
                        }
                        else
                        {
                            XmlCompletionDataProvider completionDataProvider = new XmlCompletionDataProvider();

                            ICollection c = completionDataProvider.GenerateCompletionData("", TextEditor.ActiveTextAreaControl.TextArea, ch);

                            if (c != null)
                            {
                                ArrayList items = new ArrayList(c);
                                items.Sort();

                                this.Dispatcher.BeginInvoke(
                                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                                    new OneArgDelegate(ShowCompletionWindowUI),
                                    items);
                            }
                        }
                    }
                }
            }
        }

        void w_ResultProvided(object sender, ResultProvidedEventArgs e)
        {
            // remove the event handler
            popup.ResultProvided -= w_ResultProvided;

            if (!e.Canceled)
            {
                int _CurrCaretIndex = TextEditor.ActiveTextAreaControl.Caret.Offset;
                int inputlength = (_CurrCaretIndex - _BeginCaretIndex);
                if (inputlength < 0) inputlength = 0;

                string inserttext = "";

                if (e.Item is SnippetCompletionData)
                {
                    SnippetCompletionData snippet = e.Item as SnippetCompletionData;
                    TextEditor.ActiveTextAreaControl.Caret.ValidateCaretPos();
                    inserttext = snippet.Snippet.IndentedText(TextEditor.ActiveTextAreaControl.Caret.Position.X - inputlength, true);
                }

                if (e.Item is XmlCompletionData)
                {
                    XmlCompletionData xml = e.Item as XmlCompletionData;


                    inserttext = xml.Text;
                }

                if (_CurrCaretIndex > _BeginCaretIndex)
                {

                    string prefix = Text.Substring(_BeginCaretIndex, _CurrCaretIndex - _BeginCaretIndex);
                    if (e.ForcedAccept || e.Text.ToLowerInvariant().StartsWith(prefix.ToLowerInvariant()))
                    {
                        // clear the user entered text
                        RemoveString(_BeginCaretIndex, _CurrCaretIndex - _BeginCaretIndex);

                        // insert the selected string
                        InsertString(inserttext, _BeginCaretIndex);

                        // place the caret at the end of the inserted text
                        TextEditor.ActiveTextAreaControl.TextArea.Caret.Column += inserttext.Length;
                    }
                }
                else if (_CurrCaretIndex - _BeginCaretIndex == 0)
                {
                    InsertStringAtCaret(inserttext);
                }
            }
        }

        void CodeCompletionWindowClosed(object sender, EventArgs e)
        {
            codeCompletionWindow.Closed -= new EventHandler(CodeCompletionWindowClosed);
            codeCompletionWindow.Dispose();
            codeCompletionWindow = null;
        }


        #endregion

        #region Find and Replace

        public void ReplaceSelectedText(string s)
        {
            if (TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0] != null)
            {
                int offset = TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Offset;
                int length = TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Length;

                this.RemoveString(offset, length);
                this.InsertString(s, offset);

                SetSelection(offset, offset + s.Length, true);
            }
        }

        public string SelectedText
        {
            get
            {
                return TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText;
            }
        }

        public void SetSelection(int fromOffset, int toOffset, bool suppressSelectionChangedEvent)
        {
            try
            {
                System.Drawing.Point from = TextEditor.Document.OffsetToPosition(fromOffset);
                System.Drawing.Point to = TextEditor.Document.OffsetToPosition(toOffset);

                if (suppressSelectionChangedEvent)
                {
                    TextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectionChanged -= new EventHandler(SelectionManager_SelectionChanged);
                    TextEditor.ActiveTextAreaControl.SelectionManager.SetSelection(from, to);
                    TextEditor.ActiveTextAreaControl.TextArea.SelectionManager.SelectionChanged += new EventHandler(SelectionManager_SelectionChanged);
                }
                else
                {
                    TextEditor.ActiveTextAreaControl.SelectionManager.SetSelection(from, to);
                }
            }
            catch { }
        }

        public void SelectLine(int lineNumber)
        {

            try
            {
                System.Drawing.Point startPoint = new System.Drawing.Point(0, lineNumber);
                System.Drawing.Point endPoint = new System.Drawing.Point(0, lineNumber + 1);

                TextEditor.ActiveTextAreaControl.SelectionManager.SetSelection(startPoint, endPoint);
                TextEditor.ActiveTextAreaControl.Caret.Position = new System.Drawing.Point(0, lineNumber);
            }
            catch { }

        }

        private string _findText = "";
        private int _findIndex = 0;
        private int _findStartIndex = 0;

        public void Find(string s)
        {
            _findText = s;
            _findIndex = CaretIndex;

            if (SelectedText.ToUpperInvariant() == _findText.ToUpperInvariant())
            {
                _findIndex = CaretIndex + 1;
            }
            else
            {
                _findStartIndex = _findIndex;
            }

            FindNext();
        }

        public void FindNext()
        {
            FindNext(true);
        }

        public void FindNext(bool allowStartFromTop)
        {
            int index = Text.ToUpperInvariant().IndexOf(_findText.ToUpperInvariant(), _findIndex);
            if (index > 0)
            {
                SetSelection(index, index + _findText.Length, false);
                _findIndex = index + 1;
                CaretIndex = index;

                this.Focus();
                TextEditor.Select();
            }
            else if (allowStartFromTop)
            {
                _findIndex = 0;
                FindNext(false);
            }
            else
            {
                System.Windows.MessageBox.Show("The text \"" + _findText + "\" could not be found.");
                _findIndex = 0;
            }
        }

        public void Replace(string s, string replacement, bool selectedonly)
        {
            if (selectedonly)
            {
                int c = CaretIndex;
                string sub = SelectedText;
                string r = ReplaceEx(sub, s, replacement);

                this.ReplaceSelectedText(r);

                CaretIndex = c;
            }
            else
            {
                int c = CaretIndex;
                string r = ReplaceEx(this.Text, s, replacement);
                this.Text = r;
                CaretIndex = c;
            }
        }

        private string ReplaceEx(string original, string pattern, string replacement)
        {
            int count = 0;
            int position0 = 0;
            int position1 = 0;

            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();

            int inc = (original.Length / pattern.Length) * (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];

            while ((position1 = upperString.IndexOf(upperPattern, position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }

            if (position0 == 0) return original;

            for (int i = position0; i < original.Length; ++i) chars[count++] = original[i];

            return new string(chars, 0, count);
        }


        public System.IntPtr EditorHandle
        {
            get
            {
                return TextEditor.Handle;
            }
        }

        public TextEditorControl HostedEditorControl
        {
            get
            {
                return TextEditor;
            }
        }


        #endregion

        #region Undo/Redo

        public void Undo()
        {
            TextEditor.Undo();
        }

        public void Redo()
        {
            TextEditor.Redo();
        }


        #endregion

    }

    public class TextChangedEventArgs : RoutedEventArgs
    {
        public TextChangedEventArgs(RoutedEvent routedEvent, string text)
        {
            this.RoutedEvent = routedEvent;
            this.Text = text;
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }
    }
}