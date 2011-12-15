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

        private void AddClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreateRoomPage.xaml", UriKind.Relative));
        }
    }
}