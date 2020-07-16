using System;
using System.Collections.Generic;
using System.Linq;
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
using TCP_IP.View;

namespace TCP_IP
{
    /// <summary>
    /// Логика взаимодействия для GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        private IpPortControl _ipPortControl;
        private Client _client = null;
        private PlayerRole _playerRole;
        private bool XMovement = true;

        private enum CellState
        {
            X, O, NotSelected
        }

        private CellState[] cellStates = new CellState[9]
           {
                CellState.NotSelected, CellState.NotSelected, CellState.NotSelected,
                CellState.NotSelected, CellState.NotSelected, CellState.NotSelected,
                CellState.NotSelected, CellState.NotSelected, CellState.NotSelected
           };

        private BitmapImage bmpImgCross;
        private BitmapImage bmpImgZero;

        private BitmapImage GetBitmapImage(Uri uri)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = uri;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public GameControl(Client client, PlayerRole playerRole)
        {
            InitializeComponent();

            _ipPortControl = new IpPortControl(StatePlayer.Client);
            _ipPortControl.buttonConnectOrCreate.Content = "Подключится";
            _playerRole = playerRole;
            _client = client;
            _client.DataReceived += Client_DataReceived;

            Uri uriImgZero = new Uri(@"pack://application:,,,/Resources/Nol.png", UriKind.Absolute);
            Uri uriImgCross = new Uri(@"pack://application:,,,/Resources/Krest.png", UriKind.Absolute);
            bmpImgZero = GetBitmapImage(uriImgZero);
            bmpImgCross = GetBitmapImage(uriImgCross);

            if (_playerRole == PlayerRole.Zero)
                XMovement = false;
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Image img = (Image)btn.Content;
            int btnNumber = Convert.ToInt32((string)btn.Tag);
            if (cellStates[btnNumber] != CellState.NotSelected)
                return;
            if (_playerRole == PlayerRole.Cross && cellStates[btnNumber] == CellState.NotSelected && XMovement)
            {
                img.Source = bmpImgCross;
                cellStates[btnNumber] = CellState.X;
                XMovement = false;
                SendMessage(_client, btnNumber, 0);
            }
            if (_playerRole == PlayerRole.Zero && cellStates[btnNumber] == CellState.NotSelected && XMovement)
            {
                img.Source = bmpImgZero;
                cellStates[btnNumber] = CellState.O;
                XMovement = false;
                SendMessage(_client, btnNumber, 1);
            }
        }

        private void Client_DataReceived(Client c, string str)
        {
            if (str == "Сервер заполнен!" || str == "Сервер выключен!")
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    MessageBox.Show(str);
                    MainGrid.Children.Clear();
                    MainGrid.Children.Add(_ipPortControl);
                })); ;
            }
            else
            {
                string cellStatesNum = Convert.ToString(str.First());
                string cellState = Convert.ToString(str.Last());
                if (cellStates[Convert.ToInt32(cellStatesNum)] != CellState.NotSelected)
                    return;
                if (Dispatcher.CheckAccess()) DrawingMoveOtherClient(Convert.ToInt32(cellStatesNum), Convert.ToInt32(cellState));
                else
                    Dispatcher.Invoke(() => DrawingMoveOtherClient(Convert.ToInt32(cellStatesNum), Convert.ToInt32(cellState)));
            }
        }

        private void SendMessage(Client client, int cellStatesNum, int cellState)
        {
            if (client != null)
            {
                string text = Convert.ToString(cellStatesNum + "" + cellState);
                client.Send(text);
            }
        }

        private void DrawingMoveOtherClient(int cellStatesNum, int cellState)
        {
            Button btn = null;
            switch (cellStatesNum)
            {
                case 0:
                    btn = b0;
                    break;
                case 1:
                    btn = b1;
                    break;
                case 2:
                    btn = b2;
                    break;
                case 3:
                    btn = b3;
                    break;
                case 4:
                    btn = b4;
                    break;
                case 5:
                    btn = b5;
                    break;
                case 6:
                    btn = b6;
                    break;
                case 7:
                    btn = b7;
                    break;
                case 8:
                    btn = b8;
                    break;
            }
            Image img = (Image)btn.Content;

            if (cellState == 0)
            {
                img.Source = bmpImgCross;
                cellStates[cellStatesNum] = CellState.X;
            }
            else if (cellState == 1)
            {
                img.Source = bmpImgZero;
                cellStates[cellStatesNum] = CellState.O;
            }
            XMovement = true;
        }
    }

    public enum PlayerRole
    {
        Cross, Zero
    }
}
