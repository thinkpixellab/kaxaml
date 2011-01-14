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
using KaxamlPlugins;
using System.IO;
using System.Xml;

namespace Kaxaml.Plugins.XamlScrubber
{
    [Plugin(
        Name = "Xaml Scrubber",
        Icon = "Images\\page_lightning.png",
        Description = "Reformat and cleanup up your XAML (Ctrl+K)",
        ModifierKeys = ModifierKeys.Control,
        Key = Key.K
     )]


    public partial class XamlScrubberPlugin : System.Windows.Controls.UserControl
    {
        public XamlScrubberPlugin()
        {
            InitializeComponent();

            CommandBinding binding = new CommandBinding(GoCommand);
            binding.Executed += new ExecutedRoutedEventHandler(this.Go_Executed);
            binding.CanExecute += new CanExecuteRoutedEventHandler(this.Go_CanExecute);
            this.InputBindings.Add(new InputBinding(binding.Command, new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D")));
            this.CommandBindings.Add(binding);
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            this.Go();
        }

        private void Go()
        {
            this.InitializeValues();

            string s = KaxamlInfo.Editor.Text;

            s = Indent(s);
            s = ReducePrecision(s);

            KaxamlInfo.Editor.Text = s;
        }

        #region GoCommand

        public readonly static RoutedUICommand GoCommand = new RoutedUICommand("_Go", "GoCommand", typeof(XamlScrubberPlugin));

        void Go_Executed(object sender, ExecutedRoutedEventArgs args)
        {
            if (sender == this)
            {
                this.Go();
            }
        }

        void Go_CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            if (sender == this)
            {
                args.CanExecute = true;
            }
        }

        #endregion 


        #region Config Stuff

        int _AttributeCounteTolerance = 3;
        bool _ReorderAttributes = true;
        bool _ReducePrecision = true;
        int _Precision = 3;
        bool _RemoveCommonDefaultValues = true;
        bool _ForceLineMin = true;
        int _SpaceCount = 2;
        bool _ConvertTabsToSpaces = true;

        private void InitializeValues()
        {
            _AttributeCounteTolerance = Kaxaml.Plugins.XamlScrubber.Properties.Settings.Default.AttributeCounteTolerance;
            _ReorderAttributes = Kaxaml.Plugins.XamlScrubber.Properties.Settings.Default.ReorderAttributes;
            _ReducePrecision = Kaxaml.Plugins.XamlScrubber.Properties.Settings.Default.ReducePrecision;
            _Precision = Kaxaml.Plugins.XamlScrubber.Properties.Settings.Default.Precision;
            _RemoveCommonDefaultValues = Kaxaml.Plugins.XamlScrubber.Properties.Settings.Default.RemoveCommonDefaultValues;
            _ForceLineMin = Kaxaml.Plugins.XamlScrubber.Properties.Settings.Default.ForceLineMin;
            _SpaceCount = Kaxaml.Plugins.XamlScrubber.Properties.Settings.Default.SpaceCount;
            _ConvertTabsToSpaces = Kaxaml.Plugins.XamlScrubber.Properties.Settings.Default.ConvertTabsToSpaces;
        }

        private string IndentString
        {
            get
            {
                if (_ConvertTabsToSpaces)
                {
                    string spaces = "";
                    spaces = spaces.PadRight(_SpaceCount, ' ');

                    return spaces;
                }
                else
                {
                    return "\t";
                }
            }
        }

        #endregion


        public string ReducePrecision(string s)
        {
            string old = s;

            if (_ReducePrecision)
            {
                int begin = 0;
                int end = 0;

                while (true)
                {
                    begin = old.IndexOf('.', begin);
                    if (begin == -1) break;

                    // get past the period
                    begin++;


                    for (int i = 0; i < _Precision; i++)
                    {
                        if (old[begin] >= '0' && old[begin] <= '9') begin++;
                    }

                    end = begin;

                    while (end < old.Length && old[end] >= '0' && old[end] <= '9') end++;

                    old = old.Substring(0, begin) + old.Substring(end, old.Length - end);

                    begin++;
                }
            }

            return old;
        }

        public string Indent(string s)
        {
            string result;

            MemoryStream ms = new MemoryStream(s.Length);
            StreamWriter sw = new StreamWriter(ms);
            sw.Write(s);
            sw.Flush();

            ms.Seek(0, SeekOrigin.Begin);

            StreamReader reader = new StreamReader(ms);
            XmlReader xmlReader = XmlReader.Create(reader.BaseStream);
            xmlReader.Read();
            string str = "";

            while (!xmlReader.EOF)
            {
                string xml;
                int num;
                int num6;
                int num7;
                int num8;

                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        xml = "";
                        num = 0;
                        goto Element;

                    case XmlNodeType.Text:
                        {
                            string str4 = xmlReader.Value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
                            str = str + str4;
                            xmlReader.Read();
                            continue;
                        }
                    case XmlNodeType.ProcessingInstruction:
                        xml = "";
                        num7 = 0;
                        goto ProcessingInstruction;

                    case XmlNodeType.Comment:
                        xml = "";
                        num8 = 0;
                        goto Comment;

                    case XmlNodeType.Whitespace:
                        {
                            xmlReader.Read();
                            continue;
                        }
                    case XmlNodeType.EndElement:
                        xml = "";
                        num6 = 0;
                        goto EndElement;

                    default:
                        goto Other;
                }

            Label_00C0:
                xml = xml + IndentString;
                num++;

            Element:
                if (num < xmlReader.Depth)
                {
                    goto Label_00C0;
                }

                string elementName = xmlReader.Name;

                string str5 = str;
                str = str5 + "\r\n" + xml + "<" + xmlReader.Name;
                bool isEmptyElement = xmlReader.IsEmptyElement;


                if (xmlReader.HasAttributes)
                {
                    // construct an array of the attributes that we reorder later on
                    List<AttributeValuePair> attributes = new List<AttributeValuePair>(xmlReader.AttributeCount);

                    for (int k = 0; k < xmlReader.AttributeCount; k++)
                    {
                        xmlReader.MoveToAttribute(k);

                        if (_RemoveCommonDefaultValues)
                        {
                            if (!AttributeValuePair.IsCommonDefault(elementName, xmlReader.Name, xmlReader.Value))
                            {
                                attributes.Add(new AttributeValuePair(elementName, xmlReader.Name, xmlReader.Value));
                            }
                        }
                        else
                        {
                            attributes.Add(new AttributeValuePair(elementName, xmlReader.Name, xmlReader.Value));
                        }
                    }

                    if (_ReorderAttributes)
                    {
                        attributes.Sort();
                    }

                    xml = "";
                    string str3 = "";
                    int depth = xmlReader.Depth;

                    //str3 = str3 + IndentString;

                    for (int j = 0; j < depth; j++)
                    {
                        xml = xml + IndentString;
                    }

                    foreach (AttributeValuePair a in attributes)
                    {
                        string str7 = str;

                        if (attributes.Count > _AttributeCounteTolerance && !AttributeValuePair.ForceNoLineBreaks(elementName))
                        {
                            // break up attributes into different lines
                            str = str7 + "\r\n" + xml + str3 + a.Name + "=\"" + a.Value + "\"";
                        }
                        else
                        {
                            // attributes on one line
                            str = str7 + " " + a.Name + "=\"" + a.Value + "\"";
                        }
                    }

                }
                if (isEmptyElement)
                {
                    str = str + "/";
                }
                str = str + ">";
                xmlReader.Read();
                continue;
            Label_02F4:
                xml = xml + IndentString;
                num6++;
            EndElement:
                if (num6 < xmlReader.Depth)
                {
                    goto Label_02F4;
                }
                string str8 = str;
                str = str8 + "\r\n" + xml + "</" + xmlReader.Name + ">";
                xmlReader.Read();
                continue;
            Label_037A:
                xml = xml + "    ";
                num7++;
            ProcessingInstruction:
                if (num7 < xmlReader.Depth)
                {
                    goto Label_037A;
                }
                string str9 = str;
                str = str9 + "\r\n" + xml + "<?Mapping " + xmlReader.Value + " ?>";
                xmlReader.Read();
                continue;

            Comment:

                if (num8 < xmlReader.Depth)
                {
                    xml = xml + IndentString;
                    num8++;
                }
                str = str + "\r\n" + xml + "<!--" + xmlReader.Value + "-->";

                xmlReader.Read();
                continue;

            Other:
                xmlReader.Read();
            }

            xmlReader.Close();

            result = str;
            return result;

        }

