using Avalonia.Controls;
using Avalonia.Input;
using EDVTrader.ViewModels;

namespace EDVTrader.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnStationListKeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is MainWindowViewModel vm))
                return;

            vm.SelectedStation = null;
        }
    }
}
