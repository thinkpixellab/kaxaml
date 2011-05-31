using System;
using System.Diagnostics;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using PixelLab.Common;

namespace Kaxaml.CodeCompletion
{
    public class XmlCompletionDataProvider : ICompletionDataProvider
    {

        #region Static Fields

        static XmlSchemaCompletionData defaultSchemaCompletionData = null;

        #endregion Static Fields

        #region Fields


        protected string preSelection = null;
        string defaultNamespacePrefix = String.Empty;

        #endregion Fields

        #region Properties


        public static bool IsSchemaLoaded
        {
            get { return (defaultSchemaCompletionData != null); }
        }


        #endregion Properties

        #region Static Methods

        public static void LoadSchema(string filename)
        {
            Func<Exception> doLoad = () => LoadSchemaFromFile(filename);
            doLoad.BeginInvoke((result) =>
            {
                var ex = doLoad.EndInvoke(result);
                if (ex != null)
                {
                    MessageBox.Show("Failed to load schema");
                    Debug.WriteLine(ex);
                }
            }, null);
        }

        private static Exception LoadSchemaFromFile(string filename)
        {
            try
            {
                defaultSchemaCompletionData = new XmlSchemaCompletionData(filename);
                return null;
            }
            catch (Exception ex)
            {
                if (ex.IsCriticalException())
                {
                    throw;
                }
                else
                {
                    return ex;
                }
            }
        }

        #endregion Static Methods


        #region ICompletionDataProvider Members

        public int DefaultIndex
        {
            get { return 0; }
        }

        public ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
        {
            string text = String.Concat(textArea.Document.GetText(0, textArea.Caret.Offset), charTyped);

            switch (charTyped)
            {
                case '<':
                    // Child element intellisense.
                    XmlElementPath parentPath = XmlParser.GetParentElementPath(text);
                    if (parentPath.Elements.Count > 0)
                    {
                        ICompletionData[] data = GetChildElementCompletionData(parentPath);
                        //returnval = data;
                        return data;
                    }
                    else if (defaultSchemaCompletionData != null)
                    {
                        return defaultSchemaCompletionData.GetElementCompletionData(defaultNamespacePrefix);
                    }
                    break;

                case ' ':
                    // Attribute intellisense.
                    if (!XmlParser.IsInsideAttributeValue(text, text.Length))
                    {
                        XmlElementPath path = XmlParser.GetActiveElementStartPath(text, text.Length);
                        if (path.Elements.Count > 0)
                        {
                            return GetAttributeCompletionData(path);
                        }
                    }
                    break;

                case '\'':
                case '\"':

                    // Attribute value intellisense.
                    //if (XmlParser.IsAttributeValueChar(charTyped)) {
                    text = text.Substring(0, text.Length - 1);
                    string attributeName = XmlParser.GetAttributeName(text, text.Length);
                    if (attributeName.Length > 0)
                    {
                        XmlElementPath elementPath = XmlParser.GetActiveElementStartPath(text, text.Length);
                        if (elementPath.Elements.Count > 0)
                        {
                            preSelection = charTyped.ToString();
                            return GetAttributeValueCompletionData(elementPath, attributeName);
                            //		}
                        }
                    }
                    break;
            }

            return null;

        }


        ICompletionData[] GetChildElementCompletionData(XmlElementPath path)
        {
            ICompletionData[] completionData = null;

            XmlSchemaCompletionData schema = defaultSchemaCompletionData;
            if (schema != null)
            {
                completionData = schema.GetChildElementCompletionData(path);
            }

            return completionData;
        }

        ICompletionData[] GetAttributeCompletionData(XmlElementPath path)
        {
            ICompletionData[] completionData = null;

            XmlSchemaCompletionData schema = defaultSchemaCompletionData;
            if (schema != null)
            {
                completionData = schema.GetAttributeCompletionData(path);
            }

            return completionData;
        }

        ICompletionData[] GetAttributeValueCompletionData(XmlElementPath path, string name)
        {
            ICompletionData[] completionData = null;

            XmlSchemaCompletionData schema = defaultSchemaCompletionData;
            if (schema != null)
            {
                completionData = schema.GetAttributeValueCompletionData(path, name);
            }

            return completionData;
        }

        ImageList _ImageList;
        public ImageList ImageList
        {
            get
            {
                if (_ImageList == null)
                {
                    _ImageList = new ImageList();
                    //_ImageList.Images.Add(new System.Drawing.Bitmap(@"C:\element2.png"));

                }

                return _ImageList;
            }
        }

        public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
            textArea.InsertString(data.Text);
            return false;
            //throw new Exception("The method or operation is not implemented.");
        }

        public string PreSelection
        {
            get
            {
                return "";
            }

            //get { throw new Exception("The method or operation is not implemented."); }
        }

        public CompletionDataProviderKeyResult ProcessKey(char key)
        {
            return CompletionDataProviderKeyResult.NormalKey;
            //throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
