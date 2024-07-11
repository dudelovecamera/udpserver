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
            int recv = 0;
            byte[] data = new byte[2048];
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            Socket newsock = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
            newsock.Bind(ipep);
            Console.WriteLine("Waiting for a client...");
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            // List<EndPoint> onlineUsers = new List<EndPoint>();
            Dictionary<string, EndPoint> onlineUsers = new Dictionary<string, EndPoint>();

            while (true)
            {
                data = new byte[2024];

                try
                {
                    recv = RcvData(newsock, data,
                        ref Remote);
                    if (recv <= 0)
                    {
                        Console.WriteLine("Did not receive an answer");
                        
                    }
                    
                    // recv = newsock.ReceiveFrom(data, ref Remote);

                    var msg = process(data, recv);

                    if (msg.Equals("userJoined"))
                    {
                        onlineUsers.Add(Remote.ToString(), Remote);
                        Console.WriteLine("new user joined");
                    }
                    
                    
                    foreach (var user in onlineUsers)
                    {
                        try
                        {
                            newsock.SendTo(data, recv, SocketFlags.None, user.Value);
                            Console.WriteLine("write to {0}:",
                                user.Key);
                        }
                        catch (SocketException e)
                        {
                            // // if (e.ErrorCode == 10054)
                            // // {
                            Console.WriteLine("shiiiiiitttt");
                            Console.WriteLine(e.Message);
                            //     onlineUsers.Remove(user.Key);
                            //     // }
                        }
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine(Remote.ToString());
                    Console.WriteLine(e.Source);
                }


                
            }
        }

        private static string? process(byte[] data, int recv)
        {
            var jsonWelcome = Encoding.ASCII.GetString(data, 0, recv);
            var message = JsonSerializer.Deserialize<Message>(jsonWelcome);

            Console.WriteLine(jsonWelcome);
            return message.Summary;
        }
        
        private static int RcvData(Socket s, byte[] message,
            ref EndPoint Remote)
        {
            int recv = 0;
            int retry = 0;
            while (true)
            {
                try
                {
                    Console.WriteLine("socker number {0}",Remote.ToString());

                    recv = s.ReceiveFrom(message, ref Remote);
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10054)
                    {
                        recv = 0;
                        Console.WriteLine("Error receiving 10054"); 
                    }
                    else if (e.ErrorCode == 10040)
                    {
                        Console.WriteLine("Error receiving packet");
                        recv = 0;
                    }
                }

                if (recv > 0)
                {
                    return recv;
                }
                else
                {
                    retry++;
                    if (retry > 4)
                    {
                        return 0;
                    }
                }
            }
        }
    }
}