using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetApi
{
    class Core
    {
        public struct Received
        {
            public IPEndPoint Sender;
            public string Message;
        }

        // Base
        public class UdpBase
        {
            protected UdpClient Client;

            protected UdpBase()
            {
                Client = new UdpClient();
            }

            public async Task<Received> Receive(CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                var result = await Client.ReceiveAsync();
                return new Received()
                {
                    Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                    Sender = result.RemoteEndPoint
                };
            }

            public Received ReceiveQ(IPEndPoint ip)
            {
                byte[] content = Client.Receive(ref ip);
                return new Received()
                {
                    Sender = ip,
                    Message = Encoding.ASCII.GetString(content)
                };
            }
        }

        //Server
        public class UdpListener : UdpBase
        {
            private IPEndPoint _listenOn;

            public UdpListener(int port) : this(new IPEndPoint(IPAddress.Any, port))
            {
            }

            public UdpListener(IPEndPoint endpoint)
            {
                _listenOn = endpoint;
                Client = new UdpClient(_listenOn);
            }
            public void SendTo(string message, IPEndPoint endpoint)
            {
                var datagram = Encoding.ASCII.GetBytes(message);
                Client.Send(datagram, datagram.Length, endpoint);
            }

        }

        //Client
        public class UdpUser : UdpBase
        {
            private UdpUser() { }

            public static UdpUser ConnectTo(string hostname, int port)
            {
                var connection = new UdpUser();
                connection.Client.Connect(hostname, port);
                return connection;
            }

            public void Send(string message)
            {
                var datagram = Encoding.ASCII.GetBytes(message);
                Client.Send(datagram, datagram.Length);
            }

        }
    }
}
