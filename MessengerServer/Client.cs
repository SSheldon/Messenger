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
            while (true)
            {
                Message message = Message.Receive(sock);
                Console.WriteLine(message.GetContentAsAsciiString());
            }
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }
    }
}