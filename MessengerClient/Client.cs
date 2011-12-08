﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Messenger
{
    public class Client
    {
        public static void Main(string[] args)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect("localhost", 4560);
            MessageOps.Send(sock, MessageType.Login, Encoding.ASCII.GetBytes("Tester"));
            while (true)
            {
                string s = Console.ReadLine();
                byte[] buffer = Encoding.ASCII.GetBytes(s);
                MessageOps.Send(sock, MessageType.MessagePost, buffer);
            }
        }
    }
}
