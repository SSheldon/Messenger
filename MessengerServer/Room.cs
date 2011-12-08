using System;
using System.Collections.Generic;
using System.Threading;

namespace Messenger
{
    public class Room
    {
        Server server;
        List<Client> clients;

        public Room(Server server)
        {
            this.server = server;
            clients = new List<Client>();
        }

        public void AddClient(Client client)
        {
            clients.Add(client);
        }

        public void RemoveClient(Client client)
        {
            clients.Remove(client);
        }
    }
}
