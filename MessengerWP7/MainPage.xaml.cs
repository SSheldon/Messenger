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
    public partial class MainPage : PhoneApplicationPage
    {
        Message message;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void LogInClick(object sender, RoutedEventArgs e)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            // set up endpoint for connect
            int split = this.Host.Text.IndexOf(':');
            int port;
            if (!int.TryParse(this.Host.Text.Substring(split + 1), out port)) return;
            args.RemoteEndPoint = new DnsEndPoint(this.Host.Text.Substring(0, split), port);
            // set up buffer for send after connect
            Message message = new Message(MessageType.Login, Message.Encoding.GetBytes(this.Username.Text));
            byte[] buffer = message.GetBytes();
            args.SetBuffer(buffer, 0, buffer.Length);
            // set up callback
            args.Completed += new EventHandler<SocketAsyncEventArgs>(LogInCompleted);
            // begin connect
            if (!sock.ConnectAsync(args))
                LogInCompleted(args.ConnectSocket, args);
        }

        private void LogInCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                //clean up
                if (e.ConnectSocket != null)
                {
                    e.ConnectSocket.Shutdown(SocketShutdown.Both);
                    e.ConnectSocket.Close();
                }
            }
            else
            {
                //connected and data successfully sent
                App.ConnectedSocket = e.ConnectSocket;
                //request to get rooms
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                Message m = new Message(MessageType.GetRooms, null);
                byte[] buffer = m.GetBytes();
                args.SetBuffer(buffer, 0, buffer.Length);
                args.Completed += new EventHandler<SocketAsyncEventArgs>(GetRoomsRequestSent);
                if (!App.ConnectedSocket.SendAsync(args)) GetRoomsRequestSent(null, args);
            }
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
                //show the chat page
                Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/RoomListPage.xaml", UriKind.Relative)));
            }
        }
    }
}