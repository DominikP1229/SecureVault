using SecureVault.Model;
using SecureVault.Model.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SecureVault.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Credential> Credentials { get; set; }
            = new ObservableCollection<Credential>();

        private Credential? _selectedCredential;
        public Credential? SelectedCredential
        {
            get => _selectedCredential;
            set
            {
                _selectedCredential = value;
                OnPropertyChanged();

                DeleteCommand.RaiseCanExecuteChanged();
                EditCommand.RaiseCanExecuteChanged();

                if (value != null)
                {
                    Title = value.Title;
                    Username = value.Username;
                    Category = value.Category;
                    Password = value.EncryptedPassword;
                    Notes = value.Description;
                }
            }
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _category = string.Empty;
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // COMMANDS
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand EditCommand { get; }

        public MainViewModel()
        {
            AddCommand = new RelayCommand(Add);
            DeleteCommand = new RelayCommand(Delete, () => SelectedCredential != null);
            EditCommand = new RelayCommand(Edit, () => SelectedCredential != null);

            SeedData();
        }
        private void SeedData()
        {
            Credentials.Add(new Credential
            {
                Title = "Gmail",
                Username = "jan.kowalski@gmail.com",
                Category = "Email"
            });

            Credentials.Add(new Credential
            {
                Title = "Facebook",
                Username = "janek123",
                Category = "Social Media"
            });

            Credentials.Add(new Credential
            {
                Title = "Bank",
                Username = "jan_k",
                Category = "Finance"
            });
        }
        private void Add()
        {
            ErrorMessage = "";

            // WALIDACJA
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title jest wymagany!";
                return;
            }

            if (Credentials.Any(c => c.Title == Title))
            {
                ErrorMessage = "Title musi być unikalny!";
                return;
            }

            Credentials.Add(new Credential
            {
                Title = Title,
                Username = Username,
                Category = Category,
                EncryptedPassword = Password,
                Description = Notes
            });

            ClearForm();
        }

        private void Delete()
        {
            if (SelectedCredential == null)
            {
                ErrorMessage = "Wybierz element!";
                return;
            }

            Credentials.Remove(SelectedCredential);
            ClearForm();
        }

        private void Edit()
        {
            if (SelectedCredential == null)
                return;

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title jest wymagany!";
                return;
            }

            SelectedCredential.Title = Title;
            SelectedCredential.Username = Username;
            SelectedCredential.Category = Category;
            SelectedCredential.EncryptedPassword = Password;
            SelectedCredential.Description = Notes;

            OnPropertyChanged(nameof(Credentials));
        }

        private void ClearForm()
        {
            Title = "";
            Username = "";
            Category = "";
            Password = "";
            Notes = "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
