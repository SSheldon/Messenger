using System;
using System.Collections.Generic;
using System.Linq;
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
                if (room == null || IsFull) return;
                rooms[firstOpenId] = room;
                count++;
                while (!IsFull && rooms[firstOpenId] != null)
                    firstOpenId = (byte)(firstOpenId == byte.MaxValue ? 0 : firstOpenId + 1);
            }

            public Room CreateAndAdd(Server svr)
            {
                if (IsFull) return null;
                Room room = new Room(svr, firstOpenId);
                Add(room);
                return room;
            }

            public void Remove(byte id)
            {
                rooms[id] = null;
                if (IsFull) firstOpenId = id;
                else firstOpenId = Math.Min(firstOpenId, id);
                count--;
            }

            public IEnumerable<Room> Elements()
            {
                for (int seen = 0, i = 0; seen < count; i++)
                {
                    if (rooms[i] == null) continue;
                    seen++;
                    yield return rooms[i];
                }
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
            return rooms.CreateAndAdd(this);
        }

        public void RemoveRoom(Room room)
        {
            rooms.Remove(room.Id);
        }

        public IEnumerable<RoomInfo> GetRoomInfos()
        {
            return from room in rooms.Elements() select room.Info;
        }

        public static void Main(string[] args)
        {
            Server svr = new Server();
            svr.Run();
        }
    }
}
