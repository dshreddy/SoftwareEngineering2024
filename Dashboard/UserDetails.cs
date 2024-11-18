using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dashboard
{
    [JsonSerializable(typeof(UserDetails))]
    public class UserDetails : INotifyPropertyChanged
    {
        private string? _userName;
        private string? _profilePictureUrl;

        [JsonInclude]
        public string? userName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(userName));
                }
            }
        }

        [JsonInclude]
        public bool IsHost { get; set; }

        [JsonInclude]
        public string? userId { get; set; }

        [JsonInclude]
        public string? userEmail { get; set; }

        [JsonInclude]
        public string? ProfilePictureUrl
        {
            get { return _profilePictureUrl; }
            set
            {
                if (_profilePictureUrl != value)
                {
                    _profilePictureUrl = value;
                    OnPropertyChanged(nameof(ProfilePictureUrl));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
