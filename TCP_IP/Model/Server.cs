using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace TCP_IP.Model
{
    class ClientSocket
    {
        public Socket Handler = null;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public string BufferString = string.Empty;
    }
    class Server
    {
        public List<ClientSocket> ClientConnections = new List<ClientSocket>();
        private Socket listener;
        private bool twoConnection = false;

        public delegate void OnTwoConnectionsHadler(Client c, Socket s);
        public delegate void OnReceiveDataHandler(Server s, ClientSocket cs, string str);
        public delegate void OnNewConnectHandler(Server s, ClientSocket cs);
        public event OnTwoConnectionsHadler TwoConnections;
        public event OnReceiveDataHandler DataReceived;
        public event OnNewConnectHandler NewConnect;

        public void Start(string ip, int port, int max)
        {
            byte[] bytes = new byte[1024];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Application.Current.Exit += Current_Exit;
            listener.Bind(localEndPoint);
            listener.Listen(max);
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            Stop();
        }

        public void SendForAll(string str)
        {
            foreach (ClientSocket c in ClientConnections)
            {
                try
                {
                    if (c.Handler == null) continue;
                    byte[] byteData = Encoding.UTF8.GetBytes(str + "<EOF>");
                    c.Handler.BeginSend(
                        byteData, 0, byteData.Length, 0, null, c.Handler
                    );
                }
                catch (SocketException) { }
            }
        }

        public void Stop()
        {
            SendForAll("Сервер выключен!");
            foreach (ClientSocket c in ClientConnections)
            {
                c.Handler.Shutdown(SocketShutdown.Both);
                c.Handler.Close();
            }
        }

        private void AcceptCallback(IAsyncResult async_result)
        {
            if (ClientConnections.Count < 2)
            {
                ClientSocket state = new ClientSocket();
                Socket listener = async_result.AsyncState as Socket;
                state.Handler = listener.EndAccept(async_result);
                ClientConnections.Add(state);
                OnNewConnect(state);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                state.Handler.BeginReceive(
                    state.Buffer, 0, ClientSocket.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state
                );
            }
            else if (ClientConnections.Count == 2)
            {
                ClientSocket state = new ClientSocket();
                Socket listener = async_result.AsyncState as Socket;
                state.Handler = listener.EndAccept(async_result);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                if (state.Handler == null) return;
                byte[] byteData = Encoding.UTF8.GetBytes("Сервер заполнен!" + "<EOF>");
                state.Handler.BeginSend(
                    byteData, 0, byteData.Length, 0, null, state.Handler
                );
                state.Handler.Shutdown(SocketShutdown.Both);
                state.Handler.Close();
            }
            if (ClientConnections.Count == 2 && twoConnection == false)
            {
                twoConnection = true;
                OnTwoConnections(null, null);
            }
        }


        private void ReadCallback(IAsyncResult async_result)
        {
            ClientSocket state = (ClientSocket)async_result.AsyncState;
            if (!state.Handler.Connected) return;
            int bytesRead = 0;
            try
            {
                bytesRead = state.Handler.EndReceive(async_result);
            }
            catch (Exception) { return; }
            if (bytesRead > 0)
            {
                state.BufferString += Encoding.UTF8.GetString(state.Buffer, 0, bytesRead);
                if (state.BufferString.IndexOf("<EOF>") > -1)
                {
                    state.BufferString = state.BufferString.Replace("<EOF>", "");
                    OnReceiveData(state, state.BufferString);
                    if (state.BufferString == "Отключение от сервера")
                    {
                        ClientConnections.Clear();
                        Stop();
                    }
                    state.BufferString = "";
                }
                state.Handler.BeginReceive(
                    state.Buffer, 0, ClientSocket.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state
                );
            }
        }

        private void OnReceiveData(ClientSocket cs, string str)
        {
            if (DataReceived == null) return;
            DataReceived(this, cs, str);
        }

        private void OnNewConnect(ClientSocket cs)
        {
            if (NewConnect == null) return;
            NewConnect(this, cs);
        }

        private void OnTwoConnections(Client c, Socket s)
        {
            if (TwoConnections == null) return;
            TwoConnections(c, s);
        }
    }
}
