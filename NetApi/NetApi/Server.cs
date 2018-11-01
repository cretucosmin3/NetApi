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
                            if(received.ReceivedObj.Message == "fisier")
                            {
                                System.IO.File.WriteAllBytes("clonafisier.txt", (byte[])received.ReceivedObj.Data);
                            }
                        }
                        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    }
                }
            });
        }
    }
}
