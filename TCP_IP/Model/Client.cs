using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace TCP_IP.Model
{
    public class Client
    {
        private Socket ServerSocket = null;
        public delegate void OnConnectedHandler(Client c, Socket s);
        public delegate void OnSentHandler(Client c, int bytesCount);
        public delegate void OnDataReceivedHandler(Client c, string str);
        public delegate void OnConnectionFailHandler(Client c, string msg);
        public event OnConnectedHandler Connected;
        public event OnConnectionFailHandler ConnectionFail;
        public event OnSentHandler Sent;
        public event OnDataReceivedHandler DataReceived;

        public void Connect(string ip, int port)
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.BeginConnect(
                new IPEndPoint(IPAddress.Parse(ip), port),
                new AsyncCallback(ConnectCallback),
                ServerSocket
            );
        }

        private void ConnectCallback(IAsyncResult async_result)
        {
            Socket socket = (Socket)async_result.AsyncState;
            if (!socket.Connected) { OnConnectionFail("Client can't connect"); return; }
            socket.EndConnect(async_result);
            OnConnected(socket);
            StartReceiving(socket);
        }

        private void OnConnectionFail(string msg)
        {
            if (ConnectionFail == null) return;
            ConnectionFail(this, msg);
        }

        public void Disconnect()
        {
            Send("Отключение от сервера");
        }

        public void Send(string str)
        {
            if (ServerSocket == null) throw new Exception("Client not connected!");
            byte[] byteData = Encoding.UTF8.GetBytes(str + "<EOF>");
            ServerSocket.BeginSend(
                byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback),
                ServerSocket
            );
        }

        private void SendCallback(IAsyncResult async_result)
        {
            Socket socket = (Socket)async_result.AsyncState;
            int bytesSent = socket.EndSend(async_result);
            OnSent(bytesSent);
        }

        private void StartReceiving(Socket s)
        {
            ClientSocket SocketState = new ClientSocket();
            SocketState.Handler = s;
            s.BeginReceive(
                SocketState.Buffer, 0, ClientSocket.BufferSize,
                SocketFlags.None,
                new AsyncCallback(ReceiveCallback),
                SocketState
            );
        }

        private void ReceiveCallback(IAsyncResult async_result)
        {
            ClientSocket SocketState = (ClientSocket)async_result.AsyncState;
            Socket socket = SocketState.Handler;
            int bytesRead = socket.EndReceive(async_result);
            if (bytesRead > 0)
            {
                SocketState.BufferString += Encoding.UTF8.GetString(SocketState.Buffer, 0, bytesRead);
                if (SocketState.BufferString.IndexOf("<EOF>") > -1)
                {
                    SocketState.BufferString = SocketState.BufferString.Replace("<EOF>", "");
                    OnDataReceived(SocketState.BufferString);
                    SocketState.BufferString = "";
                }
                socket.BeginReceive(
                    SocketState.Buffer, 0, ClientSocket.BufferSize,
                    SocketFlags.None,
                    new AsyncCallback(ReceiveCallback),
                    SocketState
                );
            }
        }

        private void OnConnected(Socket s)
        {
            if (Connected == null) return;
            Connected(this, s);
        }

        private void OnSent(int bytes)
        {
            if (Sent == null) return;
            Sent(this, bytes);
        }

        private void OnDataReceived(string str)
        {
            if (DataReceived == null) return;
            DataReceived(this, str);
        }
    }
}
