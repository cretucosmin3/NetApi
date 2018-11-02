using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
                var DataR = (DataSent)Tools.ByteArrayToObject(Tools.Decompress(result.Buffer));
                if (DataR.Checksum == Tools.ComputeAdditionChecksum(Tools.ObjectToByteArray(DataR.Message)) +
                                        Tools.ComputeAdditionChecksum(Tools.ObjectToByteArray(DataR.Data)))
                {
                    return new Received()
                    {
                        Sender = result.RemoteEndPoint,
                        ReceivedObj = DataR
                    };
                }

                return new Received() { Sender = null, ReceivedObj = null };
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
            public void SendTo(string msg,object obj, IPEndPoint endpoint)
            {
                DataSent ToSend = new DataSent()
                {
                    Message = msg,
                    Data = obj,
                    Checksum = Tools.ComputeAdditionChecksum(Tools.ObjectToByteArray(msg)) +
                                Tools.ComputeAdditionChecksum(Tools.ObjectToByteArray(obj))
                };
                var datagram = Tools.Compress(Tools.ObjectToByteArray(ToSend));
                Client.Send(datagram, datagram.Length, endpoint);

            }

        }

        [Serializable]
        public class DataSent
        {
            public string Message{ get; set; }
            public object Data { get; set; }
            public int Checksum { get; set; }
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
                DataSent ToSend = new DataSent()
                {
                    Message = message,
                    Data = obj,
                    Checksum = Tools.ComputeAdditionChecksum(Tools.ObjectToByteArray(message)) +
                                Tools.ComputeAdditionChecksum(Tools.ObjectToByteArray(obj))
                };
                var datagram = Tools.Compress(Tools.ObjectToByteArray(ToSend));
                Client.Send(datagram, datagram.Length);
            }
            
        }

        public static class Tools
        {
            // Convert an object to a byte array
            public static byte[] ObjectToByteArray(object obj)
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

            // Convert a byte array to an Object
            public static object ByteArrayToObject(byte[] arrBytes)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return binForm.Deserialize(memStream);
            }

            public static byte[] Compress(byte[] data)
            {
                MemoryStream output = new MemoryStream();
                using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
                {
                    dstream.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }

            public static byte[] Decompress(byte[] data)
            {
                MemoryStream input = new MemoryStream(data);
                MemoryStream output = new MemoryStream();
                using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    dstream.CopyTo(output);
                }
                return output.ToArray();
            }

            public static byte ComputeAdditionChecksum(byte[] data)
            {
                byte sum = 0;
                unchecked // Let overflow occur without exceptions
                {
                    foreach (byte b in data)
                    {
                        sum += b;
                    }
                }
                return sum;
            }
        }

    }
}
