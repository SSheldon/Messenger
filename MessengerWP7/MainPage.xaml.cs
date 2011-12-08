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
                Dispatcher.BeginInvoke(() => MessageBox.Show("Sending successful!"));
            }
        }
    }
}