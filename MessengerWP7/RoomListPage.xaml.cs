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
    public partial class RoomListPage : PhoneApplicationPage
    {
        Message message;

        public RoomListPage()
        {
            InitializeComponent();
            MainListBox.ItemsSource = App.RoomInfos;
        }

        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //If selected index is -1 (no selection) do nothing
            if (MainListBox.SelectedIndex == -1) return;
            //get id of selected room
            byte roomId = App.RoomInfos[MainListBox.SelectedIndex].Id;
            //send join request for that room to server
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            Message m = new Message(MessageType.JoinRoom, new byte[]{ roomId });
            byte[] buffer = m.GetBytes();
            args.SetBuffer(buffer, 0, buffer.Length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(RoomJoined);
            if (!App.ConnectedSocket.SendAsync(args)) RoomJoined(null, args);
            //clear selected index
            MainListBox.SelectedIndex = -1;
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
                //join sent, show the chat page
                Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/ChatRoomPage.xaml", UriKind.Relative)));
            }
        }

        private void RefreshClick(object sender, EventArgs e)
        {
            //request to get rooms
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            Message m = new Message(MessageType.GetRooms, null);
            byte[] buffer = m.GetBytes();
            args.SetBuffer(buffer, 0, buffer.Length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(GetRoomsRequestSent);
            if (!App.ConnectedSocket.SendAsync(args)) GetRoomsRequestSent(null, args);
        }

        private void GetRoomsRequestSent(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred < e.Count)
            {
                e.SetBuffer(e.Offset + e.BytesTransferred, e.Count - e.BytesTransferred);
                if (!App.ConnectedSocket.SendAsync(e)) GetRoomsRequestSent(e.ConnectSocket, e);
            }
            else
            {
                //request sent, wait to receive response
                BeginReceive();
            }
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
            else if (message.Type != MessageType.GetRooms)
            {
                //not the message we were looking for, receive another
                BeginReceive();
            }
            else
            {
                App.RoomInfos = message.GetContentAsRoomInfos().ToList();
                message = null;
                Dispatcher.BeginInvoke(() => MainListBox.ItemsSource = App.RoomInfos);
            }
        }

        private void AddClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreateRoomPage.xaml", UriKind.Relative));
        }
    }
}