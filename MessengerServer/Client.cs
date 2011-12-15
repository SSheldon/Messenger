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
        string username;

        public string Username
        {
            get { return username; }
        }

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
                        else break;
                    }
                    MessageOps.Send(sock, m);
                }
                if (sock.Poll(1000, SelectMode.SelectRead) && sock.Connected)
                {
                    HandleRequest(MessageOps.Receive(sock));
                }
            }
            CloseConnection();
            LeaveRoom();
        }

        private void HandleRequest(Message request)
        {
            if (request == null) return;
            switch (request.Type)
            {
                case MessageType.MessagePost:
                    room.PostMessage(username, request);
                    break;
                case MessageType.Login:
                    username = request.GetContentAsString();
                    break;
                case MessageType.GetRooms:
                    SendMessage(new Message(MessageType.GetRooms,
                        Message.GetRoomInfosAsBytes(server.GetRoomInfos())));
                    break;
                case MessageType.CreateRoom:
                    JoinRoom(server.CreateRoom(request.GetContentAsString()));
                    break;
                case MessageType.JoinRoom:
                    JoinRoom(server.GetRoom(request.Content[0]));
                    break;
                case MessageType.LeaveRoom:
                    LeaveRoom();
                    break;
            }
        }

        public void CloseConnection()
        {
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
            sock = null;
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