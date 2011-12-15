using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Messenger
{
    public class Room
    {
        Server server;
        byte id;
        List<Client> clients;
        Queue<Message> pending;
        Thread poster;
        ManualResetEvent posterWait;

        public byte Id
        {
            get { return id; }
        }

        public Room(Server server, byte id)
        {
            this.server = server;
            this.id = id;
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

        public void PostMessage(string username, Message message)
        {
            string prepend = username + ": ";
            byte[] buffer = new byte[prepend.Length + message.Content.Length];
            Message.Encoding.GetBytes(prepend, 0, prepend.Length, buffer, 0);
            Array.Copy(message.Content, 0, buffer, prepend.Length, message.Content.Length);
            PostMessage(new Message(MessageType.MessagePost, buffer));
        }
    }
}
