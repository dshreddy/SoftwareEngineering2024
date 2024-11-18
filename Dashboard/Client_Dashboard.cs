using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Networking;
using Networking.Communication;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using Screenshare;


namespace Dashboard
{
   
    public class Client_Dashboard : INotificationHandler, INotifyPropertyChanged
    {
        private ICommunicator _communicator;
        private string UserName { get; set; }
        private string UserEmail { get; set; }
        private string UserID { get; set; }
        private string UserProfileUrl { get; set; }
        public int CurrentUserCount { get; set; }


        public ObservableCollection<UserDetails> ClientUserList { get; set; } = new ObservableCollection<UserDetails>();

        Screenshare.ScreenShareClient.ScreenshareClient  _screenShareClient = Screenshare.ScreenShareClient.ScreenshareClient.GetInstance();

        public Client_Dashboard(ICommunicator communicator, string username, string useremail, string pictureURL)
        {
            _communicator = communicator;
            _communicator.Subscribe("Dashboard", this, isHighPriority: true);
            UserName = username;
            UserEmail = useremail;
            UserProfileUrl = pictureURL;
            UserID = string.Empty; // Initialize UserID
            ClientUserList.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ClientUserList));
        }

        public string Initialize(string serverIP, string serverPort)
        {
            string server_response = _communicator.Start(serverIP, serverPort);
            Trace.WriteLine("[DashboardClient] client connected to server");
            return server_response;
        }

        public void SendMessage(string clientIP, string message)
        {
            string json_message = JsonSerializer.Serialize(message);
            try
            {
                _communicator.Send(json_message, "Dashboard", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void SendInfo(string username, string useremail)
        {
            DashboardDetails details = new DashboardDetails
            {
                User = new UserDetails
                {
                    userName = username,
                    userEmail = useremail,
                    ProfilePictureUrl = UserProfileUrl,
                    userId = UserID
                },
                Action = Action.ClientUserConnected
            };
            string json_message = JsonSerializer.Serialize(details);
            try
            {
                _communicator.Send(json_message, "Dashboard", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
            Trace.WriteLine("[DashboardClient] sent user info to server");

            _screenShareClient.SetUserDetails(username, UserID);

             //WhiteboardGUI.ViewModel.MainPageViewModel WBviewModel = WhiteboardGUI.ViewModel.MainPageViewModel.WhiteboardInstance;
             //WBviewModel.SetUserDetails(UserName, UserID);



            Trace.WriteLine("[DashboardServer] sent info to whiteboard client");

        }

        public bool ClientLeft()
        {
            DashboardDetails details = new DashboardDetails
            {
                User = new UserDetails { userName = UserName , userId = UserID },
                Action = Action.ClientUserLeft
            };
            string json_message = JsonSerializer.Serialize(details);

            try
            {

                _communicator.Send(json_message, "Dashboard", null);
                Trace.WriteLine("[Dashboardclient] left session gracefully");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex.Message}");
            }
            //_communicator.Stop();
            return true;
            
        }

        public void OnDataReceived(string message)
        {
            try
            {
                var details = JsonSerializer.Deserialize<DashboardDetails>(message);
                if (details == null)
                {
                    Console.WriteLine("Error: Deserialized message is null");
                    return;
                }
                Trace.WriteLine("[DashClient]"+details.Action);
                switch (details.Action)
                {
                    case Action.ServerSendUserID:
                        HandleRecievedUserInfo(details);
                        break;
                    case Action.ServerUserAdded:
                        HandleUserConnected(details);
                        break;
                    case Action.ServerUserLeft:
                        HandleUserLeft(details);
                        break;
                    case Action.ServerEnd:
                        HandleEndOfMeeting();
                        break;
                    default:
                        Console.WriteLine($"Unknown action: {details.Action}");
                        break;
                }
            }
            catch (JsonException)
            {
                Trace.WriteLine("[DashClient] recieved list from server");
                var userList = JsonSerializer.Deserialize<List<UserDetails>>(message);
                if (userList != null)
                {
                    ClientUserList = new ObservableCollection<UserDetails>(userList);
                    CurrentUserCount = userList.Count;
                }
                else
                {
                    Console.WriteLine("Error: Deserialized user list is null");
                }
                OnPropertyChanged(nameof(ClientUserList));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing message: {ex.Message}");
            }
        }

        private void HandleRecievedUserInfo(DashboardDetails message)
        {
            if (message.User != null && message.User.userId != null)
            {
                UserID = message.User.userId;
                SendInfo(UserName, UserEmail);
            }
            else
            {
                Console.WriteLine("Error: Received user info is null");
            }
        }

        private void HandleUserConnected(DashboardDetails message)
        {
            if (message.User != null && message.User.userId != null)
            {
                UserDetails userData = message.User;
                string newuserid = userData.userId;

                Trace.WriteLine($"[Dash client] User Connected: {userData.userName}");

                if (ClientUserList.Count >= int.Parse(newuserid))
                {
                    ClientUserList[int.Parse(newuserid) - 1] = userData;
                }
                else
                {
                    ClientUserList.Add(userData);
                }
                CurrentUserCount++;
                OnPropertyChanged(nameof(ClientUserList));
            }
            else
            {
                Console.WriteLine("Error: Received user info is null");
            }
        }

        private void HandleUserLeft(DashboardDetails message)
        {
            if (message.User != null && message.User.userId != null)
            {
                Trace.WriteLine("[Dashboard client] some random client left");
                CurrentUserCount--;
                string leftuserid = message.User.userId;

                foreach (var user in ClientUserList)
                {
                    if (user.userId == leftuserid)
                    {
                        ClientUserList.Remove(user);
                    }
                }
                OnPropertyChanged(nameof(ClientUserList));
            }
            else
            {
                Console.WriteLine("Error: Received user info is null");
            }
        }

        private void HandleEndOfMeeting()
        {

            ClientUserList.Clear();
            _communicator.Stop();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
