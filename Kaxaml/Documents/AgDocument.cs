using System.IO;
using Kaxaml.Properties;

namespace Kaxaml.Documents
{
    class AgDocument : XamlDocument
    {

        #region Constructors

        public AgDocument(string folder, string sourceText)
            : base(folder)
        {
            XamlDocumentType = XamlDocumentType.AgDocument;
            InitializeSourceText(sourceText);
        }

        public AgDocument(string folder)
            : base(folder)
        {
            XamlDocumentType = XamlDocumentType.AgDocument;
            InitializeSourceText(Settings.Default.AgDefaultXaml);
        }

        #endregion Constructors

        #region Static Methods

        public static AgDocument FromFile(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                string sourceText = File.ReadAllText(fullPath);

                AgDocument document = new AgDocument(Path.GetDirectoryName(fullPath), sourceText);
                document.FullPath = fullPath;

                return document;
            }

            return null;
        }

        #endregion Static Methods

    }
}