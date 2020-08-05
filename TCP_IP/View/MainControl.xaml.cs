using System.Windows;
using System.Windows.Controls;
using Tic_Tac_Toe.View;

namespace Tic_Tac_Toe
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
