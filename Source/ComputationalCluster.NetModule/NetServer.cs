﻿using ComputationalCluster.Common;
using log4net;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ComputationalCluster.NetModule
{
    public interface INetServer
    {
        void Start();
        void Stop();
    }

    /// <summary>
    /// </summary>
    public class NetServer : INetServer
    {
        public static readonly char ETB = (char)23;

        private readonly IMessageReceiver _messageReceiver;
        private readonly Encoding _encoding;
        private readonly IConfigProvider _configProvider;
        private readonly ILog _log;

        private TcpListener _tcpListener;
        private Thread _listeningThread;

        private volatile bool _shouldStop;

        private ManualResetEvent _tcpClientConnected = new ManualResetEvent(false); // thread signal

        public NetServer(IMessageReceiver messageReceiver, Encoding encoding, IConfigProvider configProvider,
            ILog log)
        {
            _messageReceiver = messageReceiver;
            _encoding        = encoding;
            _configProvider  = configProvider;
            _log             = log;
        }

        public void Start()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _configProvider.Port);
            _listeningThread = new Thread(new ThreadStart(ListenForConnections));

            _shouldStop = false;
            _listeningThread.Start();

            _log.Info("Started.");
        }

        public void Stop()
        {
            _shouldStop = true;
            _tcpClientConnected.Set(); // break waiting for connection
            _listeningThread.Join();
            _tcpListener.Stop();

            _log.Info("Stopped.");
        }

        private void ListenForConnections()
        {
            _tcpListener.Start();
            
            while (!_shouldStop)
            {
                _tcpClientConnected.Reset();
                _tcpListener.BeginAcceptTcpClient(new AsyncCallback(HandleIncomingConnection), _tcpListener);
                _tcpClientConnected.WaitOne(); // wait for connection
            }
        }

        private void HandleIncomingConnection(IAsyncResult asyncResult)
        {
            try
            {
                var listener = (TcpListener)asyncResult.AsyncState;
                var tcpClient = (TcpClient)listener.EndAcceptTcpClient(asyncResult);

                _log.InfoFormat("Connected: {0}", tcpClient.Client.AddressFamily.ToString());

                _tcpClientConnected.Set(); // run waiting for next connection

                var stream = tcpClient.GetStream();

                var requestBuffer = stream.ReadBuffered(0);
                var request = _encoding.GetString(requestBuffer, 0, requestBuffer.Length);

                var response = _messageReceiver.Dispatch(request);

                byte[] responseBuffer = _encoding.GetBytes(response + NetServer.ETB);
                stream.WriteBuffered(responseBuffer, 0, responseBuffer.Length);

                tcpClient.Close();
            }
            catch(ObjectDisposedException ex)
            {
                //todo: connection closed, logi
            }
        }

    }
}
