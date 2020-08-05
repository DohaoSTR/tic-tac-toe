using System.Windows;

namespace Tic_Tac_Toe
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainControl _mainControl;

        public MainWindow()
        {
            InitializeComponent();

            _mainControl = new MainControl();
            MainGrid.Children.Add(_mainControl);
        }
    }
}
