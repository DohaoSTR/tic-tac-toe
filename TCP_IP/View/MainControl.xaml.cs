using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TCP_IP.View;

namespace TCP_IP
{
    /// <summary>
    /// Логика взаимодействия для MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        private IpPortControl _ipPortControlServer;
        private IpPortControl _ipPortControlClient;

        public MainControl()
        {
            InitializeComponent();

            _ipPortControlServer = new IpPortControl(StatePlayer.Server);
            _ipPortControlServer.buttonConnectOrCreate.Content = "Создать сервер";
            _ipPortControlClient = new IpPortControl(StatePlayer.Client);
            _ipPortControlClient.buttonConnectOrCreate.Content = "Подключится";
        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(_ipPortControlServer);
        }

        private void ConnectToServer_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Children.Clear();
            MainGrid.Children.Add(_ipPortControlClient);
        }
    }
}
