using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TCP_IP.Model;

namespace TCP_IP.View
{
    /// <summary>
    /// Логика взаимодействия для IpPortControl.xaml
    /// </summary>
    /// 
    public partial class IpPortControl : UserControl
    {
        private StatePlayer _statePlayer;
        private Server _server = null;
        private Client _client = null;
        private GameControl _gameControl;
        private MainControl _mainControl;

        public IpPortControl(StatePlayer statePlayer)
        {
            InitializeComponent();

            _statePlayer = statePlayer;
        }

        private void buttonConnectOrCreate_Click(object sender, RoutedEventArgs e)
        {
            buttonConnectOrCreate.IsEnabled = false;
            if(_statePlayer == StatePlayer.Client)
            {
                _client = new Client();
                _client.ConnectionFail += Client_ConnectionFail;
                _client.Connected += Client_Connected;
                _client.Connect(tbRemoteIP.Text, int.Parse(tbRemotePort.Text));
            }
            else
            {
                try
                {
                    _server = new Server();
                    _server.TwoConnections += Client_Connected;
                    _server.DataReceived += Server_DataReceived;
                    _server.Start(tbRemoteIP.Text, int.Parse(tbRemotePort.Text), 0);

                    _client = new Client();
                    _client.Connect(tbRemoteIP.Text, int.Parse(tbRemotePort.Text));
                }
                catch (Exception)
                {
                    MessageBox.Show("Данный порт занят, пожалуйста попробуйте другой!");
                    buttonConnectOrCreate.IsEnabled = true;
                }
            }
        }

        private void Client_ConnectionFail(Client c, string msg)
        {
            Dispatcher.Invoke(new Action(() => 
            {
                buttonConnectOrCreate.IsEnabled = true;
                MessageBox.Show("Ошибка подключения!");
            }));
        }

        private void Client_Connected(Client c, Socket s)
        {
            if (_statePlayer == StatePlayer.Server)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    _gameControl = new GameControl(_client, PlayerRole.Cross);
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(_gameControl);
                }));
            }
            else
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    _gameControl = new GameControl(_client, PlayerRole.Zero);
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(_gameControl);
                }));
            }
        }

        private void Server_DataReceived(Server s, ClientSocket cs, string str)
        {
            if (_server == null)
                return;
            if (str == "Отключение от сервера")
            {
                _mainControl = new MainControl();
                MainGrid.Children.Add(_mainControl);
                return;
            }
            s.SendForAll(str);
        }
    }

    public enum StatePlayer
    {
        Server, Client
    }
}
