using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Messenger
{
    public class Server
    {
        private class RoomArray
        {
            Room[] rooms = new Room[byte.MaxValue + 1];
            int count = 0;
            byte firstOpenId = 0;

            public Room this[byte id]
            {
                get { return rooms[id]; }
            }

            public bool IsFull
            {
                get { return count == rooms.Length; }
            }

            public void Add(Room room)
            {
                if (room == null) return;
                rooms[firstOpenId] = room;
                count++;
                while (!IsFull && rooms[firstOpenId] != null)
                    firstOpenId = (byte)(firstOpenId == byte.MaxValue ? 0 : firstOpenId + 1);
            }

            public void Remove(byte id)
            {
                rooms[id] = null;
                if (IsFull) firstOpenId = id;
                else firstOpenId = Math.Min(firstOpenId, id);
                count--;
            }
        }

        Socket sock;
        RoomArray rooms;

        public Server(int port = 4560)
        {
            rooms = new RoomArray();
            CreateRoom();
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(new IPEndPoint(IPAddress.Any, port));
            sock.Listen(10);
        }

        public void Run()
        {
            while (true)
            {
                Socket clientSock = sock.Accept();
                new Client(this, clientSock).JoinRoom(rooms[0]);
            }
        }

        public Room GetRoom(byte id)
        {
            return rooms[id];
        }

        public Room CreateRoom()
        {
            if (rooms.IsFull) return null;
            Room room = new Room(this);
            rooms.Add(room);
            return room;
        }

        public static void Main(string[] args)
        {
            Server svr = new Server();
            svr.Run();
        }
    }
}
