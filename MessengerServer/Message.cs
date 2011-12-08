using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Messenger
{
    public enum MessageType : byte
    {
        MessagePost, Connect
    }

    public class Message
    {
        MessageType type;
        byte[] content;
        int length
        {
            get { return content.Length; }
        }

        public MessageType Type
        {
            get { return type; }
        }

        public Message(MessageType type, byte[] content)
        {
            this.type = type;
            this.content = content;
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[length + 5];
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length)), bytes, 4);
            bytes[4] = (byte)type;
            Array.Copy(content, 0, bytes, 5, length);
            return bytes;
        }

        public string GetContentAsAsciiString()
        {
            return Encoding.ASCII.GetString(content, 0, length);
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
            return new Message(type, buffer);
        }
    }
}
