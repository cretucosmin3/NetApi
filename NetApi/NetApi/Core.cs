using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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
            public object ReceivedObj;
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
                    //Message = Encoding.ASCII.GetString(content),
                    ReceivedObj = ByteArrayToObject(content)
                };
            }
            // Convert a byte array to an Object
            public Object ByteArrayToObject(byte[] arrBytes)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                Object obj = (Object)binForm.Deserialize(memStream);

                return obj;
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
            public void SendTo(object obj, IPEndPoint endpoint)
            {
                var datagram = ObjectToByteArray(obj);
                Client.Send(datagram, datagram.Length, endpoint);
                //var datagram = Encoding.ASCII.GetBytes(message);
                //Client.Send(datagram, datagram.Length, endpoint);

            }
            // Convert an object to a byte array
            public byte[] ObjectToByteArray(object obj)
            {
                if (obj == null)
                    return null;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }
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

            public void Send(object obj)
            {
                var datagram = ObjectToByteArray(obj);
                Client.Send(datagram, datagram.Length);
            }
            // Convert an object to a byte array
            public byte[] ObjectToByteArray(object obj)
            {
                if (obj == null)
                    return null;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }
            }
        }
    }
}
