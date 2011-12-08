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
        Queue<Message> pending;

        public Client(Server server, Socket sock)
        {
            this.server = server;
            this.sock = sock;
            this.pending = new Queue<Message>();
            this.handler = new Thread(new ThreadStart(this.HandleConnection));
            handler.Start();
        }

        public void HandleConnection()
        {
            while (sock.Connected)
            {
                while (true)
                {
                    Message m = null;
                    lock (pending)
                    {
                        if (pending.Count > 0)
                            m = pending.Dequeue();
                    }
                    if (m != null) m.Send(sock);
                    else break;
                }
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

        public void SendMessage(Message message)
        {
            lock (pending)
            {
                pending.Enqueue(message);
            }
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