using System;
using System.Collections.Generic;
using System.Text;
using Kaxaml.Documents;
using System.Windows.Media;
using KaxamlPlugins;

namespace Kaxaml.DocumentViews
{
    public interface IXamlDocumentView
    {
        //-------------------------------------------------------------------
        //
        //  Properties
        //
        //-------------------------------------------------------------------

        IKaxamlInfoTextEditor TextEditor { get; }
        XamlDocument XamlDocument { get; }

        //-------------------------------------------------------------------
        //
        //  Methods
        //
        //-------------------------------------------------------------------

        void Parse();

        /// <summary>
        /// A method which is called when a new document gets loaded into
        /// the view.  Anything that needs to happen before the user can
        /// interact with the view should happen here (such as an initial
        /// parse, etc.).
        /// </summary>
        void Initialize();
        void OnActivate();

        // GotoLine
        // Find?

        //-------------------------------------------------------------------
        //
        //  Events
        //
        //-------------------------------------------------------------------



    }
}
