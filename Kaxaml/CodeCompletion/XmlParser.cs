using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Kaxaml.CodeCompletion
{
    /// <summary>
    /// Utility class that contains xml parsing routines used to determine
    /// the currently selected element so we can provide intellisense.
    /// </summary>
    /// <remarks>
    /// All of the routines return <see cref="XmlElementPath"/> objects
    /// since we are interested in the complete path or tree to the 
    /// currently active element. 
    /// </remarks>
    public static class XmlParser
    {
        #region Static Methods

        /// <summary>
        /// Locates the index of the end tag character.
        /// </summary>
        /// <returns>
        /// Returns the index of the end tag character; otherwise
        /// -1 if no end tag character is found or a start tag
        /// character is found first.
        /// </returns>
        static int GetActiveElementEndIndex(string xml, int index)
        {
            int elementEndIndex = index;

            for (int i = index; i < xml.Length; ++i)
            {

                char currentChar = xml[i];
                if (currentChar == '>')
                {
                    elementEndIndex = i;
                    break;
                }
                else if (currentChar == '<')
                {
                    elementEndIndex = -1;
                    break;
                }
            }

            return elementEndIndex;
        }

        /// <summary>
        /// Locates the index of the start tag &lt; character.
        /// </summary>
        /// <returns>
        /// Returns the index of the start tag character; otherwise
        /// -1 if no start tag character is found or a end tag
        /// &gt; character is found first.
        /// </returns>
        public static int GetActiveElementStartIndex(string xml, int index)
        {
            int elementStartIndex = -1;

            int currentIndex = index - 1;
            for (int i = 0; i < index; ++i)
            {

                char currentChar = xml[currentIndex];
                if (currentChar == '<')
                {
                    elementStartIndex = currentIndex;
                    break;
                }
                else if (currentChar == '>')
                {
                    break;
                }

                --currentIndex;
            }

            return elementStartIndex;
        }

        /// <summary>
        /// Gets path of the xml element start tag that the specified 
        /// <paramref name="index"/> is currently inside.
        /// </summary>
        /// <remarks>If the index outside the start tag then an empty path
        /// is returned.</remarks>
        public static XmlElementPath GetActiveElementStartPath(string xml, int index)
        {
            XmlElementPath path = new XmlElementPath();

            string elementText = GetActiveElementStartText(xml, index);

            if (elementText != null)
            {
                QualifiedName elementName = GetElementName(elementText);
                NamespaceURI elementNamespace = GetElementNamespace(elementText);

                path = GetParentElementPath(xml.Substring(0, index));
                if (elementNamespace.Namespace.Length == 0)
                {
                    if (path.Elements.Count > 0)
                    {
                        QualifiedName parentName = path.Elements[path.Elements.Count - 1];
                        elementNamespace.Namespace = parentName.Namespace;
                        elementNamespace.Prefix = parentName.Prefix;
                    }
                }

                path.Elements.Add(new QualifiedName(elementName.Name, elementNamespace.Namespace, elementNamespace.Prefix));
                path.Compact();
            }

            return path;
        }

        /// <summary>
        /// Gets the active element path given the element text.
        /// </summary>
        static XmlElementPath GetActiveElementStartPath(string xml, int index, string elementText)
        {
            QualifiedName elementName = GetElementName(elementText);
            NamespaceURI elementNamespace = GetElementNamespace(elementText);

            XmlElementPath path = GetParentElementPath(xml.Substring(0, index));
            if (elementNamespace.Namespace.Length == 0)
            {
                if (path.Elements.Count > 0)
                {
                    QualifiedName parentName = path.Elements[path.Elements.Count - 1];
                    elementNamespace.Namespace = parentName.Namespace;
                    elementNamespace.Prefix = parentName.Prefix;
                }
            }
            path.Elements.Add(new QualifiedName(elementName.Name, elementNamespace.Namespace, elementNamespace.Prefix));
            path.Compact();
            return path;
        }

        /// <summary>
        /// Gets path of the xml element start tag that the specified 
        /// <paramref name="index"/> is currently located. This is different to the
        /// GetActiveElementStartPath method since the index can be inside the element 
        /// name.
        /// </summary>
        /// <remarks>If the index outside the start tag then an empty path
        /// is returned.</remarks>
        public static XmlElementPath GetActiveElementStartPathAtIndex(string xml, int index)
        {
            // Find first non xml element name character to the right of the index.
            index = GetCorrectedIndex(xml.Length, index);
            int currentIndex = index;
            for (; currentIndex < xml.Length; ++currentIndex)
            {
                char ch = xml[currentIndex];
                if (!IsXmlNameChar(ch))
                {
                    break;
                }
            }

            string elementText = GetElementNameAtIndex(xml, currentIndex);
            if (elementText != null)
            {
                return GetActiveElementStartPath(xml, currentIndex, elementText);
            }
            return new XmlElementPath();
        }

        /// <summary>
        /// Gets the text of the xml element start tag that the index is 
        /// currently inside.
        /// </summary>
        /// <returns>
        /// Returns the text up to and including the start tag &lt; character.
        /// </returns>
        static string GetActiveElementStartText(string xml, int index)
        {
            int elementStartIndex = GetActiveElementStartIndex(xml, index);
            if (elementStartIndex >= 0)
            {
                if (elementStartIndex < index)
                {
                    int elementEndIndex = GetActiveElementEndIndex(xml, index);
                    if (elementEndIndex >= index)
                    {
                        return xml.Substring(elementStartIndex, elementEndIndex - elementStartIndex);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the name of the attribute inside but before the specified 
        /// index.
        /// </summary>
        public static string GetAttributeName(string xml, int index)
        {
            if (xml.Length == 0)
            {
                return String.Empty;
            }

            index = GetCorrectedIndex(xml.Length, index);

            return GetAttributeName(xml, index, true, true, true);
        }

        static string GetAttributeName(string xml, int index, bool ignoreWhitespace, bool ignoreQuote, bool ignoreEqualsSign)
        {
            string name = String.Empty;

            // From the end of the string work backwards until we have
            // picked out the attribute name.
            StringBuilder reversedAttributeName = new StringBuilder();

            int currentIndex = index;
            bool invalidString = true;

            for (int i = 0; i <= index; ++i)
            {

                char currentChar = xml[currentIndex];

                if (Char.IsLetterOrDigit(currentChar))
                {
                    if (!ignoreEqualsSign)
                    {
                        ignoreWhitespace = false;
                        reversedAttributeName.Append(currentChar);
                    }
                }
                else if (Char.IsWhiteSpace(currentChar))
                {
                    if (ignoreWhitespace == false)
                    {
                        // Reached the start of the attribute name.
                        invalidString = false;
                        break;
                    }
                }
                else if ((currentChar == '\'') || (currentChar == '\"'))
                {
                    if (ignoreQuote)
                    {
                        ignoreQuote = false;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (currentChar == '=')
                {
                    if (ignoreEqualsSign)
                    {
                        ignoreEqualsSign = false;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (IsAttributeValueChar(currentChar))
                {
                    if (!ignoreQuote)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

                --currentIndex;
            }

            if (!invalidString)
            {
                name = ReverseString(reversedAttributeName.ToString());
            }

            return name;
        }

        /// <summary>
        /// Gets the name of the attribute at the specified index. The index
        /// can be anywhere inside the attribute name or in the attribute value.
        /// </summary>
        public static string GetAttributeNameAtIndex(string xml, int index)
        {
            index = GetCorrectedIndex(xml.Length, index);

            bool ignoreWhitespace = true;
            bool ignoreEqualsSign = false;
            bool ignoreQuote = false;

            if (IsInsideAttributeValue(xml, index))
            {
                // Find attribute name start.
                int elementStartIndex = GetActiveElementStartIndex(xml, index);
                if (elementStartIndex == -1)
                {
                    return String.Empty;
                }

                // Find equals sign.
                for (int i = index; i > elementStartIndex; --i)
                {
                    char ch = xml[i];
                    if (ch == '=')
                    {
                        index = i;
                        ignoreEqualsSign = true;
                        break;
                    }
                }
            }
            else
            {
                // Find end of attribute name.
                for (; index < xml.Length; ++index)
                {
                    char ch = xml[index];
                    if (!Char.IsLetterOrDigit(ch))
                    {
                        if (ch == '\'' || ch == '\"')
                        {
                            ignoreQuote = true;
                            ignoreEqualsSign = true;
                        }
                        break;
                    }
                }
                --index;
            }

            return GetAttributeName(xml, index, ignoreWhitespace, ignoreQuote, ignoreEqualsSign);
        }

        /// <summary>
        /// Gets the attribute value at the specified index.
        /// </summary>
        /// <returns>An empty string if no attribute value can be found.</returns>
        public static string GetAttributeValueAtIndex(string xml, int index)
        {
            if (!IsInsideAttributeValue(xml, index))
            {
                return String.Empty;
            }

            index = GetCorrectedIndex(xml.Length, index);

            int elementStartIndex = GetActiveElementStartIndex(xml, index);
            if (elementStartIndex == -1)
            {
                return String.Empty;
            }

            // Find equals sign.
            int equalsSignIndex = -1;
            for (int i = index; i > elementStartIndex; --i)
            {
                char ch = xml[i];
                if (ch == '=')
                {
                    equalsSignIndex = i;
                    break;
                }
            }

            if (equalsSignIndex == -1)
            {
                return String.Empty;
            }

            // Find attribute value.
            char quoteChar = ' ';
            bool foundQuoteChar = false;
            StringBuilder attributeValue = new StringBuilder();
            for (int i = equalsSignIndex; i < xml.Length; ++i)
            {
                char ch = xml[i];
                if (!foundQuoteChar)
                {
                    if (ch == '\"' || ch == '\'')
                    {
                        quoteChar = ch;
                        foundQuoteChar = true;
                    }
                }
                else
                {
                    if (ch == quoteChar)
                    {
                        // End of attribute value.
                        return attributeValue.ToString();
                    }
                    else if (IsAttributeValueChar(ch) || (ch == '\"' || ch == '\''))
                    {
                        attributeValue.Append(ch);
                    }
                    else
                    {
                        // Invalid character found.
                        return String.Empty;
                    }
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Ensures that the index is on the last character if it is
        /// too large.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <param name="index">The current index.</param>
        /// <returns>The index unchanged if the index is smaller than the
        /// length of the string; otherwise it returns length - 1.</returns>
        static int GetCorrectedIndex(int length, int index)
        {
            if (index >= length)
            {
                index = length - 1;
            }
            return index;
        }

        /// <summary>
        /// Gets the element name from the element start tag string.
        /// </summary>
        /// <param name="xml">This string must start at the 
        /// element we are interested in.</param>
        static QualifiedName GetElementName(string xml)
        {
            string name = String.Empty;

            // Find the end of the element name.
            xml = xml.Replace("\r\n", " ");
            int index = xml.IndexOf(' ');
            if (index > 0)
            {
                name = xml.Substring(1, index - 1);
            }
            else
            {
                name = xml.Substring(1);
            }

            QualifiedName qualifiedName = new QualifiedName();

            int prefixIndex = name.IndexOf(':');
            if (prefixIndex > 0)
            {
                qualifiedName.Prefix = name.Substring(0, prefixIndex);
                qualifiedName.Name = name.Substring(prefixIndex + 1);
            }
            else
            {
                qualifiedName.Name = name;
            }

            return qualifiedName;
        }

        /// <summary>
        /// Gets the element name at the specified index.
        /// </summary>
        static string GetElementNameAtIndex(string xml, int index)
        {
            int elementStartIndex = GetActiveElementStartIndex(xml, index);
            if (elementStartIndex >= 0 && elementStartIndex < index)
            {
                int elementEndIndex = GetActiveElementEndIndex(xml, index);
                if (elementEndIndex == -1)
                {
                    elementEndIndex = xml.IndexOf(' ', elementStartIndex);
                }
                if (elementEndIndex >= elementStartIndex)
                {
                    return xml.Substring(elementStartIndex, elementEndIndex - elementStartIndex);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the element namespace from the element start tag
        /// string.
        /// </summary>
        /// <param name="xml">This string must start at the 
        /// element we are interested in.</param>
        static NamespaceURI GetElementNamespace(string xml)
        {
            NamespaceURI namespaceURI = new NamespaceURI();

            Match match = Regex.Match(xml, ".*?(xmlns\\s*?|xmlns:.*?)=\\s*?['\\\"](.*?)['\\\"]");
            if (match.Success)
            {
                namespaceURI.Namespace = match.Groups[2].Value;

                string xmlns = match.Groups[1].Value.Trim();
                int prefixIndex = xmlns.IndexOf(':');
                if (prefixIndex > 0)
                {
                    namespaceURI.Prefix = xmlns.Substring(prefixIndex + 1);
                }
            }

            return namespaceURI;
        }

        /// <summary>
        /// Gets the parent element path based on the index position.
        /// </summary>
        public static XmlElementPath GetParentElementPath(string xml)
        {
            XmlElementPath path = new XmlElementPath();

            try
            {
                using (var reader = new StringReader(xml))
                {
                    using (var xmlReader = new XmlTextReader(reader))
                    {
                        xmlReader.XmlResolver = null;
                        while (xmlReader.Read())
                        {
                            switch (xmlReader.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (!xmlReader.IsEmptyElement)
                                    {
                                        QualifiedName elementName = new QualifiedName(xmlReader.LocalName, xmlReader.NamespaceURI, xmlReader.Prefix);
                                        path.Elements.Add(elementName);
                                    }
                                    break;
                                case XmlNodeType.EndElement:
                                    path.Elements.RemoveLast();
                                    break;
                            }
                        }
                    }
                }
            }
            catch (XmlException)
            {
                //MessageBox.Show(e.Message);
            }
            catch (WebException)
            {
                // Do nothing.
            }

            path.Compact();

            return path;
        }

        /// <summary>
        /// Checks for valid xml attribute value character
        /// </summary>
        public static bool IsAttributeValueChar(char ch)
        {
            if (Char.IsLetterOrDigit(ch) ||
                (ch == ':') ||
                (ch == '/') ||
                (ch == '_') ||
                (ch == '.') ||
                (ch == '-') ||
                (ch == '#'))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified index is inside an attribute value.
        /// </summary>
        public static bool IsInsideAttributeValue(string xml, int index)
        {
            if (xml.Length == 0)
            {
                return false;
            }

            if (index > xml.Length)
            {
                index = xml.Length;
            }

            int elementStartIndex = GetActiveElementStartIndex(xml, index);
            if (elementStartIndex == -1)
            {
                return false;
            }

            // Count the number of double quotes and single quotes that exist
            // before the first equals sign encountered going backwards to
            // the start of the active element.
            bool foundEqualsSign = false;
            int doubleQuotesCount = 0;
            int singleQuotesCount = 0;
            char lastQuoteChar = ' ';
            for (int i = index - 1; i > elementStartIndex; --i)
            {
                char ch = xml[i];
                if (ch == '=')
                {
                    foundEqualsSign = true;
                    break;
                }
                else if (ch == '\"')
                {
                    lastQuoteChar = ch;
                    ++doubleQuotesCount;
                }
                else if (ch == '\'')
                {
                    lastQuoteChar = ch;
                    ++singleQuotesCount;
                }
            }

            bool isInside = false;

            if (foundEqualsSign)
            {
                // Odd number of quotes?
                if ((lastQuoteChar == '\"') && ((doubleQuotesCount % 2) > 0))
                {
                    isInside = true;
                }
                else if ((lastQuoteChar == '\'') && ((singleQuotesCount % 2) > 0))
                {
                    isInside = true;
                }
            }

            return isInside;
        }

        public static bool IsInsideXmlTag(string xml, int index)
        {
            if (GetActiveElementStartIndex(xml, index) == -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks whether the attribute at the end of the string is a 
        /// namespace declaration.
        /// </summary>
        public static bool IsNamespaceDeclaration(string xml, int index)
        {
            if (xml.Length == 0)
            {
                return false;
            }

            index = GetCorrectedIndex(xml.Length, index);

            // Move back one character if the last character is an '='
            if (xml[index] == '=')
            {
                xml = xml.Substring(0, xml.Length - 1);
                --index;
            }

            // From the end of the string work backwards until we have
            // picked out the last attribute and reached some whitespace.
            StringBuilder reversedAttributeName = new StringBuilder();

            bool ignoreWhitespace = true;
            int currentIndex = index;
            for (int i = 0; i < index; ++i)
            {

                char currentChar = xml[currentIndex];

                if (Char.IsWhiteSpace(currentChar))
                {
                    if (ignoreWhitespace == false)
                    {
                        // Reached the start of the attribute name.
                        break;
                    }
                }
                else if (Char.IsLetterOrDigit(currentChar) || (currentChar == ':'))
                {
                    ignoreWhitespace = false;
                    reversedAttributeName.Append(currentChar);
                }
                else
                {
                    // Invalid string.
                    break;
                }

                --currentIndex;
            }

            // Did we get a namespace?

            bool isNamespace = false;

            if ((reversedAttributeName.ToString() == "snlmx") || (reversedAttributeName.ToString().EndsWith(":snlmx")))
            {
                isNamespace = true;
            }

            return isNamespace;
        }

        /// <summary>
        /// Checks for valid xml element or attribute name character.
        /// </summary>
        public static bool IsXmlNameChar(char ch)
        {
            if (Char.IsLetterOrDigit(ch) ||
                (ch == ':') ||
                (ch == '/') ||
                (ch == '_') ||
                (ch == '.') ||
                (ch == '-'))
            {
                return true;
            }

            return false;
        }

        static string ReverseString(string text)
        {
            StringBuilder reversedString = new StringBuilder(text);

            int index = text.Length;
            foreach (char ch in text)
            {
                --index;
                reversedString[index] = ch;
            }

            return reversedString.ToString();
        }

        #endregion Static Methods

        #region Nested Classes


        /// <summary>
        /// Helper class.  Holds the namespace URI and the prefix currently
        /// in use for this namespace.
        /// </summary>
        class NamespaceURI
        {

            #region Fields


            string namespaceURI = String.Empty;
            string prefix = String.Empty;

            #endregion Fields

            #region Constructors

            public NamespaceURI(string namespaceURI, string prefix)
            {
                this.namespaceURI = namespaceURI;
                this.prefix = prefix;
            }

            public NamespaceURI()
            {
            }

            #endregion Constructors

            #region Properties


            public string Namespace
            {
                get
                {
                    return namespaceURI;
                }
                set
                {
                    namespaceURI = value;
                }
            }

            public string Prefix
            {
                get
                {
                    return prefix;
                }
                set
                {
                    prefix = value;
                    if (prefix == null)
                    {
                        prefix = String.Empty;
                    }
                }
            }


            #endregion Properties

        }
        #endregion Nested Classes

    }












    /////////////////////////////////////////////////////


    /// <summary>
    /// Represents the path to an xml element starting from the root of the
    /// document.
    /// </summary>
    public class XmlElementPath
    {

        #region Fields


        QualifiedNameCollection elements = new QualifiedNameCollection();

        #endregion Fields

        #region Constructors

        public XmlElementPath()
        {
        }

        #endregion Constructors

        #region Properties


        /// <summary>
        /// Gets the elements specifying the path.
        /// </summary>
        /// <remarks>The order of the elements determines the path.</remarks>
        public QualifiedNameCollection Elements
        {
            get
            {
                return elements;
            }
        }


        #endregion Properties

        #region Overridden Methods

        /// <summary>
        /// An xml element path is considered to be equal if 
        /// each path item has the same name and namespace.
        /// </summary>
        public override bool Equals(object obj)
        {

            if (!(obj is XmlElementPath)) return false;
            if (this == obj) return true;

            XmlElementPath rhs = (XmlElementPath)obj;
            if (elements.Count == rhs.elements.Count)
            {

                for (int i = 0; i < elements.Count; ++i)
                {
                    if (!elements[i].Equals(rhs.elements[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return elements.GetHashCode();
        }

        #endregion Overridden Methods

        #region Private Methods

        /// <summary>
        /// Finds the first parent that does belong in the specified
        /// namespace.
        /// </summary>
        int FindNonMatchingParentElement(string namespaceUri)
        {
            int index = -1;

            if (elements.Count > 1)
            {
                // Start the check from the the last but one item.
                for (int i = elements.Count - 2; i >= 0; --i)
                {
                    QualifiedName name = elements[i];
                    if (name.Namespace != namespaceUri)
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Removes elements up to and including the specified index.
        /// </summary>
        void RemoveParentElements(int index)
        {
            while (index >= 0)
            {
                --index;
                elements.RemoveFirst();
            }
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Compacts the path so it only contains the elements that are from 
        /// the namespace of the last element in the path. 
        /// </summary>
        /// <remarks>This method is used when we need to know the path for a
        /// particular namespace and do not care about the complete path.
        /// </remarks>
        public void Compact()
        {
            if (elements.Count > 0)
            {
                QualifiedName lastName = Elements[Elements.Count - 1];
                if (lastName != null)
                {
                    int index = FindNonMatchingParentElement(lastName.Namespace);
                    if (index != -1)
                    {
                        RemoveParentElements(index);
                    }
                }
            }
        }

        #endregion Public Methods

    }





    /////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// An <see cref="XmlQualifiedName"/> with the namespace prefix.
    /// </summary>
    /// <remarks>
    /// The namespace prefix active for a namespace is 
    /// needed when an element is inserted via autocompletion. This
    /// class just adds this extra information alongside the 
    /// <see cref="XmlQualifiedName"/>.
    /// </remarks>
    public class QualifiedName
    {

        #region Fields


        string prefix = String.Empty;

        XmlQualifiedName xmlQualifiedName = XmlQualifiedName.Empty;

        #endregion Fields

        #region Constructors

        public QualifiedName(string name, string namespaceUri, string prefix)
        {
            xmlQualifiedName = new XmlQualifiedName(name, namespaceUri);
            this.prefix = prefix;
        }

        public QualifiedName(string name, string namespaceUri)
            : this(name, namespaceUri, String.Empty)
        {
        }

        public QualifiedName()
        {
        }

        #endregion Constructors

        #region Properties


        /// <summary>
        /// Gets the namespace of the qualified name.
        /// </summary>
        public string Namespace
        {
            get
            {
                return xmlQualifiedName.Namespace;
            }
            set
            {
                xmlQualifiedName = new XmlQualifiedName(xmlQualifiedName.Name, value);
            }
        }

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        public string Name
        {
            get
            {
                return xmlQualifiedName.Name;
            }
            set
            {
                xmlQualifiedName = new XmlQualifiedName(value, xmlQualifiedName.Namespace);
            }
        }

        /// <summary>
        /// Gets the namespace prefix used.
        /// </summary>
        public string Prefix
        {
            get
            {
                return prefix;
            }
            set
            {
                prefix = value;
            }
        }


        #endregion Properties

        #region Static Methods

        public static bool operator !=(QualifiedName lhs, QualifiedName rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(QualifiedName lhs, QualifiedName rhs)
        {
            bool equals = false;

            if (((object)lhs != null) && ((object)rhs != null))
            {
                equals = lhs.Equals(rhs);
            }
            else if (((object)lhs == null) && ((object)rhs == null))
            {
                equals = true;
            }

            return equals;
        }

        #endregion Static Methods

        #region Overridden Methods

        /// <summary>
        /// A qualified name is considered equal if the namespace and 
        /// name are the same.  The prefix is ignored.
        /// </summary>
        public override bool Equals(object obj)
        {
            bool equals = false;

            QualifiedName qualifiedName = obj as QualifiedName;
            if (qualifiedName != null)
            {
                equals = xmlQualifiedName.Equals(qualifiedName.xmlQualifiedName);
            }
            else
            {
                XmlQualifiedName name = obj as XmlQualifiedName;
                if (name != null)
                {
                    equals = xmlQualifiedName.Equals(name);
                }
            }

            return equals;
        }

        public override int GetHashCode()
        {
            return xmlQualifiedName.GetHashCode();
        }

        #endregion Overridden Methods

    }


    //////////////////////////////////////////////////////////////////////////////////



    /// <summary>
    ///   A collection that stores <see cref='QualifiedName'/> objects.
    /// </summary>
    [Serializable()]
    public class QualifiedNameCollection : CollectionBase
    {

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of <see cref='QualifiedNameCollection'/> based on another <see cref='QualifiedNameCollection'/>.
        /// </summary>
        /// <param name='val'>
        ///   A <see cref='QualifiedNameCollection'/> from which the contents are copied
        /// </param>
        public QualifiedNameCollection(QualifiedNameCollection val)
        {
            this.AddRange(val);
        }

        /// <summary>
        ///   Initializes a new instance of <see cref='QualifiedNameCollection'/> containing any array of <see cref='QualifiedName'/> objects.
        /// </summary>
        /// <param name='val'>
        ///       A array of <see cref='QualifiedName'/> objects with which to intialize the collection
        /// </param>
        public QualifiedNameCollection(QualifiedName[] val)
        {
            this.AddRange(val);
        }

        /// <summary>
        ///   Initializes a new instance of <see cref='QualifiedNameCollection'/>.
        /// </summary>
        public QualifiedNameCollection()
        {
        }

        #endregion Constructors

        #region Properties


        /// <summary>
        /// Gets the namespace prefix of the last item.
        /// </summary>
        public string LastPrefix
        {
            get
            {
                string prefix = String.Empty;

                if (Count > 0)
                {
                    QualifiedName name = this[Count - 1];
                    prefix = name.Prefix;
                }

                return prefix;
            }
        }


        /// <summary>
        ///   Represents the entry at the specified index of the <see cref='QualifiedName'/>.
        /// </summary>
        /// <param name='index'>The zero-based index of the entry to locate in the collection.</param>
        /// <value>The entry at the specified index of the collection.</value>
        /// <exception cref='ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
        public QualifiedName this[int index]
        {
            get
            {
                return ((QualifiedName)(List[index]));
            }
            set
            {
                List[index] = value;
            }
        }


        #endregion Properties

        #region Public Methods

        /// <summary>
        ///   Adds a <see cref='QualifiedName'/> with the specified value to the 
        ///   <see cref='QualifiedNameCollection'/>.
        /// </summary>
        /// <param name='val'>The <see cref='QualifiedName'/> to add.</param>
        /// <returns>The index at which the new element was inserted.</returns>
        /// <seealso cref='QualifiedNameCollection.AddRange'/>
        public int Add(QualifiedName val)
        {
            return List.Add(val);
        }

        /// <summary>
        ///   Copies the elements of an array to the end of the <see cref='QualifiedNameCollection'/>.
        /// </summary>
        /// <param name='val'>
        ///    An array of type <see cref='QualifiedName'/> containing the objects to add to the collection.
        /// </param>
        /// <seealso cref='QualifiedNameCollection.Add'/>
        public void AddRange(QualifiedName[] val)
        {
            for (int i = 0; i < val.Length; i++)
            {
                this.Add(val[i]);
            }
        }

        /// <summary>
        ///   Adds the contents of another <see cref='QualifiedNameCollection'/> to the end of the collection.
        /// </summary>
        /// <param name='val'>
        ///    A <see cref='QualifiedNameCollection'/> containing the objects to add to the collection.
        /// </param>
        /// <seealso cref='QualifiedNameCollection.Add'/>
        public void AddRange(QualifiedNameCollection val)
        {
            for (int i = 0; i < val.Count; i++)
            {
                this.Add(val[i]);
            }
        }

        /// <summary>
        ///   Gets a value indicating whether the 
        ///    <see cref='QualifiedNameCollection'/> contains the specified <see cref='QualifiedName'/>.
        /// </summary>
        /// <param name='val'>The <see cref='QualifiedName'/> to locate.</param>
        /// <returns>
        /// <see langword='true'/> if the <see cref='QualifiedName'/> is contained in the collection; 
        ///   otherwise, <see langword='false'/>.
        /// </returns>
        /// <seealso cref='QualifiedNameCollection.IndexOf'/>
        public bool Contains(QualifiedName val)
        {
            return List.Contains(val);
        }

        /// <summary>
        ///   Copies the <see cref='QualifiedNameCollection'/> values to a one-dimensional <see cref='Array'/> instance at the 
        ///    specified index.
        /// </summary>
        /// <param name='array'>The one-dimensional <see cref='Array'/> that is the destination of the values copied from <see cref='QualifiedNameCollection'/>.</param>
        /// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
        /// <exception cref='ArgumentException'>
        ///   <para><paramref name='array'/> is multidimensional.</para>
        ///   <para>-or-</para>
        ///   <para>The number of elements in the <see cref='QualifiedNameCollection'/> is greater than
        ///         the available space between <paramref name='arrayIndex'/> and the end of
        ///         <paramref name='array'/>.</para>
        /// </exception>
        /// <exception cref='ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
        /// <exception cref='ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
        /// <seealso cref='Array'/>
        public void CopyTo(QualifiedName[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        ///  Returns an enumerator that can iterate through the <see cref='QualifiedNameCollection'/>.
        /// </summary>
        /// <seealso cref='IEnumerator'/>
        public new QualifiedNameEnumerator GetEnumerator()
        {
            return new QualifiedNameEnumerator(this);
        }

        /// <summary>
        ///    Returns the index of a <see cref='QualifiedName'/> in 
        ///       the <see cref='QualifiedNameCollection'/>.
        /// </summary>
        /// <param name='val'>The <see cref='QualifiedName'/> to locate.</param>
        /// <returns>
        ///   The index of the <see cref='QualifiedName'/> of <paramref name='val'/> in the 
        ///   <see cref='QualifiedNameCollection'/>, if found; otherwise, -1.
        /// </returns>
        /// <seealso cref='QualifiedNameCollection.Contains'/>
        public int IndexOf(QualifiedName val)
        {
            return List.IndexOf(val);
        }

        /// <summary>
        ///   Inserts a <see cref='QualifiedName'/> into the <see cref='QualifiedNameCollection'/> at the specified index.
        /// </summary>
        /// <param name='index'>The zero-based index where <paramref name='val'/> should be inserted.</param>
        /// <param name='val'>The <see cref='QualifiedName'/> to insert.</param>
        /// <seealso cref='QualifiedNameCollection.Add'/>
        public void Insert(int index, QualifiedName val)
        {
            List.Insert(index, val);
        }

        /// <summary>
        ///   Removes a specific <see cref='QualifiedName'/> from the <see cref='QualifiedNameCollection'/>.
        /// </summary>
        /// <param name='val'>The <see cref='QualifiedName'/> to remove from the <see cref='QualifiedNameCollection'/>.</param>
        /// <exception cref='ArgumentException'><paramref name='val'/> is not found in the Collection.</exception>
        public void Remove(QualifiedName val)
        {
            List.Remove(val);
        }

        /// <summary>
        /// Removes the first item in the collection.
        /// </summary>
        public void RemoveFirst()
        {
            if (Count > 0)
            {
                RemoveAt(0);
            }
        }

        /// <summary>
        /// Removes the last item in this collection.
        /// </summary>
        public void RemoveLast()
        {
            if (Count > 0)
            {
                RemoveAt(Count - 1);
            }
        }

        #endregion Public Methods

        #region Nested Classes


        /// <summary>
        ///   Enumerator that can iterate through a QualifiedNameCollection.
        /// </summary>
        /// <seealso cref='IEnumerator'/>
        /// <seealso cref='QualifiedNameCollection'/>
        /// <seealso cref='QualifiedName'/>
        public class QualifiedNameEnumerator : IEnumerator
        {

            #region Fields


            IEnumerable temp;
            IEnumerator baseEnumerator;

            #endregion Fields

            #region Constructors

            /// <summary>
            ///   Initializes a new instance of <see cref='QualifiedNameEnumerator'/>.
            /// </summary>
            public QualifiedNameEnumerator(QualifiedNameCollection mappings)
            {
                this.temp = ((IEnumerable)(mappings));
                this.baseEnumerator = temp.GetEnumerator();
            }

            #endregion Constructors

            #region Properties


            object IEnumerator.Current
            {
                get
                {
                    return baseEnumerator.Current;
                }
            }

            /// <summary>
            ///   Gets the current <see cref='QualifiedName'/> in the <seealso cref='QualifiedNameCollection'/>.
            /// </summary>
            public QualifiedName Current
            {
                get
                {
                    return ((QualifiedName)(baseEnumerator.Current));
                }
            }


            #endregion Properties

            #region Public Methods

            /// <summary>
            ///   Advances the enumerator to the next <see cref='QualifiedName'/> of the <see cref='QualifiedNameCollection'/>.
            /// </summary>
            public bool MoveNext()
            {
                return baseEnumerator.MoveNext();
            }

            /// <summary>
            ///   Sets the enumerator to its initial position, which is before the first element in the <see cref='QualifiedNameCollection'/>.
            /// </summary>
            public void Reset()
            {
                baseEnumerator.Reset();
            }

            #endregion Public Methods

        }
        #endregion Nested Classes

    }

}
