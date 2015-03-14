﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ComputationalCluster.NetModule
{
    public interface INetClient
    {
        IMessage Send(IMessage message);
    }

    public class NetClient : INetClient
    {
        private readonly int _port = 3000; // todo: refactor

        private readonly IMessageTranslator _messageTranslator;
        private readonly Encoding _encoding;

        public NetClient(IMessageTranslator translator, Encoding encoding)
        {
            _messageTranslator = translator;
            _encoding          = encoding;
        }

        public IMessage Send(IMessage message)
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _port);

            var client = new TcpClient();
            client.Connect(serverEndPoint);
            //todo: buforowane wejście i wyjście
            var stream = client.GetStream();

            var request = _messageTranslator.Stringify(message);
            byte[] encodedRequest = _encoding.GetBytes(request);
            stream.Write(encodedRequest, 0, encodedRequest.Length);

            byte[] encodedResponse = new byte[4096 * 4];
            int responseLength = stream.Read(encodedResponse, 0, encodedResponse.Length);
            var response = _encoding.GetString(encodedResponse, 0, responseLength);
            //todo: serwer kończy połączenia?
            client.Close();

            var responseMessage = _messageTranslator.CreateObject(response);
            return responseMessage;
        }

    }
}