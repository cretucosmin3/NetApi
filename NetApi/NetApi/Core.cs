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
//test
namespace NetApi
{
    class Core
    {
        public struct Received
        {
            public IPEndPoint Sender;
            public DataSent ReceivedObj;
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
                    //Message = Encoding.ASCII.GetString(content),
                    ReceivedObj = (DataSent)ByteArrayToObject(result.Buffer)
                };
            }

            // Convert a byte array to an Object
            public object ByteArrayToObject(byte[] arrBytes)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return binForm.Deserialize(memStream);
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

        [Serializable]
        public class DataSent
        {
            public string Message{ get; set; }
            public object Data { get; set; }
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
            
            public void Send(string message, object obj)
            {
                DataSent ToSend = new DataSent() { Message = message, Data = obj };
                var datagram = ObjectToByteArray(ToSend);
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
