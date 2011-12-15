using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Messenger
{
    public class Client
    {
        public static void Main(string[] args)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect("localhost", 4560);
            MessageOps.Send(sock, MessageType.Login, Message.Encoding.GetBytes("Tester"));
            // write all open rooms
            MessageOps.Send(sock, MessageType.GetRooms, null);
            Message m;
            do
            {
                m = MessageOps.Receive(sock);
            } while (m.Type != MessageType.GetRooms);
            foreach (RoomInfo info in m.GetContentAsRoomInfos())
                Console.WriteLine(info.id + " " + info.name + " (" + info.members + ")");
            // create and join new room
            MessageOps.Send(sock, MessageType.CreateRoom, null);
            while (true)
            {
                string s = Console.ReadLine();
                byte[] buffer = Message.Encoding.GetBytes(s);
                MessageOps.Send(sock, MessageType.MessagePost, buffer);
            }
        }
    }
}
