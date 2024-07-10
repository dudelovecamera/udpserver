using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Net; 
using System.Net.Sockets;
using System.Text.Json;
using udpclient;


namespace SimpleUdpSrvr 
{ 
    class Program 
    { 
        static void Main(string[] args) 
        { 
            int recv; 
            byte[] data = new byte[2048]; 
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050); 
            Socket newsock = new Socket(AddressFamily.InterNetwork, 
                SocketType.Dgram, ProtocolType.Udp); 
            newsock.Bind(ipep); 
            Console.WriteLine("Waiting for a client..."); 
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0); 
            EndPoint Remote = (EndPoint)(sender); 
            recv = newsock.ReceiveFrom(data, ref Remote); 
            Console.WriteLine("Message received from {0}:", 
                Remote.ToString()); 
            
            process(data, recv);


            string welcome = "Welcome to my test server"; 
            data = Encoding.ASCII.GetBytes(welcome); 
            newsock.SendTo(data, data.Length, SocketFlags.None, Remote); 
            while (true) 
            { 
                data = new byte[2024]; 
                recv = newsock.ReceiveFrom(data, ref Remote);

                process(data, recv);
                newsock.SendTo(data, recv, SocketFlags.None, Remote); 
            } 
        }

        private static void process(byte[] data, int recv)
        {
            var jsonWelcome = Encoding.ASCII.GetString(data, 0, recv);
            var message = JsonSerializer.Deserialize<Message>(jsonWelcome);

            Console.WriteLine(message.Date);
            Console.WriteLine(jsonWelcome);
        }
    } 
}