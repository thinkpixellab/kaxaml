using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Threading;

namespace Kaxaml.Silverlight
{
    class SilverlightBridge
    {

		#region Fields 


        string _xaml;

        bool _isClientWaiting;

        SocketServer server = new SocketServer();

		#endregion Fields 

		#region Constructors 

        public SilverlightBridge()
        {
            Listen();
        }

		#endregion Constructors 

		#region Properties 


        public bool IsConnected { get; set; }

        public bool IsListening { get; set; }


		#endregion Properties 

		#region Event Handlers 

        void server_Connected(object sender, EventArgs e)
        {
            server.BeginReceive();
            IsConnected = true;
            RaiseConnected();
        }

        void server_Listening(object sender, EventArgs e)
        {
            IsListening = true;
            RaiseListening();
        }

        void server_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _isClientWaiting = false;
            Debug.WriteLine("SERVER RECEIVED:" + e.Message);

            if (e.Message.Equals("SENDLENGTH"))
            {
                _isClientWaiting = true; // this lets us know that we can begin the send process later on
            }
            else if (e.Message.Equals("SENDXAML"))
            {
                server.SendReceive("XAML|" + _xaml);
            }
            else if (e.Message.Equals("SUCCESS"))
            {
                // it worked
                // send final COMPLETE message
                server.SendReceive("COMPLETE");

                RaiseParseCompleted(true, string.Empty, 0, 0);
            }
            else if (e.Message.StartsWith("ERROR|"))
            {
                // parse the error and respond
                // send final COMPLETE message

                //client.SendReceive("ERROR|" + errorLineNumber + "|" + errorLinePos + "|" + errorMessage);

                string[] errorResponse = e.Message.Split(new char[]{'|'});
                int errorLineNumber = int.Parse(errorResponse[1]);
                int errorLinePos = int.Parse(errorResponse[2]);
                string errorMessage = errorResponse[3];

                RaiseParseCompleted(false, errorMessage, errorLineNumber, errorLinePos);

                //Debug.WriteLine(e.Message);
                server.SendReceive("COMPLETE");
            }
        }

		#endregion Event Handlers 

		#region Private Methods 

        private void SendXaml()
        {
            bool _sendBegin = false;

            while (!_sendBegin)
            {
                if (IsConnected && IsListening)
                {
                    if (_isClientWaiting)
                    {
                        // send the length of the xaml
                        server.SendReceive("LENGTH|" + _xaml.Length.ToString());

                        // the rest of the message processing happens in the message received area
                    }
                    else
                    {
                        throw new Exception("Error communicating with the Silverlight object: messages received out of order.");
                    }
                    _sendBegin = true;
                }

                Thread.Sleep(200);
            }
        }

		#endregion Private Methods 

		#region Public Methods 

        public void Listen()
        {
            server.Connected += new EventHandler(server_Connected);
            server.Listening += new EventHandler(server_Listening);
            server.MessageReceived += new MessageReceivedEventHandler(server_MessageReceived);
            server.BeginListen("127.0.0.1", NextPort);
        }

        public void SendXaml(string xaml)
        {
            _xaml = xaml;

            Thread thread = new Thread(new ThreadStart(SendXaml));
            thread.Start();
        }

		#endregion Public Methods 


        #region Port Server 

        static SocketServer portServer = new SocketServer();

       static SilverlightBridge()
        {
            portServer.Connected += new EventHandler(portServer_Connected);
            portServer.Listening += new EventHandler(portServer_Listening);
            portServer.MessageReceived += new MessageReceivedEventHandler(portServer_MessageReceived);
            portServer.BeginListen("127.0.0.1", 4505);
        }

        static void portServer_Connected(object sender, EventArgs e)
        {
            portServer.BeginReceive();
        }

        static void portServer_Listening(object sender, EventArgs e)
        {
            //may not need this
        }

        static void portServer_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Equals("GETPORT"))
            {
                portServer.SendReceive(NextPort.ToString());
                NextPort++;
            }
        }

        static int NextPort = 4506;

        #endregion

        #region Connected Event

        public event EventHandler Connected;

        private void RaiseConnected()
        {
            if (Connected != null)
            {
                Connected(this, new EventArgs());
            }
        }

        #endregion

        #region Listening Event

        public event EventHandler Listening;

        private void RaiseListening()
        {
            if (Listening != null)
            {
                Listening(this, new EventArgs());
            }
        }

        #endregion

        #region ParseCompleted

        public event ParseCompletedEventHandler ParseCompleted;

        private void RaiseParseCompleted(bool isValidXaml, string errorMessage, int errorLineNumber, int errorLinePosition)
        {
            if (ParseCompleted != null)
            {
                ParseCompleted(this, new ParseCompletedEventArgs(isValidXaml, errorMessage, errorLineNumber, errorLinePosition));
            }
        }

        #endregion
    }


    public delegate void ParseCompletedEventHandler(object sender, ParseCompletedEventArgs e);

    public class ParseCompletedEventArgs : EventArgs
    {

		#region Constructors 

        public ParseCompletedEventArgs(bool isValidXaml, string errorMessage, int errorLineNumber, int errorLinePosition)
        {
            IsValidXaml = isValidXaml;
            ErrorMessage = errorMessage;
            ErrorLineNumber = errorLineNumber;
            ErrorLinePosition = errorLinePosition;
        }

		#endregion Constructors 

		#region Properties 


        public string ErrorMessage { get; set; }


        public bool IsValidXaml { get; set; }

        public int ErrorLineNumber { get; set; }

        public int ErrorLinePosition { get; set; }


		#endregion Properties 

    }
    

}
