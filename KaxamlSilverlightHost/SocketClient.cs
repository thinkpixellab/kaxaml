using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace KaxamlSilverlightHost
{
    internal sealed class SocketClient : IDisposable
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


        public bool IsConnected = false;
        public bool IsConnecting = false;
        int _receiveBufferLength = 2048;

        private DnsEndPoint endPoint;
        private Socket socket;

        #endregion Fields

        #region Constructors

        internal SocketClient(string host, int port)
        {
            endPoint = new DnsEndPoint(host, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #endregion Constructors

        #region Private Methods

        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (s.Connected)
                        s.Close();
                }
            }

            throw new SocketException((int)e.SocketError);
        }

        #endregion Private Methods

        #region Methods

        internal bool Connect()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();

            args.UserToken = socket;
            args.RemoteEndPoint = endPoint;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);

            socket.ConnectAsync(args);

            IsConnecting = true;
            autoEvent.WaitOne();

            if (args.SocketError != SocketError.Success)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal void Disconnect()
        {
            socket.Close();
        }

        internal String SendReceive(string message)
        {
            return SendReceive(message, 2048);
        }

        internal String SendReceive(string message, int receiveBufferLength)
        {
            if (IsConnected)
            {

                // we'll use this in the handler for OnSend when we setup the receive buffer
                _receiveBufferLength = receiveBufferLength;

                Byte[] bytes = Encoding.UTF8.GetBytes(message);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(bytes, 0, bytes.Length);
                args.UserToken = socket;
                args.RemoteEndPoint = endPoint;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

                socket.SendAsync(args);

                AutoResetEvent.WaitAll(autoSendReceiveEvents);

                return Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
            }
            else
                throw new SocketException((int)SocketError.NotConnected);
        }

        #endregion Methods


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

        #region Async Event Handlers

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {
            autoEvent.Set();
            IsConnected = (e.SocketError == SocketError.Success);
            if (IsConnected)
            {
                IsConnecting = false;
                RaiseConnected();
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            autoSendReceiveEvents[Send].Set();
            string receiveString = Encoding.UTF8.GetString(e.Buffer, 0, Math.Min(e.BytesTransferred, e.Buffer.Length));
            RaiseMessageReceived(receiveString);

        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            autoSendReceiveEvents[Receive].Set();

            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    // Prepare receiving
                    Socket s = e.UserToken as Socket;

                    byte[] response = new byte[_receiveBufferLength];
                    e.SetBuffer(response, 0, response.Length);
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                    s.ReceiveAsync(e);
                }
            }
            else
            {
                ProcessError(e);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            autoEvent.Close();
            autoSendReceiveEvents[Send].Close();
            autoSendReceiveEvents[Receive].Close();
            if (socket.Connected)
                socket.Close();
        }

        #endregion

        #region MessageReceived

        public event RoutedEventHandler MessageReceived;

        private void RaiseMessageReceived(string message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageReceivedEventArgs(message));
            }
        }

        #endregion
    }

    public class MessageReceivedEventArgs : RoutedEventArgs
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
