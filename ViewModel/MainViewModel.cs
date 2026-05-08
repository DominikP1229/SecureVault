using SecureVault.Model;
using SecureVault.Model.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace SecureVault.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly PasswordGenerator _passwordGenerator = new();
        private readonly PasswordStrengthService _passwordStrengthService = new();

        public ObservableCollection<Credential> Credentials { get; set; }
            = new ObservableCollection<Credential>();

        public ObservableCollection<Category> Categories => CategoryStore.Categories;

        public ObservableCollection<Category> FilterCategories { get; } = new()
        {
            new Category { CategoryType = "All" }
        };

        public ICollectionView FilteredCredentials { get; }

        private Category? _selectedFilterCategory;
        public Category? SelectedFilterCategory
        {
            get => _selectedFilterCategory;
            set
            {
                _selectedFilterCategory = value;
                OnPropertyChanged();
                FilteredCredentials.Refresh();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilteredCredentials.Refresh();
            }
        }

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
                    Website = value.Account;
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
            set
            {
                _password = value;
                PasswordStrength = _passwordStrengthService.EvaluateStrength(_password);
                OnPropertyChanged();
            }
        }

        private int _passwordStrength;
        public int PasswordStrength
        {
            get => _passwordStrength;
            set { _passwordStrength = value; OnPropertyChanged(); }
        }

        private string _website = string.Empty;
        public string Website
        {
            get => _website;
            set { _website = value; OnPropertyChanged(); }
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
        public RelayCommand GeneratePasswordCommand { get; }

        public MainViewModel()
        {
            foreach (var category in Categories)
            {
                FilterCategories.Add(category);
            }

            Categories.CollectionChanged += (_, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Category category in e.NewItems)
                    {
                        FilterCategories.Add(category);
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (Category category in e.OldItems)
                    {
                        FilterCategories.Remove(category);

                        if (SelectedFilterCategory == category)
                        {
                            SelectedFilterCategory = FilterCategories.FirstOrDefault();
                        }
                    }
                }
            };

            AddCommand = new RelayCommand(Add);
            DeleteCommand = new RelayCommand(Delete, () => SelectedCredential != null);
            EditCommand = new RelayCommand(Edit, () => SelectedCredential != null);
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);

            SeedData();
            FilteredCredentials = CollectionViewSource.GetDefaultView(Credentials);
            FilteredCredentials.Filter = FilterCredential;
            SelectedFilterCategory = FilterCategories.FirstOrDefault();
        }

        private void GeneratePassword()
        {
            Password = _passwordGenerator.Generate(16, true);
        }

        private bool FilterCredential(object item)
        {
            if (item is not Credential credential)
            {
                return false;
            }

            if (SelectedFilterCategory == null || SelectedFilterCategory.CategoryType == "All")
            {
                return MatchesSearch(credential);
            }

            return credential.Category == SelectedFilterCategory.CategoryType && MatchesSearch(credential);
        }

        private bool MatchesSearch(Credential credential)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            var searchText = SearchText.Trim();
            return credential.Title.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
                || credential.Username.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
                || credential.Category.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)
                || credential.Account.Contains(searchText, System.StringComparison.OrdinalIgnoreCase);
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
                Account = Website,
                Description = Notes
            });

            FilteredCredentials.Refresh();
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
            ErrorMessage = "";

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
            SelectedCredential.Account = Website;
            SelectedCredential.Description = Notes;

            OnPropertyChanged(nameof(Credentials));
            FilteredCredentials.Refresh();
        }

        public void ClearForm()
        {
            Title = "";
            Username = "";
            Category = "";
            Password = "";
            Website = "";
            Notes = "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
