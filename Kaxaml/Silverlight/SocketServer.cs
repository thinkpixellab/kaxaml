using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Net;
using System.Windows;
using System.Diagnostics;
using System.Threading;

namespace Kaxaml.Silverlight
{
    public class SocketServer
    {

		#region Const Fields 

        private const int Receive = 1;
        private const int Send = 0;

		#endregion Const Fields 

		#region Static Fields 

        private static AutoResetEvent autoEvent = new AutoResetEvent(false);
        private static AutoResetEvent[] autoSendReceiveEvents = new AutoResetEvent[]
        {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
        };

		#endregion Static Fields 

		#region Fields 


        bool isConnected;

        IPEndPoint endPoint;
        Socket listener;
        Socket worker;

		#endregion Fields 


        #region Public Methods

        public void BeginListen(string ipString, int port)
        {
            try
            {
                //create the listening socket.
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                endPoint = new IPEndPoint(IPAddress.Parse(ipString), port);

                //bind to localhost
                listener.Bind(endPoint);

                //start listening...
                listener.Listen(10);

                // create the call back for any client connections...
                listener.BeginAccept(new AsyncCallback(OnClientConnect), null);
                RaiseListening();
            }
            catch (SocketException se)
            {
                Debug.WriteLine(se.Message);
                //MessageBox.Show(se.Message);
            }
        }

        public void BeginReceive()
        {
            if (worker != null && isConnected)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                byte[] response = new byte[2048];
                args.SetBuffer(response, 0, response.Length);
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);

                worker.ReceiveAsync(args);
            }
        }

        public void SendReceive(string message)
        {
            if (isConnected)
            {
                Byte[] bytes = Encoding.UTF8.GetBytes(message);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(bytes, 0, bytes.Length);
                args.UserToken = worker;
                args.RemoteEndPoint = endPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

                worker.SendAsync(args);

                //AutoResetEvent.WaitAll(autoSendReceiveEvents);

                //return Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
            }
            else
            {
                throw new SocketException((int)SocketError.NotConnected);
            }
        }



        #endregion

        #region Async Callbacks

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {

            autoSendReceiveEvents[Receive].Set();

            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    // Prepare receiving
                    Socket s = e.UserToken as Socket;

                    byte[] response = new byte[2048];
                    e.SetBuffer(response, 0, response.Length);
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                    s.ReceiveAsync(e);
                }
            }
            else
            {
                //ProcessError(e);
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            autoSendReceiveEvents[Send].Set();
            string receiveString = Encoding.UTF8.GetString(e.Buffer, 0, Math.Min(e.BytesTransferred, e.Buffer.Length));
            RaiseMessageReceived(receiveString);
        }


        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                // if the work already exists, we get rid of the old worker (we only service the last connection we see)
                if (worker != null)
                {
                    worker.Shutdown(SocketShutdown.Both);
                    worker.Close();
                }
                worker = listener.EndAccept(asyn);
                isConnected = true;
                RaiseConnected();

                // setup the socket to receive new connections 
                listener.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (SocketException se)
            {
                Debug.WriteLine(se.Message);
            }
        }

        #endregion

        #region Events


        #region Listening

        public event EventHandler Listening;

        private void RaiseListening()
        {
            if (Listening != null)
            {
                Listening(this, null);
            }
        }

        #endregion
        #region Connected

        public event EventHandler Connected;

        private void RaiseConnected()
        {
            if (Connected != null)
            {
                Connected(this, null);
            }
        }

        #endregion
        #region MessageReceived

        public event MessageReceivedEventHandler MessageReceived;

        private void RaiseMessageReceived(string message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageReceivedEventArgs(message));
            }
        }

        #endregion
        #endregion
    }

    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    public class MessageReceivedEventArgs : EventArgs
    {

		#region Constructors 

        public MessageReceivedEventArgs(string message)
        {
            Message = message;
        }

		#endregion Constructors 

		#region Properties 


        public string Message { get; set; }


		#endregion Properties 

    }

}
