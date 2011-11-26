using System;
using System.Net;
using System.Net.Sockets;

namespace Messenger
{
    public enum MessageType : byte
    {
        MessagePost, Connect
    }

    public class Message
    {
        MessageType type;
        int length;
        byte[] content;

        public Message(MessageType type, byte[] content)
        {
            this.type = type;
            this.content = content;
            this.length = content.Length;
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[content.Length + 5];
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length)), bytes, 4);
            bytes[4] = (byte)type;
            Array.Copy(content, bytes, content.Length);
            return bytes;
        }

        public void Send(Socket sock)
        {
            byte[] message = GetBytes();
            for (int sent = 0; sent < message.Length; )
            {
                sent += sock.Send(message, sent, message.Length - sent, SocketFlags.None);
            }
        }

        public static void Send(MessageType type, byte[] content, Socket sock)
        {
            Message m = new Message(type, content);
            m.Send(sock);
        }

        public static Message Receive(Socket sock)
        {
            byte[] buffer = new byte[5];
            sock.Receive(buffer);
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
            MessageType type = (MessageType)buffer[4];
            buffer = new byte[length];
            for (int recv = 0; recv < length; )
            {
                recv += sock.Receive(buffer, recv, length - recv, SocketFlags.None);
            }
            sock.Receive(buffer);
            return new Message(type, buffer);
        }
    }
}
