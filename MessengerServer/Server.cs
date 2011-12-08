using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Messenger
{
    public class Server
    {
        Socket sock;
        Room room;

        public Server(int port = 4560)
        {
            room = new Room(this);
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(new IPEndPoint(IPAddress.Any, port));
            sock.Listen(10);
        }

        public void Run()
        {
            while (true)
            {
                Socket clientSock = sock.Accept();
                new Client(this, clientSock).JoinRoom(room);
            }
        }

        public static void Main(string[] args)
        {
            Server svr = new Server();
            svr.Run();
        }
    }
}
