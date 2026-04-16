using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SecureVault.Model
{
    /*public class Credential
    {
        public Credential()
        {
        }

        public string? Title { get; set; }
        public string? Username { get; set; }
        public string? Category { get; set; }
    }*/
    public class Credential : INotifyPropertyChanged
    {
        private string _title;
        public string? Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _username;
        public string? Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _category;
        public string? Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
