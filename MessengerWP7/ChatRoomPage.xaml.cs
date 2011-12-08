using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Messenger
{
    public partial class ChatRoomPage : PhoneApplicationPage
    {
        Message message;

        public ChatRoomPage()
        {
            InitializeComponent();
            BeginReceive();
        }

        private void BeginReceive()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(new byte[5], 0, 5);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(HeaderReceived);
            if (!App.ConnectedSocket.ReceiveAsync(args)) HeaderReceived(null, args);
        }

        private void HeaderReceived(object sender, SocketAsyncEventArgs e)
        {
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(e.Buffer, 0));
            MessageType type = (MessageType)e.Buffer[4];
            message = new Message(type, new byte[length]);
            //receive the content of the message
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(message.Content, 0, length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(ContentReceived);
            if (!App.ConnectedSocket.ReceiveAsync(args)) ContentReceived(null, args);
        }

        private void ContentReceived(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred < e.Count)
            {
                e.SetBuffer(e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred);
                if (!App.ConnectedSocket.ReceiveAsync(e)) ContentReceived(e.ConnectSocket, e);
            }
            else
            {
                string content = message.GetContentAsString();
                message = null;
                Dispatcher.BeginInvoke(() => ChatLogBox.Items.Add(content));
                BeginReceive();
            }
        }
    }
}