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
using System.Collections.ObjectModel;
using Kaxaml.Documents;
using Kaxaml.DocumentViews;
using KaxamlPlugins;

namespace Kaxaml.Views
{
    /// <summary>
    /// Interaction logic for DocumentsView.xaml
    /// </summary>

    public partial class DocumentsView : System.Windows.Controls.UserControl
    {

		#region Constructors 

        public DocumentsView()
        {
            InitializeComponent();
            KaxamlInfo.ParseRequested += new KaxamlInfo.ParseRequestedDelegate(KaxamlInfo_ParseRequested);
        }

		#endregion Constructors 


        #region XamlDocuments (DependencyProperty)

        /// <summary>
        /// description of XamlDocuments
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
            DependencyProperty.Register("XamlDocuments", typeof(ObservableCollection<XamlDocument>), typeof(DocumentsView), new FrameworkPropertyMetadata(new ObservableCollection<XamlDocument>()));

        #endregion

        #region SelectedDocument (DependencyProperty)

        /// <summary>
        /// The currently selected XamlDocument.
        /// </summary>
        public XamlDocument SelectedDocument
        {
            get { return (XamlDocument)GetValue(SelectedDocumentProperty); }
            set { SetValue(SelectedDocumentProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for SelectedDocument
        /// </summary>
        public static readonly DependencyProperty SelectedDocumentProperty =
            DependencyProperty.Register("SelectedDocument", typeof(XamlDocument), typeof(DocumentsView),
            new FrameworkPropertyMetadata(default(XamlDocument), new PropertyChangedCallback(SelectedDocumentChanged)));

        /// <summary>
        /// PropertyChangedCallback for SelectedDocument
        /// </summary>
        private static void SelectedDocumentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is DocumentsView)
            {
                DocumentsView owner = (DocumentsView)obj;
                // handle changed event here

                XamlDocument document = (XamlDocument)args.NewValue;
                ListBoxItem listBoxItem = (ListBoxItem)owner.ContentListBox.ItemContainerGenerator.ContainerFromItem(document);

                if (listBoxItem != null)
                {
                    IXamlDocumentView v = (IXamlDocumentView)listBoxItem.Template.FindName("PART_DocumentView", listBoxItem);
                    if (v != null)
                    {
                        owner._view = v; // (IXamlDocumentView)listBoxItem.Template.FindName("PART_DocumentView", listBoxItem);
                        owner.SelectedView = owner._view;
                        v.OnActivate();
                        KaxamlInfo.Editor = owner.SelectedView.TextEditor;
                    }
                }
            }
        }


        #endregion

        #region SelectedView (DependencyProperty)

        /// <summary>
        /// The view associated with the currently selected document.
        /// </summary>
        public IXamlDocumentView SelectedView
        {
            get { return (IXamlDocumentView)GetValue(SelectedViewProperty); }
            set { SetValue(SelectedViewProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for SelectedView
        /// </summary>
        public static readonly DependencyProperty SelectedViewProperty =
            DependencyProperty.Register("SelectedView", typeof(IXamlDocumentView), typeof(DocumentsView), new FrameworkPropertyMetadata(default(IXamlDocumentView)));

        #endregion

        #region Event Handlers

        IXamlDocumentView _view;

        private void DocumentViewLoaded(object sender, RoutedEventArgs e)
        {
            _view = (IXamlDocumentView)sender;

            if (SelectedDocument == _view.XamlDocument)
            {
                SelectedView = _view;
                KaxamlInfo.Editor = SelectedView.TextEditor;
            }
        }

        void KaxamlInfo_ParseRequested()
        {
            if (SelectedView != null)
            {
                SelectedView.Parse();
            }
        }

        #endregion

        #region Public Methods


        #endregion
    }
}