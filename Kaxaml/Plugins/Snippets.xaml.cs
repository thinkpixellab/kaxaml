using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace Kaxaml.Plugins.Default
{
    /// <summary>
    /// Interaction logic for Snippets.xaml
    /// </summary>

    public partial class Snippets : UserControl
    {

        #region Const Fields

        private const string _SnippetsFile = "KaxamlSnippets.xml";

        #endregion Const Fields

        #region Fields


        private ObservableCollection<SnippetCategory> _SnippetCategories;

        private TextBoxOverlay _tbo;
        EventHandler<TextBoxOverlayHideEventArgs> SnippetHidden;
        EventHandler<TextBoxOverlayHideEventArgs> CategoryHidden;

        #endregion Fields

        #region Constructors

        public Snippets()
        {
            // load the snippets file from the disk
            ReadValues();
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties


        public ObservableCollection<SnippetCategory> SnippetCategories
        {
            get { return _SnippetCategories; }
            set { _SnippetCategories = value; }
        }


        private string _SnippetsFullPath
        {
            get
            {
                FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string dir = fi.DirectoryName;
                return dir + "\\" + _SnippetsFile;
            }
        }


        private TextBoxOverlay _TextBoxOverlay
        {
            get
            {
                if (_tbo == null)
                {
                    _tbo = new TextBoxOverlay();
                    Style style = null;
                    try
                    {
                        style = (Style)this.FindResource("TextBoxOverlayStyle");
                    }
                    catch { }
                    _tbo.Style = style;
                }
                return _tbo;
            }
        }


        #endregion Properties

        #region Event Handlers

        void editor_CommitValues(object sender, RoutedEventArgs e)
        {
            WriteValues();
        }

        #endregion Event Handlers

        #region Private Methods

        private void MoveSnippetDown(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            ListBoxItem lbi = (ListBoxItem)cm.PlacementTarget;
            Snippet s = (Snippet)lbi.DataContext;
            SnippetCategory c = s.Category;

            int index = c.Snippets.IndexOf(s);
            if (index < c.Snippets.Count - 1)
            {
                c.Snippets.Move(index, index + 1);
            }

            WriteValues();
        }

        private void MoveSnippetUp(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            ListBoxItem lbi = (ListBoxItem)cm.PlacementTarget;
            Snippet s = (Snippet)lbi.DataContext;
            SnippetCategory c = s.Category;

            int index = c.Snippets.IndexOf(s);
            if (index > 0)
            {
                c.Snippets.Move(index, index - 1);
            }

            WriteValues();
        }

        #endregion Private Methods

        #region Public Methods

        public void DeleteCategory(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            TabItem t = (TabItem)cm.PlacementTarget;
            SnippetCategory s = (SnippetCategory)t.DataContext;

            if (MessageBox.Show("Are you sure you want to delete the category " + s.Name + " and all associated snippets?", "Delete Category?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                SnippetCategories.Remove(s);
                WriteValues();
            }
        }

        public void DeleteSnippet(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            ListBoxItem lbi = (ListBoxItem)cm.PlacementTarget;
            Snippet s = (Snippet)lbi.DataContext;
            SnippetCategory c = s.Category;
            c.Snippets.Remove(s);
            WriteValues();
        }

        public void DoCategoryHidden(object o, TextBoxOverlayHideEventArgs e)
        {
            TabItem ti = (TabItem)o;
            SnippetCategory c = (SnippetCategory)ti.DataContext;

            if (e.Result == TextBoxOverlayResult.Accept)
            {
                c.Name = e.ResultText;
                WriteValues();
            }

            _TextBoxOverlay.Hidden -= CategoryHidden;
        }

        public void DoDrop(object o, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Snippet)))
            {
                SnippetCategory sc = (SnippetCategory)(o as FrameworkElement).DataContext;
                Snippet s = (Snippet)(e.Data.GetData(typeof(Snippet)));

                // make sure we're not dragging into the same category
                if (s.Category == sc) return;

                // otherwise, consider this a move so remove from the previous category
                // and add to the new one

                // remove from old category
                s.Category.Snippets.Remove(s);

                // add to the new one
                sc.Snippets.Add(s);

                // update the category
                s.Category = sc;

                // save the changes
                WriteValues();
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string Text = (string)e.Data.GetData(DataFormats.Text);
                SnippetCategory sc = (SnippetCategory)(o as FrameworkElement).DataContext;
                sc.AddSnippet("New Snippet", "", Text);

                // write the xaml file
                WriteValues();
            }

            // if the drop target is a TabItem, then expand it 
            if (o.GetType() == typeof(TabItem))
            {
                TabItem ti = (TabItem)o;
                ti.IsSelected = true;
            }
        }

        public void DoGridDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                // create a new category
                SnippetCategory sc = new SnippetCategory();
                sc.Name = "New Category";
                sc.Snippets = new ObservableCollection<Snippet>();
                SnippetCategories.Add(sc);

                string Text = (string)e.Data.GetData(DataFormats.Text);
                sc.AddSnippet("New Snippet", "", Text);

                TabItem ti = (TabItem)SnippetCategoriesTabControl.ItemContainerGenerator.ContainerFromItem(sc);
                if (ti != null) ti.IsSelected = true;

                // write the xaml file
                WriteValues();

                // don't allow drops here anymore
                MainGrid.AllowDrop = false;
                MainGrid.Drop -= new DragEventHandler(DoGridDrop);
            }

        }

        public void DoSnippetHidden(object o, TextBoxOverlayHideEventArgs e)
        {
            _TextBoxOverlay.Hidden -= SnippetHidden;

            ListBoxItem lbi = (ListBoxItem)o;
            Snippet s = (Snippet)lbi.DataContext;

            if (e.Result == TextBoxOverlayResult.Accept)
            {
                s.Name = e.ResultText;
                WriteValues();
            }
        }

        public void EditSnippet(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            ListBoxItem lbi = (ListBoxItem)cm.PlacementTarget;
            Snippet s = lbi.DataContext as Snippet;

            if (s != null)
            {
                SnippetEditor editor = SnippetEditor.Show(s, Application.Current.MainWindow);
                editor.CommitValues += new RoutedEventHandler(editor_CommitValues);
            }
        }

        public ArrayList GetSnippetCompletionItems()
        {
            ArrayList items = new ArrayList();
            items.Sort();

            foreach (SnippetCategory c in SnippetCategories)
            {
                foreach (Snippet s in c.Snippets)
                {
                    if (!string.IsNullOrEmpty(s.Shortcut))
                    {
                        SnippetCompletionData item = new SnippetCompletionData(s.Text, s.Shortcut, s);
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        public void GridLoaded(object sender, RoutedEventArgs e)
        {
            if (SnippetCategories.Count == 0)
            {
                MainGrid.AllowDrop = true;
                MainGrid.Drop += new DragEventHandler(DoGridDrop);
            }
        }

        public void MoveCategoryDown(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            TabItem t = (TabItem)cm.PlacementTarget;
            SnippetCategory s = (SnippetCategory)t.DataContext;

            int index = SnippetCategories.IndexOf(s);
            if (index < SnippetCategories.Count - 1)
            {
                SnippetCategories.Move(index, index + 1);
            }
        }

        public void MoveCategoryUp(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            TabItem t = (TabItem)cm.PlacementTarget;
            SnippetCategory s = (SnippetCategory)t.DataContext;

            int index = SnippetCategories.IndexOf(s);
            if (index > 0)
            {
                SnippetCategories.Move(index, index - 1);
            }
        }

        public void NewCategory(object o, EventArgs e)
        {
            SnippetCategory s = new SnippetCategory();
            s.Name = "New Category";
            s.Snippets = new ObservableCollection<Snippet>();
            SnippetCategories.Add(s);
        }

        public void NewSnippet(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            ListBoxItem lbi = (ListBoxItem)cm.PlacementTarget;
            Snippet s = (Snippet)lbi.DataContext;
            WriteValues();

        }

        public void ReadValues()
        {
            SnippetCategories = new ObservableCollection<SnippetCategory>();

            XmlDocument xml = new XmlDocument();

            try
            {
                xml.Load(_SnippetsFullPath);
            }
            catch (System.IO.FileNotFoundException)
            {
                return;
            }

            XmlNode root = xml.DocumentElement;

            foreach (XmlNode CategoryNode in root.ChildNodes)
            {
                if (CategoryNode.Name == "Category")
                {
                    // look for a matching categor
                    SnippetCategory c = null;

                    foreach (SnippetCategory sc in SnippetCategories)
                    {
                        if (sc.Name.CompareTo(CategoryNode.Attributes["Name"].Value) == 0) c = sc;
                    }

                    if (c == null)
                    {
                        c = new SnippetCategory();
                        c.Name = CategoryNode.Attributes["Name"].Value;
                        c.Snippets = new ObservableCollection<Snippet>();
                        SnippetCategories.Add(c);
                    }

                    foreach (XmlNode SnippetNode in CategoryNode.ChildNodes)
                    {
                        string name = "";
                        string shortcut = "";

                        if (SnippetNode.Attributes["Name"] != null) name = SnippetNode.Attributes["Name"].Value;
                        if (SnippetNode.Attributes["Shortcut"] != null) shortcut = SnippetNode.Attributes["Shortcut"].Value;

                        c.AddSnippet(name, shortcut, SnippetNode.InnerText);
                    }
                }
            }
        }

        public void RenameCategory(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            TabItem ti = (TabItem)cm.PlacementTarget;
            SnippetCategory c = (SnippetCategory)ti.DataContext;

            if (CategoryHidden == null) CategoryHidden = DoCategoryHidden;
            _TextBoxOverlay.Hidden += CategoryHidden;

            _TextBoxOverlay.Hidden += DoCategoryHidden;
            _TextBoxOverlay.Show((ti as FrameworkElement), new Rect(new Point(14, 2), new Size(ti.ActualWidth - 20, 20)), c.Name);

            WriteValues();

        }

        public void RenameSnippet(object o, EventArgs e)
        {
            ContextMenu cm = (ContextMenu)ItemsControl.ItemsControlFromItemContainer(o as MenuItem);
            ListBoxItem lbi = (ListBoxItem)cm.PlacementTarget;
            Snippet s = (Snippet)lbi.DataContext;

            if (SnippetHidden == null) SnippetHidden = DoSnippetHidden;
            _TextBoxOverlay.Hidden += SnippetHidden;

            // HACK: i'm handling the offset here rather than in the style
            _TextBoxOverlay.Show((lbi as FrameworkElement), new Rect(new Point(14, 0), new Size(lbi.ActualWidth - 14, lbi.ActualHeight)), s.Name);

            WriteValues();

        }

        public void WriteValues()
        {
            XmlTextWriter xmlWriter = new XmlTextWriter(_SnippetsFullPath, System.Text.Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xmlWriter.WriteStartElement("Snippets");
            xmlWriter.Close();

            XmlDocument xml = new XmlDocument();
            xml.Load(_SnippetsFullPath);

            XmlNode root = xml.DocumentElement;

            foreach (SnippetCategory c in SnippetCategories)
            {
                XmlElement cnode = xml.CreateElement("Category");
                cnode.SetAttribute("Name", c.Name);

                if (c.Snippets != null)
                {

                    foreach (Snippet s in c.Snippets)
                    {
                        XmlElement snode = xml.CreateElement("Snippet");
                        snode.SetAttribute("Name", s.Name);
                        snode.SetAttribute("Shortcut", s.Shortcut);
                        cnode.AppendChild(snode);

                        XmlCDataSection cdata = xml.CreateCDataSection(s.Text);
                        snode.AppendChild(cdata);
                    }

                    root.AppendChild(cnode);
                }
            }

            xml.Save(_SnippetsFullPath);

        }

        #endregion Public Methods

    }

    public class SnippetCompletionData : ICompletionData
    {

        #region Constructors

        public SnippetCompletionData(string description, string text, Snippet snippet)
        {
            _Description = description;
            _Text = text;
            _Snippet = snippet;
        }

        #endregion Constructors


        #region ICompletionData Members

        private string _Description;
        public string Description
        {
            get { return _Description; }
        }

        public int ImageIndex
        {
            get { return 0; }
        }

        public bool InsertAction(ICSharpCode.TextEditor.TextArea textArea, char ch)
        {
            return true;
        }

        public double Priority
        {
            get { return 0; }
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

        private Snippet _Snippet;
        public Snippet Snippet
        {
            get { return _Snippet; }
            set { _Snippet = value; }
        }


        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            SnippetCompletionData s = (SnippetCompletionData)obj;
            return (s.Text.CompareTo(this.Text));
        }

        #endregion
    }

    public class Snippet : INotifyPropertyChanged
    {

        #region Fields


        private string _Name;
        private string _Shortcut;
        private string _Text;

        private SnippetCategory _Category;

        #endregion Fields

        #region Constructors

        public Snippet(string name, string shortcut, string text, SnippetCategory category)
        {
            Name = name;
            Shortcut = shortcut;
            Text = text;
            Category = category;
        }

        #endregion Constructors

        #region Properties


        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Shortcut
        {
            get { return _Shortcut; }
            set
            {
                if (_Shortcut != value)
                {
                    _Shortcut = value;
                    OnPropertyChanged("Shortcut");
                }
            }
        }

        public string Text
        {
            get { return _Text; }
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    OnPropertyChanged("Text");
                }
            }
        }


        public SnippetCategory Category
        {
            get { return _Category; }
            set
            {
                if (_Category != value)
                {
                    _Category = value;
                    OnPropertyChanged("Category");
                }
            }
        }


        #endregion Properties

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Overridden Methods

        public override string ToString()
        {
            return Text;
        }

        #endregion Overridden Methods

        #region Private Methods

        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion Private Methods

        #region Public Methods

        public string IndentedText(int count, bool skipFirstLine)
        {
            string t = Text.Replace("\r\n", "\n");

            if (t.CompareTo(Text) != 0)
            {
                // separate Text into lines
                string[] lines = t.Split('\n');

                // generate the "indent" string
                string indent = "";
                for (int i = 0; i < count; i++)
                {
                    indent = indent + " ";
                }

                // append indent to the beginning of each string and
                // generate the result string (with newly inserted line ends)

                string result = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    if (skipFirstLine && i == 0)
                    {
                        result = result + lines[i] + "\r\n";
                    }
                    else if (i == lines.Length - 1)
                    {
                        lines[i] = lines[i].Replace("\n", "");
                        result = result + indent + lines[i];
                    }
                    else
                    {
                        lines[i] = lines[i].Replace("\n", "");
                        result = result + indent + lines[i] + "\r\n";
                    }
                }

                return result;
            }

            return Text;
        }

        #endregion Public Methods

    }

    public class SnippetCategory : INotifyPropertyChanged
    {

        #region Fields


        private ObservableCollection<Snippet> _Snippets;

        private string _Name;

        #endregion Fields

        #region Properties


        public ObservableCollection<Snippet> Snippets
        {
            get { return _Snippets; }
            set { _Snippets = value; }
        }


        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }


        #endregion Properties

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Private Methods

        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion Private Methods

        #region Public Methods

        public void AddSnippet(string name, string shortcut, string text)
        {
            Snippet s = new Snippet(name, shortcut, text, this);
            Snippets.Add(s);
        }

        #endregion Public Methods

    }


    public enum TextBoxOverlayResult { None, Accept, Cancel };

    public class TextBoxOverlayHideEventArgs : EventArgs
    {

        #region Fields


        public readonly string ResultText;

        public readonly TextBoxOverlayResult Result;

        #endregion Fields

        #region Constructors

        public TextBoxOverlayHideEventArgs(TextBoxOverlayResult result, string resultText)
        {
            Result = result;
            ResultText = resultText;
        }

        #endregion Constructors

    }

    public class TextBoxOverlay : TextBox
    {

        #region Fields


        private bool IsOpen = false;

        private AdornerLayer _AdornerLayer = null;
        private ElementAdorner _ElementAdorner = null;
        private UIElement _Element;

        #endregion Fields

        #region Constructors

        public TextBoxOverlay()
        {

        }

        #endregion Constructors

        #region Events

        public event EventHandler<TextBoxOverlayHideEventArgs> Hidden;

        #endregion Events

        #region Overridden Methods

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (IsOpen) Hide(TextBoxOverlayResult.Accept);
            }

            if (e.Key == Key.Escape)
            {
                if (IsOpen) Hide(TextBoxOverlayResult.Cancel);
            }

            base.OnKeyDown(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (IsOpen) Hide(TextBoxOverlayResult.Accept);
            base.OnLostFocus(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (IsOpen) Hide(TextBoxOverlayResult.Accept);
            base.OnLostKeyboardFocus(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            if (IsOpen) Hide(TextBoxOverlayResult.Accept);
            base.OnLostMouseCapture(e);
        }

        #endregion Overridden Methods

        #region Public Methods

        public void Hide(TextBoxOverlayResult result)
        {
            if (IsOpen) // only hide once
            {
                if (_ElementAdorner != null)
                {
                    AdornerLayer layer = VisualTreeHelper.GetParent(_ElementAdorner) as AdornerLayer;
                    if (layer != null)
                    {
                        _ElementAdorner.Hide();
                        layer.Remove(_ElementAdorner);
                    }
                }

                TextBoxOverlayHideEventArgs e = new TextBoxOverlayHideEventArgs(result, Text);
                OnHidden(e);

                IsOpen = false;
            }
        }

        public void OnHidden(TextBoxOverlayHideEventArgs e)
        {
            if (Hidden != null) Hidden(_Element, e);
        }

        public void Show(UIElement element, Rect rect, string InitialValue)
        {
            Size size = rect.Size;
            Point offset = rect.Location;

            Height = size.Height;
            Width = size.Width;

            Text = InitialValue;
            SelectAll();

            _Element = element;

            _AdornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (_AdornerLayer == null)
            {
                return;
            }
            else
            {
                _ElementAdorner = new ElementAdorner(element, this, offset);
                _AdornerLayer.Add(_ElementAdorner);
                this.Focus();
            }

            IsOpen = true;
        }

        #endregion Public Methods

    }

    internal sealed class ElementAdorner : Adorner
    {

        #region Fields


        private Point _Offset;
        UIElement _Element;

        #endregion Fields

        #region Constructors

        public ElementAdorner(UIElement owner, UIElement element, Point offset)
            : base(owner)
        {
            _Element = element;

            this.AddVisualChild(element);
            Offset = offset;
        }

        #endregion Constructors

        #region Properties


        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }


        public Point Offset
        {
            get { return _Offset; }
            set
            {
                _Offset = value;
                InvalidateArrange();
            }
        }


        #endregion Properties

        #region Overridden Methods

        protected override Size ArrangeOverride(Size finalSize)
        {
            _Element.Arrange(new Rect(Offset, _Element.DesiredSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _Element;
        }

        #endregion Overridden Methods

        #region Methods

        internal void Hide()
        {
            this.RemoveVisualChild(_Element);
            _Element = null;
        }

        #endregion Methods

    }

}