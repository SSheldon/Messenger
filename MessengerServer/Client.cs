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
        Room room;
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
                    HandleRequest(Message.Receive(sock));
                }
            }
        }

        private void HandleRequest(Message request)
        {
            switch (request.Type)
            {
                case MessageType.MessagePost:
                    Console.WriteLine(request.GetContentAsAsciiString());
                    break;
            }
        }

        public void CloseConnection()
        {
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }

        public void JoinRoom(Room room)
        {
            if (this.room != null)
                this.room.RemoveClient(this);
            this.room = room;
            room.AddClient(this);
        }

        public void LeaveRoom()
        {
            room.RemoveClient(this);
            room = null;
        }
    }
}