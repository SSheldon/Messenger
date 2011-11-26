using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Messenger
{
    public class Client
    {
        Server server;
        Socket sock;
        Thread handler;

        public Client(Server server, Socket sock)
        {
            this.server = server;
            this.sock = sock;
            this.handler = new Thread(new ThreadStart(this.HandleConnection));
            handler.Start();
        }

        public void HandleConnection()
        {
            while (sock.Connected)
            {
                if (sock.Poll(1000, SelectMode.SelectRead) && sock.Connected)
                {
                    Console.WriteLine(Message.Receive(sock).GetContentAsAsciiString());
                }
            }
        }

        public void CloseConnection()
        {
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }
    }
}