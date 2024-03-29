﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Messenger
{
    public class Room
    {
        Server server;
        byte id;
        string name;
        List<Client> clients;
        Queue<Message> pending;
        Thread poster;
        ManualResetEvent posterWait;

        public byte Id
        {
            get { return id; }
        }
        public string Name
        {
            get { return name; }
        }
        public RoomInfo Info
        {
            get
            {
                RoomInfo info = new RoomInfo();
                info.id = id;
                info.members = (byte)clients.Count;
                info.name = name;
                return info;
            }
        }

        public Room(Server server, byte id, string name)
        {
            this.server = server;
            this.id = id;
            this.name = name;
            clients = new List<Client>();
            pending = new Queue<Message>();
            posterWait = new ManualResetEvent(true);
            poster = new Thread(new ThreadStart(PostMessages));
            poster.Start();
        }

        private void PostMessages()
        {
            while (posterWait.WaitOne())
            {
                while (true)
                {
                    Message m = null;
                    lock (pending)
                    {
                        if (pending.Count > 0)
                            m = pending.Dequeue();
                        else
                        {
                            posterWait.Reset();
                            break;
                        }
                    }
                    Console.WriteLine(m.GetContentAsString());
                    lock (clients)
                    {
                        foreach (Client c in clients)
                            c.SendMessage(m);
                    }
                }
            }
        }

        public void AddClient(Client client)
        {
            lock (clients)
            {
                clients.Add(client);
            }
            PostMessage(client.Username + " joined.");
        }

        public void RemoveClient(Client client)
        {
            lock (clients)
            {
                clients.Remove(client);
            }
            PostMessage(client.Username + " left.");
        }

        public void PostMessage(Message message)
        {
            lock (pending)
            {
                pending.Enqueue(message);
                posterWait.Set();
            }
        }

        public void PostMessage(string username, Message message)
        {
            string prepend = username + ": ";
            byte[] buffer = new byte[prepend.Length + message.Content.Length];
            Message.Encoding.GetBytes(prepend, 0, prepend.Length, buffer, 0);
            Array.Copy(message.Content, 0, buffer, prepend.Length, message.Content.Length);
            PostMessage(new Message(MessageType.MessagePost, buffer));
        }

        public void PostMessage(string message)
        {
            PostMessage(new Message(MessageType.MessagePost, Message.Encoding.GetBytes(message)));
        }
    }
}
