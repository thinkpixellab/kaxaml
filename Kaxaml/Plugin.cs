using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Kaxaml
{
    public class Plugin
    {
        #region Properties

        private UserControl _Root;
        public UserControl Root
        {
            get { return _Root; }
            set { _Root = value; }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        private Key _Key;
        public Key Key
        {
            get { return _Key; }
            set { _Key = value; }
        }

        public ModifierKeys _ModifierKeys;
        public ModifierKeys ModifierKeys
        {
            get { return _ModifierKeys; }
            set { _ModifierKeys = value; }
        }

        private ImageSource _Icon;
        public ImageSource Icon
        {
            get { return _Icon; }
            set { _Icon = value; }
        }

        #endregion
    }
}
