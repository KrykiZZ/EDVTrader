using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EDVTrader.Views
{
    public partial class EnterSystemWindow : Window
    {
        public EnterSystemWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
