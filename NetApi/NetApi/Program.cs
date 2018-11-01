﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetApi
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Server sv = new Server(555);
            Client cl = new Client("192.168.0.7", 555, "jeorje");
            

            var FileToSend = File.ReadAllBytes(@"C:\Users\Cosmin\Desktop\UClient.txt");
            cl.client.Send("fisier", FileToSend);
            Console.Read();
        }
    }
    
    [Serializable]
    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
