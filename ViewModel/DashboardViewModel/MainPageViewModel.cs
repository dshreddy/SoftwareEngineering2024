using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using Networking;
using Networking.Communication;
using System.Windows;
using System.Diagnostics;
using Dashboard;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace ViewModel.DashboardViewModel 
{
    public class GraphDataModel
    {
        public DateTime Time { get; set; }
        public int UserCount { get; set; }
    }
    public class MainPageViewModel : INotifyPropertyChanged  
    {
        private ICommunicator _communicator;
        private Server_Dashboard _serverSessionManager;
        private Client_Dashboard _clientSessionManager;

        // UserDetailsList is bound to the UI to display the participant list
        private ObservableCollection<UserDetails> _userDetailsList = new ObservableCollection<UserDetails>();
        public ObservableCollection<UserDetails> UserDetailsList
        {
            get { return _userDetailsList; }
            set
            {
                if (_userDetailsList != value)
                {
                    _userDetailsList = value;
                    OnPropertyChanged(nameof(UserDetailsList));
                }
            }
        }
        private ObservableCollection<GraphDataModel> _userCountData;
        public ObservableCollection<GraphDataModel> UserCountData
        {
            get => _userCountData;
            private set
            {
                _userCountData = value;
                OnPropertyChanged(nameof(UserCountData));
            }
        }

        public SeriesCollection SeriesCollection { get; private set; }
        private List<string> _timeLabels;
        public List<string> TimeLabels
        {
            get => _timeLabels;
            set
            {
                _timeLabels = value;
                OnPropertyChanged(nameof(TimeLabels));
            }
        }

        private double _axisMax;
        private double _axisMin;
        public double AxisMax
        {
            get => _axisMax;
            set
            {
                _axisMax = value;
                OnPropertyChanged(nameof(AxisMax));
            }
        }
        public double AxisMin
        {
            get => _axisMin;
            set
            {
                _axisMin = value;
                OnPropertyChanged(nameof(AxisMin));
            }
        }

        // Add property for max Y value
        private int _maxYValue = 3;  // Default value
        public int MaxYValue
        {
            get => _maxYValue;
            set
            {
                _maxYValue = value;
                OnPropertyChanged(nameof(MaxYValue));
            }
        }

        private DispatcherTimer _timer;

        private int _currentUserCount;
        public int CurrentUserCount
        {
            get => _currentUserCount;
            set
            {
                if (_currentUserCount != value)
                {
                    _currentUserCount = value;
                    OnPropertyChanged(nameof(CurrentUserCount));
                }
            }
        }


        public MainPageViewModel()
        {
            _serverPort = string.Empty;
            _serverIP = string.Empty;
            _userName = string.Empty;

            InitializeGraph();
            SetupTimer();
        }

        private string _serverPort;
        private string _serverIP;
        private string _userName;
        private string _profilepicurl;

        public string? UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public string? ProfilePicURL
        {
            get { return _profilepicurl; }
            set
            {
                _profilepicurl = value;
                OnPropertyChanged(nameof(ProfilePicURL));
            }
        }

        public string? UserEmail { get; private set; }

        public string? ServerIP
        {
            get { return _serverIP; }
            set
            {
                _serverIP = value;
                OnPropertyChanged(nameof(ServerIP));
            }
        }

        public string ServerPort
        {
            get { return _serverPort; }
            set
            {
                if (_serverPort != value)
                {
                    _serverPort = value;
                    OnPropertyChanged(nameof(ServerPort));
                }
            }
        }

        public bool IsHost { get; private set; } = false;

        // Method to create a session as host
        public string CreateSession(string username, string useremail, string profilePictureUrl)
        {
            IsHost = true;
            UserName = username;
            ProfilePicURL = profilePictureUrl ?? string.Empty;
            _communicator = CommunicationFactory.GetCommunicator(isClientSide: false);
            _serverSessionManager = new Server_Dashboard(_communicator, username, useremail, profilePictureUrl);
            _serverSessionManager.PropertyChanged += UpdateUserListOnPropertyChanged; // Subscribe to PropertyChanged
            string serverCredentials = _serverSessionManager.Initialize();

            if (serverCredentials != "failure")
            {
                string[] parts = serverCredentials.Split(':');
                ServerIP = parts[0];
                ServerPort = parts[1];
                return "success";
            }
            return "failure";
        }

        // Method to join a session as client
        public string JoinSession(string username, string useremail, string serverip, string serverport, string profilePictureUrl)
        {
            IsHost = false;
            UserName = username;
            ProfilePicURL = profilePictureUrl ?? string.Empty;
            _communicator = CommunicationFactory.GetCommunicator();
            _clientSessionManager = new Client_Dashboard(_communicator, username, useremail,profilePictureUrl);
            _clientSessionManager.PropertyChanged += UpdateUserListOnPropertyChanged; // Subscribe to PropertyChanged
            string serverResponse = _clientSessionManager.Initialize(serverip, serverport);

            if (serverResponse == "success")
            {
                UserName = username;
                UserEmail = useremail;
                UpdateUserListOnPropertyChanged(this, new PropertyChangedEventArgs(nameof(Client_Dashboard.ClientUserList)));
            }
            return serverResponse;
        }

        public bool ServerStopSession()
        {
            return _serverSessionManager.ServerStop();
        }

        public bool ClientLeaveSession()
        {
            return _clientSessionManager.ClientLeft();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void UpdateUserListOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Server_Dashboard.ServerUserList) || e.PropertyName == nameof(Client_Dashboard.ClientUserList))
            {
                var users = _serverSessionManager?.ServerUserList ?? _clientSessionManager?.ClientUserList;
                

                if (users != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UserDetailsList.Clear();
                        foreach (var user in users)
                        {
                            UserDetailsList.Add(user);
                        }
                    });
                }

                
            }
        }
        private void InitializeGraph()
        {
            _userCountData = new ObservableCollection<GraphDataModel>();
            _timeLabels = new List<string>();

            SeriesCollection = new SeriesCollection
    {
        new LineSeries
        {
            Title = "Users",
            Values = new ChartValues<ObservableValue>(), // Ensure ChartValues<ObservableValue>
            PointGeometry = null,
            LineSmoothness = 0
        }
    };

            AxisMax = DateTime.Now.Ticks + TimeSpan.FromSeconds(1).Ticks;
            AxisMin = DateTime.Now.Ticks;
        }


        //added
        private void UpdateUserCountGraph()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var now = DateTime.Now;
                var currentCount = UserDetailsList.Count;

                CurrentUserCount = currentCount;

                // Ensure we are adding ObservableValue to the ChartValues
                SeriesCollection[0].Values.Add(new ObservableValue(currentCount));
                TimeLabels.Add(now.ToString("hh:mm"));

                if (currentCount + 2 > MaxYValue)
                {
                    MaxYValue = currentCount + 2;
                }

                // Remove the limit on points if you want to keep all the data points
                // if (SeriesCollection[0].Values.Count > 20)
                // {
                //     SeriesCollection[0].Values.RemoveAt(0);
                //     TimeLabels.RemoveAt(0);
                // }

                OnPropertyChanged(nameof(SeriesCollection));
                OnPropertyChanged(nameof(TimeLabels));
            });
        }
        //added
        private void SetupTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }
        //added
        private void TimerOnTick(object sender, EventArgs e)
        {
            UpdateUserCountGraph();
        }
        //added
        // Add cleanup method
        public void Cleanup()
        {
            _timer?.Stop();
        }
    }
}
