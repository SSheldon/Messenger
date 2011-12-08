using System;
using System.Net;
using System.Net.Sockets;

namespace Messenger
{
    public static class MessageOps
    {
        public static void Send(Socket sock, Message message)
        {
            byte[] buffer = message.GetBytes();
            for (int sent = 0; sent < buffer.Length; )
            {
                sent += sock.Send(buffer, sent, buffer.Length - sent, SocketFlags.None);
            }
        }

        public static void Send(Socket sock, MessageType type, byte[] content)
        {
            Message m = new Message(type, content);
            Send(sock, m);
        }

        public static Message Receive(Socket sock)
        {
            byte[] buffer = new byte[5];
            if (!Receive(sock, buffer)) return null;
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
            MessageType type = (MessageType)buffer[4];
            buffer = new byte[length];
            if (!Receive(sock, buffer)) return null;
            else return new Message(type, buffer);
        }

        private static bool Receive(Socket sock, byte[] buffer)
        {
            for (int recv = 0; recv < buffer.Length; )
            {
                try
                {
                    recv += sock.Receive(buffer, recv, buffer.Length - recv, SocketFlags.None);
                }
                catch (SocketException)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