        private class AttributeValuePair : IComparable
        {
            public string Name = "";
            public string Value = "";
            public AttributeType AttributeType = AttributeType.Other;

            public AttributeValuePair(string elementname, string name, string value)
            {
                Name = name;
                Value = value;

                // compute the AttributeType
                if (name.StartsWith("xmlns"))
                {
                    AttributeType = AttributeType.Namespace;

                }
                else
                {
                    switch (name)
                    {
                        case "Key":
                        case "x:Key":
                            AttributeType = AttributeType.Key;
                            break;

                        case "Name":
                        case "x:Name":
                            AttributeType = AttributeType.Name;
                            break;

                        case "x:Class":
                            AttributeType = AttributeType.Class;
                            break;

                        case "Canvas.Top":
                        case "Canvas.Left":
                        case "Canvas.Bottom":
                        case "Canvas.Right":
                        case "Grid.Row":
                        case "Grid.RowSpan":
                        case "Grid.Column":
                        case "Grid.ColumnSpan":
                            AttributeType = AttributeType.AttachedLayout;
                            break;

                        case "Width":
                        case "Height":
                        case "MaxWidth":
                        case "MinWidth":
                        case "MinHeight":
                        case "MaxHeight":
                            AttributeType = AttributeType.CoreLayout;
                            break;

                        case "Margin":
                        case "VerticalAlignment":
                        case "HorizontalAlignment":
                        case "Panel.ZIndex":
                            AttributeType = AttributeType.StandardLayout;
                            break;

                        case "mc:Ignorable":
                        case "d:IsDataSource":
                        case "d:LayoutOverrides":
                        case "d:IsStaticText":

                            AttributeType = AttributeType.BlendGoo;
                            break;

                        default:
                            AttributeType = AttributeType.Other;
                            break;
                    }
                }
            }

