using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetApi
{
    class Server
    {
        public Core.UdpListener server;
        CancellationTokenSource cts;

        public delegate void ReceivedMessage(string Message, object Data);
        public event ReceivedMessage OnDataReceived;

        public Server(int Port)
        {
            server = new Core.UdpListener(555);
            StartListening();
        }

        private void StartListening()
        {
            Task.Factory.StartNew(async () =>
            {
                {
                    while (true)
                    {
                        try
                        {
                            cts = new CancellationTokenSource();
                            var received = await server.Receive(cts.Token);
                            if(received.Sender != null && received.ReceivedObj != null)
                            {
                                OnDataReceived.Invoke(received.ReceivedObj.Message, received.ReceivedObj.Data);
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    }
                }
            });
        }
    }
}
