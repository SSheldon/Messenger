using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Messenger
{
    public enum MessageType : byte
    {
        MessagePost, Login, GetRooms, CreateRoom, JoinRoom, LeaveRoom
    }

    public struct RoomInfo
    {
        public byte id;
        public byte members;
        public string name;

        public byte NameLength
        {
            get { return (byte)Message.Encoding.GetByteCount(name); }
        }
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

        public IEnumerable<RoomInfo> GetContentAsRoomInfos()
        {
            for (int i = 0; i < length; i++)
            {
                RoomInfo info = new RoomInfo();
                info.id = content[i++];
                info.members = content[i++];
                int nameLength = content[i++];
                info.name = Message.Encoding.GetString(content, i, nameLength);
                i += nameLength;
                yield return info;
            }
        }

        public static byte[] GetRoomInfosAsBytes(IEnumerable<RoomInfo> infos)
        {
            int buffLen = 0;
            foreach (RoomInfo info in infos)
                buffLen += 3 + info.NameLength;
            byte[] buffer = new byte[buffLen];
            int i = 0;
            foreach (RoomInfo info in infos)
            {
                buffer[i++] = info.id;
                buffer[i++] = info.members;
                byte nameLength = info.NameLength;
                buffer[i++] = nameLength;
                Message.Encoding.GetBytes(info.name, 0, info.name.Length, buffer, i);
                i += nameLength;
            }
            return buffer;
        }
    }
}
