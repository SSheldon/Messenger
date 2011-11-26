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

        public Client(Server server, Socket sock)
        {
            this.server = server;
            this.sock = sock;
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