            #region IComparable Members

            public int CompareTo(object obj)
            {
                AttributeValuePair other = obj as AttributeValuePair;

                if (other != null)
                {
                    if (this.AttributeType == other.AttributeType)
                    {
                        // some common special cases where we want things to be out of the normal order

                        if (this.Name.Equals("StartPoint") && other.Name.Equals("EndPoint")) return -1;
                        if (this.Name.Equals("EndPoint") && other.Name.Equals("StartPoint")) return 1;

                        if (this.Name.Equals("Width") && other.Name.Equals("Height")) return -1;
                        if (this.Name.Equals("Height") && other.Name.Equals("Width")) return 1;

                        if (this.Name.Equals("Offset") && other.Name.Equals("Color")) return -1;
                        if (this.Name.Equals("Color") && other.Name.Equals("Offset")) return 1;

                        if (this.Name.Equals("TargetName") && other.Name.Equals("Property")) return -1;
                        if (this.Name.Equals("Property") && other.Name.Equals("TargetName")) return 1;

                        return Name.CompareTo(other.Name);
                    }
                    else
                    {
                        return this.AttributeType.CompareTo(other.AttributeType);
                    }
                }

                return 0;
            }

            public static bool IsCommonDefault(string elementname, string name, string value)
            {

                if (
                    (name == "HorizontalAlignment" && value == "Stretch") ||
                    (name == "VerticalAlignment" && value == "Stretch") ||
                    (name == "Margin" && value == "0") ||
                    (name == "Margin" && value == "0,0,0,0") ||
                    (name == "Opacity" && value == "1") ||
                    (name == "FontWeight" && value == "{x:Null}") ||
                    (name == "Background" && value == "{x:Null}") ||
                    (name == "Stroke" && value == "{x:Null}") ||
                    (name == "Fill" && value == "{x:Null}") ||
                    (name == "Visibility" && value == "Visible") ||
                    (name == "Grid.RowSpan" && value == "1") ||
                    (name == "Grid.ColumnSpan" && value == "1") ||
                    (name == "BasedOn" && value == "{x:Null}") ||

                    //(elementname == "ScaleTransform" && name == "ScaleX" && value == "1") ||
                    //(elementname == "ScaleTransform" && name == "ScaleY" && value == "1") ||
                    //(elementname == "SkewTransform" && name == "AngleX" && value == "0") ||
                    //(elementname == "SkewTransform" && name == "AngleY" && value == "0") ||
                    //(elementname == "RotateTransform" && name == "Angle" && value == "0") ||
                    //(elementname == "TranslateTransform" && name == "X" && value == "0") ||
                    //(elementname == "TranslateTransform" && name == "Y" && value == "0") ||

                    (elementname != "ColumnDefinition" && elementname != "RowDefinition" && name == "Width" && value == "Auto") ||
                    (elementname != "ColumnDefinition" && elementname != "RowDefinition" && name == "Height" && value == "Auto")

                    )
                {
                    return true;
                }

                return false;
            }

            public static bool ForceNoLineBreaks(string elementname)
            {
                if (
                    (elementname == "RadialGradientBrush") ||
                    (elementname == "GradientStop") ||
                    (elementname == "LinearGradientBrush") ||
                    (elementname == "ScaleTransfom") ||
                    (elementname == "SkewTransform") ||
                    (elementname == "RotateTransform") ||
                    (elementname == "TranslateTransform") ||
                    (elementname == "Trigger") ||
                    (elementname == "Setter")
                    )
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            #endregion
        }

        // note that these are declared in priority order for easy sorting
        private enum AttributeType
        {
            Key = 10,
            Name = 20,
            Class = 30,
            Namespace = 40,
            CoreLayout = 50,
            AttachedLayout = 60,
            StandardLayout = 70,
            Other = 1000,
            BlendGoo = 2000
        }

    }
}