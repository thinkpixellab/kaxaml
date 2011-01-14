using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace KaxamlPlugins
{
    public interface IKaxamlInfoTextEditor
    {
        // Properties
        string SelectedText { get; }
        int CaretIndex { get; set; }
        string Text { get; set; }

        // Methods
        void InsertCharacter(char ch);
        void InsertStringAtCaret(string s);
        void InsertString(string s, int offset);
        void RemoveString(int beginIndex, int count);
        void Find(string s);
        void FindNext();
        void Replace(string s, string replacement, bool selectedonly);
        void ReplaceSelectedText(string s);
        void Undo();
        void Redo();

        // Events
        event RoutedEventHandler TextSelectionChanged;

    }
}
