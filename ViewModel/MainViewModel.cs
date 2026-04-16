using SecureVault.Model;
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

        private Credential _selectedCredential;
        public Credential SelectedCredential
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
                }
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _category;
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
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
                Category = Category
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

            OnPropertyChanged(nameof(Credentials));
        }

        private void ClearForm()
        {
            Title = "";
            Username = "";
            Category = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}