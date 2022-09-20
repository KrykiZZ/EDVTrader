using Avalonia.Controls;
using ReactiveUI;
using System.Reactive;

namespace EDVTrader.ViewModels
{
    public class EnterSystemWindowViewModel : ViewModelBase
    {
        private string? _systemName;
        public string? SystemName
        {
            get => _systemName;
            set => this.RaiseAndSetIfChanged(ref _systemName, value);
        }

        public ReactiveCommand<Window, Unit> ChangeSystem { get; }

        public EnterSystemWindowViewModel(string? currentSystem)
        {
            ChangeSystem = ReactiveCommand.Create<Window>(OnChangeSystem);
            SystemName = currentSystem;
        }

        private void OnChangeSystem(Window window) => window.Close();
    }
}
