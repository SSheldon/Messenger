﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Messenger
{
    public class Room
    {
        Server server;
        List<Client> clients;
        Queue<Message> pending;
        Thread poster;
        ManualResetEvent posterWait;

        public Room(Server server)
        {
            this.server = server;
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
                    Console.WriteLine(m.GetContentAsAsciiString());
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
        }

        public void RemoveClient(Client client)
        {
            lock (clients)
            {
                clients.Remove(client);
            }
        }

        public void PostMessage(Message message)
        {
            lock (pending)
            {
                pending.Enqueue(message);
                posterWait.Set();
            }
        }
    }
}
