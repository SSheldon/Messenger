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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Messenger
{
    public partial class CreateRoomPage : PhoneApplicationPage
    {
        public CreateRoomPage()
        {
            InitializeComponent();
        }

        private void CreateClick(object sender, RoutedEventArgs e)
        {
            //send create request for that room to server
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            Message m = new Message(MessageType.CreateRoom, Message.Encoding.GetBytes(InputBox.Text));
            byte[] buffer = m.GetBytes();
            args.SetBuffer(buffer, 0, buffer.Length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(RoomJoined);
            if (!App.ConnectedSocket.SendAsync(args)) RoomJoined(null, args);
        }

        private void RoomJoined(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred < e.Count)
            {
                e.SetBuffer(e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred);
                if (!App.ConnectedSocket.SendAsync(e)) RoomJoined(e.ConnectSocket, e);
            }
            else
            {
                //create sent, show the chat page
                Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/ChatRoomPage.xaml", UriKind.Relative)));
            }
        }
    }
}