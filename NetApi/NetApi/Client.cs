using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetApi
{
    class Client
    {
        public Core.UdpUser client;
        CancellationTokenSource cts;
        public Client(string Adress, int Port, string ClientName)
        {
            client = Core.UdpUser.ConnectTo(Adress, Port);
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
                            var received = await client.Receive(cts.Token);
                        }
                        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                    }
                }
            });
        }
    }
}
