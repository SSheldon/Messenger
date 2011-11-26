using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Messenger
{
    public class Server
    {
        Socket sock;

        public Server(int port = 4560)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(new IPEndPoint(IPAddress.Any, port));
            sock.Listen(10);
        }

        public void Run()
        {
            while (true)
            {
                HandleCientConnection();
            }
        }

        public void HandleCientConnection()
        {
            Socket clientSock = sock.Accept();
            Client client = new Client(this, clientSock);
        }

        public static void Main(string[] args)
        {
            Server svr = new Server();
            svr.Run();
        }
    }
}
