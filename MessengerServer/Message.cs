using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Messenger
{
    public enum MessageType : byte
    {
        MessagePost, Login
    }

    public class Message
    {
        public static System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

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
        public byte[] Content
        {
            get { return content; }
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

        public string GetContentAsString()
        {
            return Message.Encoding.GetString(content, 0, length);
        }
    }
}
