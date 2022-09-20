using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Notification;
using EDVTrader.API;
using EDVTrader.Common;
using EDVTrader.Models;
using EDVTrader.Views;
using NLog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDVTrader.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private HttpClient _http;

        private JournalWatcher _watcher;
        private InaraAPI _inara;
        private Config? _config;

        public INotificationMessageManager NotificationManager { get; }
        private Notifications _notifications;

        #region Commander Info
        private string? _commanderName = "-";
        public string? CommanderName
        {
            get => _commanderName;
            set => this.RaiseAndSetIfChanged(ref _commanderName, value);
        }

        private string? _currentSystem = "-";
        public string? CurrentSystem
        {
            get => _currentSystem;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentSystem, value);
                this.RaisePropertyChanged(nameof(CurrentLocation));
            }
        }

        private string? _selectedSystem;
        public string? SelectedSystem
        {
            get => _selectedSystem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedSystem, value);
                Task.Factory.StartNew(UpdateStations);

                if (SelectedSystem != null && _config != null)
                {
                    _config.SelectedSystem = SelectedSystem;
                    _config.Save();
                }
            }
        }

        private string? _currentStation = null;
        public string? CurrentStation
        {
            get => _currentStation;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentStation, value);
                this.RaisePropertyChanged(nameof(CurrentLocation));
            }
        }

        private string? _currentBody = null;
        public string? CurrentBody
        {
            get => _currentBody;
            set
            {
                this.RaiseAndSetIfChanged(ref _currentBody, value);
                this.RaisePropertyChanged(nameof(CurrentLocation));
            }
        }


        public string CurrentLocation => $"{CurrentSystem}, {CurrentStation ?? CurrentBody}";

        private long _balance = 0;
        public long Balance
        {
            get => _balance;
            set
            {
                this.RaiseAndSetIfChanged(ref _balance, value);
                this.RaisePropertyChanged(nameof(DisplayBalance));
            }
        }

        public string DisplayBalance => Balance.ToDotted();
        #endregion

        #region Search Settings
        private List<Commodity> _totalCommodities = new();

        public ObservableCollection<Commodity> Commodities
        {
            get => new ObservableCollection<Commodity>(_totalCommodities.Where(x => x.Category.Id == SelectedCategory.Id));
        }

        private ObservableCollection<CommodityCategory> _categories = new();
        public ObservableCollection<CommodityCategory> Categories
        {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        private Commodity? _selectedCommodity;
        public Commodity? SelectedCommodity
        {
            get => _selectedCommodity;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedCommodity, value);
                Task.Factory.StartNew(UpdateStations);

                if (SelectedCommodity != null && _config != null)
                {
                    _config.Commodity = SelectedCommodity.Id;
                    _config.Save();
                }
            }
        }

        private CommodityCategory? _selectedCategory;
        public CommodityCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedCategory, value);
                this.RaisePropertyChanged(nameof(Commodities));

                if (SelectedCategory != null && _config != null)
                {
                    _config.Category = SelectedCategory.Id;
                    _config.Save();
                }
            }
        }

        public ObservableCollection<ShipSize> ShipSizes => new ObservableCollection<ShipSize>() { ShipSize.Small, ShipSize.Medium, ShipSize.Large };

        private ShipSize? _selectedShipSize;
        public ShipSize? SelectedShipSize
        {
            get => _selectedShipSize;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedShipSize, value);
                Task.Factory.StartNew(UpdateStations);

                if (_selectedShipSize != null)
                {
                    _config.ShipSize = _selectedShipSize.Value;
                    _config.Save();
                }
            }
        }

        private int _maxDistance;
        public string MaxDistance
        {
            get => _maxDistance == 0 ? "" : _maxDistance.ToString();
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    this.RaiseAndSetIfChanged(ref _maxDistance, 0);
                else
                {
                    if (int.TryParse(value, out int number))
                        this.RaiseAndSetIfChanged(ref _maxDistance, number);
                    else this.RaiseAndSetIfChanged(ref _maxDistance, 0);
                }

                _config.MaxDistance = _maxDistance;
                _config.Save();
            }
        }

        private bool _useFleetCarriers = false;
        public bool UseFleetCarriers
        {
            get => _useFleetCarriers;
            set
            {
                this.RaiseAndSetIfChanged(ref _useFleetCarriers, value);
                Task.Factory.StartNew(UpdateStations);

                _config.UseFleetCarriers = UseFleetCarriers;
                _config.Save();
            }
        }

        private bool _usePlanetaryStations = false;
        public bool UsePlanetaryStations
        {
            get => _usePlanetaryStations;
            set
            {
                this.RaiseAndSetIfChanged(ref _usePlanetaryStations, value);
                Task.Factory.StartNew(UpdateStations);

                _config.UsePlanetaryStations = UsePlanetaryStations;
                _config.Save();
            }
        }
        #endregion

        #region Profit Calculation
        private int _questReward;
        public string QuestReward
        {
            get => _questReward == 0 ? "" : _questReward.ToString();
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    this.RaiseAndSetIfChanged(ref _questReward, 0);
                else
                {
                    if (int.TryParse(value, out int number))
                        this.RaiseAndSetIfChanged(ref _questReward, number);
                    else this.RaiseAndSetIfChanged(ref _questReward, 0);
                }

                UpdateProfit();
            }
        }

        private int _questRequirement;
        public string QuestRequirement
        {
            get => _questRequirement == 0 ? "" : _questRequirement.ToString();
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    this.RaiseAndSetIfChanged(ref _questRequirement, 0);
                else
                {
                    if (int.TryParse(value, out int number))
                        this.RaiseAndSetIfChanged(ref _questRequirement, number);
                    else this.RaiseAndSetIfChanged(ref _questRequirement, 0);
                }

                UpdateProfit();
            }
        }

        //

        public string DisplayQuestProfit => (_questReward - QuestBalanceRequirement).ToDotted();

        public int QuestBalanceRequirement
        {
            get
            {
                if (SelectedStation != null)
                    return _questRequirement * SelectedStation.Price;
                else
                    return Stations.Count == 0 ? 0 : (int)(Stations.Select(x => x.Price).Average() * _questRequirement);
            }
        }

        public string DisplayQuestBalanceRequirement => QuestBalanceRequirement.ToDotted();
        #endregion

        #region Top Menu
        public string DisplayAveragePrice => (Stations.Count == 0 ? 0 : (int)Stations.Select(x => x.Price).Average()).ToDotted();
        public string DisplayCheapestPrice => (Stations.Count == 0 ? 0 : Stations.Min(x => x.Price)).ToDotted();
        public string DisplayDearestPrice => (Stations.Count == 0 ? 0 : Stations.Max(x => x.Price)).ToDotted();
        #endregion

        private ObservableCollection<StationInfo> _stations = new();

        public ObservableCollection<StationInfo> Stations
        {
            get => _stations;
            set
            {
                this.RaiseAndSetIfChanged(ref _stations, value);
                UpdateProfit();
            }
        }

        private StationInfo? _selectedStation;
        public StationInfo? SelectedStation
        {
            get => _selectedStation;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedStation, value);
                UpdateProfit();
            }
        }

        public ReactiveCommand<object, object> RefreshStationsListCommand { get; }
        public ReactiveCommand<string, Unit> LoadCommodityPreset { get; }

        public ReactiveCommand<Window, Unit> ChangeSystem { get; }
        public ReactiveCommand<Unit, Unit> UseCurrentSystem { get; }


        public MainWindowViewModel()
        {
            RefreshStationsListCommand = ReactiveCommand.Create<object, object>(OnRefreshStationsListClicked);
            LoadCommodityPreset = ReactiveCommand.Create<string>(OnLoadCommodityPresetClicked);
            ChangeSystem = ReactiveCommand.Create<Window>(OnChangeSystem);
            UseCurrentSystem = ReactiveCommand.Create(OnUseCurrentSystem);

            NotificationManager = new NotificationMessageManager();
            _notifications = new Notifications(NotificationManager);

            _http = new HttpClient();
            _inara = new InaraAPI(_http);

            _watcher = new JournalWatcher(JournalAPI.JournalsDirectory);
            _watcher.OnChange += _watcher_OnChange;

            CommanderInfo? commander = JournalAPI.GetCommanderInfo();
            if (commander != null)
            {
                CommanderName = commander.Name;
                CurrentSystem = commander.System;
                CurrentStation = commander.Station;
                CurrentBody = commander.Body;
                Balance = commander.Balance;
            }

            this.RaisePropertyChanged(nameof(Stations));
            Task.Factory.StartNew(async () =>
            {
                var result = await _http.GetAsync("https://eddb.io/archive/v6/commodities.json");
                
                var commodities = JsonSerializer.Deserialize<List<Commodity>?>(await result.Content.ReadAsStringAsync());
                var commodityIds = await _inara.GetCommodityIds();

                if (commodities == null)
                    return;

                if (commodityIds.Status != InaraResultStatus.Success || commodityIds.Result == null)
                {
                    _notifications.ShowError("Failed to access Inara.", "The program cannot function when Inara is unavailable.", "Exit", (b) => { Environment.Exit(0); });
                    return;
                }

                foreach (var id in commodityIds.Result)
                {
                    var commodity = commodities.FirstOrDefault(x => x.Name == id.Value);
                    if (commodity == null)
                        continue;

                    commodity.InaraId = id.Key;
                }

                foreach (Commodity commodity in commodities)
                {
                    if (commodity.InaraId != 0)
                        _totalCommodities.Add(commodity);
                }

                foreach (CommodityCategory category in commodities.Select(x => x.Category).DistinctBy(x => x.Id))
                    Categories.Add(category);


                var config = Config.Read();
                if (config != null)
                {
                    _config = config;

                    MaxDistance = _config.MaxDistance.ToString();
                    SelectedShipSize = _config.ShipSize;

                    UseFleetCarriers = _config.UseFleetCarriers;
                    UsePlanetaryStations = _config.UsePlanetaryStations;

                    SelectedCategory = Categories.FirstOrDefault(x => x.Id == _config.Category);
                    SelectedCommodity = Commodities.FirstOrDefault(x => x.Id == _config.Commodity);

                    SelectedSystem = _config.SelectedSystem;
                }
                else _config = new Config();

                if (string.IsNullOrWhiteSpace(SelectedSystem))
                    SelectedSystem = CurrentSystem;
            });
        }

        private void _watcher_OnChange(string data)
        {
            try
            {
                JsonDocument doc = JsonDocument.Parse(data);

                string? eventType = doc.RootElement.GetProperty("event").GetString();
                if (eventType == null)
                    return;

                switch (eventType)
                {
                    case "RestockVehicle":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "SellDrones":
                    {
                        Balance += doc.RootElement.GetProperty("SellPrice").GetInt64();
                        break;
                    }
                    case "ShipyardBuy":
                    {
                        Balance -= doc.RootElement.GetProperty("ShipPrice").GetInt64();
                        Balance += doc.RootElement.GetProperty("SellPrice").GetInt64();
                        break;
                    }
                    case "ShipyardSell":
                    {
                        Balance += doc.RootElement.GetProperty("ShipPrice").GetInt64();
                        break;
                    }
                    case "ModuleBuy":
                    {
                        Balance -= doc.RootElement.GetProperty("BuyPrice").GetInt64();
                        Balance += doc.RootElement.GetProperty("SellPrice").GetInt64();
                        break;
                    }
                    case "ModuleSell":
                    {
                        Balance += doc.RootElement.GetProperty("SellPrice").GetInt64();
                        break;
                    }
                    case "ModuleSellRemote":
                    {
                        Balance += doc.RootElement.GetProperty("SellPrice").GetInt64();
                        break;
                    }
                    case "PayFines":
                    {
                        Balance -= doc.RootElement.GetProperty("Amount").GetInt64();
                        break;
                    }
                    case "PayLegacyFines":
                    {
                        Balance -= doc.RootElement.GetProperty("Amount").GetInt64();
                        break;
                    }
                    case "RedeemVoucher":
                    {
                        Balance += doc.RootElement.GetProperty("Amount").GetInt64();
                        break;
                    }
                    case "RefuelAll":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "RefuelPartial":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "Repair":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "RepairAll":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "MissionCompleted":
                    {
                        Balance += doc.RootElement.GetProperty("Reward").GetInt64();
                        break;
                    }
                    case "MarketBuy":
                    {
                        Balance -= doc.RootElement.GetProperty("BuyPrice").GetInt64();
                        break;
                    }
                    case "BuyTradeData":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "SellExplorationData":
                    {
                        Balance += doc.RootElement.GetProperty("BaseValue").GetInt64() + doc.RootElement.GetProperty("Bonus").GetInt64();
                        break;
                    }
                    case "BuyExplorationData":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "CrewHire":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "LoadGame":
                    {
                        Balance = doc.RootElement.GetProperty("Credits").GetInt64();
                        break;
                    }
                    case "Location":
                    {
                        CurrentSystem = doc.RootElement.GetProperty("StarSystem").GetString();
                        break;
                    }
                    case "FSDJump":
                    {
                        CurrentSystem = doc.RootElement.GetProperty("StarSystem").GetString();
                        break;
                    }
                    case "BuyDrones":
                    {
                        Balance -= doc.RootElement.GetProperty("TotalCost").GetInt64();
                        break;
                    }
                    case "BuyAmmo":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "MarketSell":
                    {
                        Balance += doc.RootElement.GetProperty("TotalSale").GetInt64();
                        break;
                    }
                    case "PowerplaySalary":
                    {
                        Balance += doc.RootElement.GetProperty("Amount").GetInt64();
                        break;
                    }
                    case "DatalinkVoucher":
                    {
                        Balance += doc.RootElement.GetProperty("Reward").GetInt64();
                        break;
                    }
                    case "Resurrect":
                    {
                        Balance -= doc.RootElement.GetProperty("Cost").GetInt64();
                        break;
                    }
                    case "CommunityGoalReward":
                    {
                        Balance += doc.RootElement.GetProperty("Reward").GetInt64();
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error($"Failed to parse event. Data: {data}, Error: {ex}");
            }
        }

        private void OnUseCurrentSystem()
        {
            CommanderInfo? commander = JournalAPI.GetCommanderInfo();
            if (commander != null)
            {
                CommanderName = commander.Name;
                CurrentSystem = commander.System;
                CurrentStation = commander.Station;
                CurrentBody = commander.Body;
                Balance = commander.Balance;

                SelectedSystem = CurrentSystem;
            }
        }

        private async void OnChangeSystem(Window window)
        {
            EnterSystemWindow dialog = new EnterSystemWindow() { DataContext = new EnterSystemWindowViewModel(SelectedSystem) };
            await dialog.ShowDialog(window);

            if (!(dialog.DataContext is EnterSystemWindowViewModel vm))
                return;

            if (!string.IsNullOrWhiteSpace(vm.SystemName))
                SelectedSystem = vm.SystemName;
        }

        private void OnLoadCommodityPresetClicked(string commodity)
        {
            string category;
            switch(commodity)
            {
                case "Silver":
                case "Gold":
                case "Cobalt":
                {
                    category = "Metals";
                    break;
                }
                case "Bertrandite":
                case "Indite":
                {
                    category = "Minerals";
                    break;
                }
                case "Tritium":
                {
                    category = "Chemicals";
                    break;
                }
                default: return;
            }

            SelectedCategory = Categories.First(x => x.Name == category);
            SelectedCommodity = Commodities.First(x => x.Name == commodity);
        }

        private async Task<object> OnRefreshStationsListClicked(object sender)
        {
            await UpdateStations();
            return Task.CompletedTask;
        }

        public async Task UpdateStations()
        {
            try
            {
                if (SelectedSystem != null && SelectedShipSize != null && SelectedCommodity != null)
                {
                    var stations = await _inara.GetNearestCommodities(SelectedSystem, SelectedShipSize.Value, SelectedCommodity.InaraId, _maxDistance, UseFleetCarriers, UsePlanetaryStations);
                    if (stations.Status != InaraResultStatus.Success || stations.Result == null)
                    {
                        Stations = new ObservableCollection<StationInfo>();
                        _notifications.ShowError("Failed to access Inara.", "The program cannot function when Inara is unavailable.", "Exit", (b) => { Environment.Exit(0); });
                    }
                    else
                    {
                        Stations = new ObservableCollection<StationInfo>(stations.Result);
                        if (stations.Result.Count == 0) _notifications.ShowWarning("Nothing found.", "No stations found for your criteria.");
                    }

                    this.RaisePropertyChanged(nameof(DisplayAveragePrice));
                    this.RaisePropertyChanged(nameof(DisplayCheapestPrice));
                    this.RaisePropertyChanged(nameof(DisplayDearestPrice));
                }
            }
            catch(Exception ex)
            {
                _logger.Error($"An exception thrown while updating stations:\n{ex}");
            }
        }
        
        public void UpdateProfit()
        {
            this.RaisePropertyChanged(nameof(DisplayQuestBalanceRequirement));
            this.RaisePropertyChanged(nameof(DisplayQuestProfit));
        }
    }
}
