using SecureVault.Model;
using SecureVault.Model.Services;
using System;
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

        public ObservableCollection<Credential> Credentials { get; set; } = CredentialStore.Load();
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
                FilteredCredentials?.Refresh();
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
                FilteredCredentials?.Refresh();
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
                    CredentialPasswordReminderEnabled = value.PasswordReminderEnabled;
                    CredentialPasswordReminderMonths = value.PasswordReminderMonths.ToString();
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

        private bool _credentialPasswordReminderEnabled;
        public bool CredentialPasswordReminderEnabled
        {
            get => _credentialPasswordReminderEnabled;
            set { _credentialPasswordReminderEnabled = value; OnPropertyChanged(); }
        }

        private string _credentialPasswordReminderMonths = "6";
        public string CredentialPasswordReminderMonths
        {
            get => _credentialPasswordReminderMonths;
            set { _credentialPasswordReminderMonths = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

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

                FilteredCredentials?.Refresh();
            };

            AddCommand = new RelayCommand(Add);
            DeleteCommand = new RelayCommand(Delete, () => SelectedCredential != null);
            EditCommand = new RelayCommand(Edit, () => SelectedCredential != null);
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);

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

            var categoryMatches = SelectedFilterCategory == null
                || SelectedFilterCategory.CategoryType == "All"
                || credential.Category == SelectedFilterCategory.CategoryType;

            return categoryMatches && MatchesSearch(credential);
        }

        private bool MatchesSearch(Credential credential)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            var searchText = SearchText.Trim();
            return credential.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || credential.Username.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || credential.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || credential.Account.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }

        private void Add()
        {
            ErrorMessage = string.Empty;

            if (!ValidateCredential())
            {
                return;
            }

            if (Credentials.Any(c =>
                    c.Category.Equals(Category.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    c.Title.Equals(Title.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Title musi być unikalny w wybranej kategorii.";
                return;
            }

            var credential = new Credential
            {
                Title = Title.Trim(),
                Username = Username.Trim(),
                Category = Category.Trim(),
                EncryptedPassword = Password,
                Account = Website.Trim(),
                Description = Notes.Trim(),
                PasswordReminderEnabled = CredentialPasswordReminderEnabled,
                PasswordReminderMonths = int.Parse(CredentialPasswordReminderMonths),
                LastPasswordChangedAt = DateTime.Now
            };

            CredentialStore.Add(credential);
            Credentials.Add(credential);

            FilteredCredentials.Refresh();
            ClearForm();
        }

        private void Delete()
        {
            if (SelectedCredential == null)
            {
                ErrorMessage = "Wybierz element do usunięcia.";
                return;
            }

            CredentialStore.Remove(SelectedCredential);
            Credentials.Remove(SelectedCredential);
            FilteredCredentials.Refresh();
            ClearForm();
        }

        private void Edit()
        {
            ErrorMessage = string.Empty;

            if (SelectedCredential == null)
            {
                ErrorMessage = "Wybierz element do edycji.";
                return;
            }

            if (!ValidateCredential())
            {
                return;
            }

            if (Credentials.Any(c =>
                    c != SelectedCredential &&
                    c.Category.Equals(Category.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    c.Title.Equals(Title.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Title musi być unikalny w wybranej kategorii.";
                return;
            }

            SelectedCredential.Title = Title.Trim();
            SelectedCredential.Username = Username.Trim();
            SelectedCredential.Category = Category.Trim();
            SelectedCredential.EncryptedPassword = Password;
            SelectedCredential.Account = Website.Trim();
            SelectedCredential.Description = Notes.Trim();
            SelectedCredential.PasswordReminderEnabled = CredentialPasswordReminderEnabled;
            SelectedCredential.PasswordReminderMonths = int.Parse(CredentialPasswordReminderMonths);
            SelectedCredential.LastPasswordChangedAt = DateTime.Now;

            CredentialStore.Update(SelectedCredential);
            OnPropertyChanged(nameof(Credentials));
            FilteredCredentials.Refresh();
        }

        private bool ValidateCredential()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title jest wymagany.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Username jest wymagany.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                ErrorMessage = "Wybierz kategorię.";
                return false;
            }

            if (!CategoryStore.Exists(Category.Trim()))
            {
                ErrorMessage = "Wybrana kategoria nie istnieje.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password jest wymagany.";
                return false;
            }

            if (Password.Length < 8)
            {
                ErrorMessage = "Password musi mieć co najmniej 8 znaków.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Website) &&
                !Uri.TryCreate(Website.Trim(), UriKind.Absolute, out _))
            {
                ErrorMessage = "Website musi być poprawnym adresem URL.";
                return false;
            }

            if (!int.TryParse(CredentialPasswordReminderMonths, out var reminderMonths) ||
                reminderMonths < 1 ||
                reminderMonths > 60)
            {
                ErrorMessage = "Okres przypomnienia hasła musi wynosić od 1 do 60 miesięcy.";
                return false;
            }

            return true;
        }

        public void ClearForm()
        {
            Title = string.Empty;
            Username = string.Empty;
            Category = string.Empty;
            Password = string.Empty;
            Website = string.Empty;
            Notes = string.Empty;
            CredentialPasswordReminderEnabled = false;
            CredentialPasswordReminderMonths = "6";
            ErrorMessage = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
