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
using System.Windows.Shapes;
using Kaxaml.Plugins.Default;

namespace Kaxaml.Plugins
{
    /// <summary>
    /// Interaction logic for SnippetEditor.xaml
    /// </summary>

    public partial class SnippetEditor : System.Windows.Window
    {
        #region fields

        string _name = "";
        string _shortcut = "";
        string _text = "";

        #endregion

        public SnippetEditor()
        {
            InitializeComponent();
        }

        #region Snippet (DependencyProperty)

        /// <summary>
        /// The snippet being edited
        /// </summary>
        public Snippet Snippet
        {
            get { return (Snippet)GetValue(SnippetProperty); }
            set { SetValue(SnippetProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for Snippet
        /// </summary>
        public static readonly DependencyProperty SnippetProperty =
            DependencyProperty.Register("Snippet", typeof(Snippet), typeof(SnippetEditor), new FrameworkPropertyMetadata(default(Snippet), new PropertyChangedCallback(SnippetChanged)));

        /// <summary>
        /// PropertyChangedCallback for Snippet
        /// </summary>
        private static void SnippetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is SnippetEditor)
            {
                SnippetEditor owner = (SnippetEditor)obj;

                Snippet s = (args.NewValue as Snippet);

                // create a local backup of the editable properties (for the cancel operation)

                if (s != null)
                {
                    owner._name = s.Name;
                    owner._shortcut = s.Shortcut;
                    owner._text = s.Text;
                }
            }
        }

        #endregion

        private static SnippetEditor instance = null;
        public static SnippetEditor Show(Snippet s, Window owner)
        {
            if (instance == null)
            {
                instance = new SnippetEditor();
            }

            instance.Owner = owner;
            instance.Snippet = s;
            instance.Show();

            return instance;
        }

        private void DoDone(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, null);
            this.Hide();

            RaiseCommitValuesEvent(this);

            instance = null;
        }

        private void DoCancel(object sender, RoutedEventArgs e)
        {
            Snippet.Name = _name;
            Snippet.Shortcut = _shortcut;
            Snippet.Text = _text;

            this.Hide();
            instance = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            instance = null;
        }

        #region RoutedEvent Helper Methods

        /// <summary>
        /// A static helper method to raise a routed event on a target UIElement or ContentElement.
        /// </summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        /// <param name="args">RoutedEventArgs to use when raising the event</param>
        private static void RaiseEvent(DependencyObject target, RoutedEventArgs args)
        {
            if (target is UIElement)
            {
                (target as UIElement).RaiseEvent(args);
            }
            else if (target is ContentElement)
            {
                (target as ContentElement).RaiseEvent(args);
            }
        }

        /// <summary>
        /// A static helper method that adds a handler for a routed event 
        /// to a target UIElement or ContentElement.
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="routedEvent">Event that will be handled</param>
        /// <param name="handler">Event handler to be added</param>
        private static void AddHandler(DependencyObject element, RoutedEvent routedEvent, Delegate handler)
        {
            UIElement uie = element as UIElement;
            if (uie != null)
            {
                uie.AddHandler(routedEvent, handler);
            }
            else
            {
                ContentElement ce = element as ContentElement;
                if (ce != null)
                {
                    ce.AddHandler(routedEvent, handler);
                }
            }
        }

        /// <summary>
        /// A static helper method that removes a handler for a routed event 
        /// from a target UIElement or ContentElement.
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to the event</param>
        /// <param name="routedEvent">Event that will no longer be handled</param>
        /// <param name="handler">Event handler to be removed</param>
        private static void RemoveHandler(DependencyObject element, RoutedEvent routedEvent, Delegate handler)
        {
            UIElement uie = element as UIElement;
            if (uie != null)
            {
                uie.RemoveHandler(routedEvent, handler);
            }
            else
            {
                ContentElement ce = element as ContentElement;
                if (ce != null)
                {
                    ce.RemoveHandler(routedEvent, handler);
                }
            }
        }

        #endregion

        #region CommitValues

        /// <summary>
        /// CommitValues Routed Event
        /// </summary>
        public static readonly RoutedEvent CommitValuesEvent = EventManager.RegisterRoutedEvent("CommitValues",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SnippetEditor));

        /// <summary>
        /// Occurs when ...
        /// </summary>
        public event RoutedEventHandler CommitValues
        {
            add { AddHandler(CommitValuesEvent, value); }
            remove { RemoveHandler(CommitValuesEvent, value); }
        }

        /// <summary>
        /// A helper method to raise the CommitValues event.
        /// </summary>
        protected RoutedEventArgs RaiseCommitValuesEvent()
        {
            return RaiseCommitValuesEvent(this);
        }

        /// <summary>
        /// A static helper method to raise the CommitValues event on a target element.
        /// </summary>
        /// <param name="target">UIElement or ContentElement on which to raise the event</param>
        internal static RoutedEventArgs RaiseCommitValuesEvent(UIElement target)
        {
            if (target == null) return null;

            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = CommitValuesEvent;
            RaiseEvent(target, args);
            return args;
        }

        #endregion
        


    }



}