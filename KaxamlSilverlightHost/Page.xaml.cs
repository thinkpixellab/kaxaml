using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Markup;
using PixelLab.Common;

namespace KaxamlSilverlightHost
{
    [ScriptableType]
    public partial class Page : UserControl
    {

        #region Fields


        string errorMessage = "";

        int errorLineNumber = 0;
        int errorLinePos = 0;

        #endregion Fields

        #region Constructors

        public Page()
        {
            InitializeComponent();
            HtmlPage.RegisterScriptableObject("Bridge", this);
        }

        #endregion Constructors

        #region Public Methods

        public string GetLastErrorLineNumber()
        {
            return errorLineNumber.ToString();
        }

        public string GetLastErrorLinePos()
        {
            return errorLinePos.ToString();
        }

        public string GetLastErrorMessage()
        {
            return errorMessage;
        }

        [ScriptableMember]
        public bool ParseXaml(string xaml)
        {
            bool parseSuccess = false;

            try
            {
                object o = XamlReader.Load(xaml);

                UIElement rootElement = o as UIElement;
                if (rootElement != null)
                {
                    this.__________ContentHost.Children.Clear();
                    this.__________ContentHost.Children.Add(rootElement);
                }

                parseSuccess = true;
            }
            catch (XamlParseException parseException)
            {
                parseSuccess = false;

                errorLineNumber = parseException.LineNumber;
                errorLinePos = parseException.LinePosition;

                Exception exception = parseException;
                while (exception.InnerException != null) exception = exception.InnerException;
                errorMessage = exception.Message;
            }
            catch (Exception exception)
            {
                if (exception.IsCriticalException())
                {
                    throw;
                }

                parseSuccess = false;

                errorLineNumber = 0;
                errorLinePos = 0;
                while (exception.InnerException != null) exception = exception.InnerException;
                errorMessage = exception.Message;
            }

            return parseSuccess;
        }

        #endregion Public Methods

    }





}
