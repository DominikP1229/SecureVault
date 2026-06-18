using SecureVault.Model;
using SecureVault.Model.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SecureVault.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly PasswordGenerator _passwordGenerator = new();
        private readonly PasswordStrengthService _passwordStrengthService = new();
        private readonly IFileDialogService _fileDialogService;
        private bool _isEditMode;
        private bool _accountPasswordReminderVisible;
        private string _accountPasswordReminderMessage = string.Empty;

        public ObservableCollection<Credential> Credentials { get; set; } = new();
        public ObservableCollection<Category> Categories => CategoryStore.Categories;

        public ObservableCollection<string> SortFields { get; } = new()
        {
            "Title",
            "Category",
            "Username",
            "Website",
            "Next reminder",
            "Modified date"
        };

        public ObservableCollection<string> SearchFields { get; } = new()
        {
            "All",
            "Title",
            "Category",
            "Username",
            "Website",
            "Next reminder",
            "Modified date"
        };

        public ObservableCollection<string> SortDirections { get; } = new()
        {
            "Ascending",
            "Descending"
        };

        public ICollectionView? FilteredCredentials { get; private set; }

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

        private string _selectedSearchField = "All";
        public string SelectedSearchField
        {
            get => _selectedSearchField;
            set
            {
                _selectedSearchField = value;
                OnPropertyChanged();
                FilteredCredentials?.Refresh();
            }
        }

        private string _primarySortField = "Title";
        public string PrimarySortField
        {
            get => _primarySortField;
            set
            {
                _primarySortField = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        private string _secondarySortField = "Category";
        public string SecondarySortField
        {
            get => _secondarySortField;
            set
            {
                _secondarySortField = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        private string _sortDirection = "Ascending";
        public string SortDirection
        {
            get => _sortDirection;
            set
            {
                _sortDirection = value;
                OnPropertyChanged();
                ApplySorting();
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

        public AsyncRelayCommand AddCommand { get; }
        public AsyncRelayCommand DeleteCommand { get; }
        public AsyncRelayCommand EditCommand { get; }
        public RelayCommand GeneratePasswordCommand { get; }
        public RelayCommand OpenAddCommand { get; }
        public AsyncRelayCommand OpenEditCommand { get; }
        public RelayCommand OpenDetailsCommand { get; }
        public RelayCommand OpenHistoryCommand { get; }
        public RelayCommand OpenCategoriesCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public AsyncRelayCommand SaveCredentialCommand { get; }
        public AsyncRelayCommand ImportCsvCommand { get; }
        public AsyncRelayCommand ExportCsvCommand { get; }
        public RelayCommand CancelCredentialFormCommand { get; }
        public AsyncRelayCommand<Credential> CopyCredentialCommand { get; }
        public AsyncRelayCommand<Credential> DeleteCredentialCommand { get; }
        public AsyncRelayCommand<Credential> EditCredentialCommand { get; }
        public RelayCommand DismissAccountPasswordReminderCommand { get; }
        public RelayCommand OpenChangePasswordFromReminderCommand { get; }

        public event Action<MainNavigationTarget>? NavigationRequested;

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
            }
        }

        public bool AccountPasswordReminderVisible
        {
            get => _accountPasswordReminderVisible;
            set
            {
                _accountPasswordReminderVisible = value;
                OnPropertyChanged();
            }
        }

        public string AccountPasswordReminderMessage
        {
            get => _accountPasswordReminderMessage;
            set
            {
                _accountPasswordReminderMessage = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
            : this(new FileDialogService())
        {
        }

        public MainViewModel(IFileDialogService fileDialogService)
        {
            _fileDialogService = fileDialogService;

            AddCommand = new AsyncRelayCommand(AddAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => SelectedCredential != null);
            EditCommand = new AsyncRelayCommand(EditAsync, () => SelectedCredential != null);
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);
            OpenAddCommand = new RelayCommand(OpenAdd);
            OpenEditCommand = new AsyncRelayCommand(OpenEditAsync);
            OpenDetailsCommand = new RelayCommand(OpenDetails, () => SelectedCredential != null);
            OpenHistoryCommand = new RelayCommand(() => NavigationRequested?.Invoke(MainNavigationTarget.History));
            OpenCategoriesCommand = new RelayCommand(() => NavigationRequested?.Invoke(MainNavigationTarget.Categories));
            OpenSettingsCommand = new RelayCommand(() => NavigationRequested?.Invoke(MainNavigationTarget.Settings));
            LogoutCommand = new RelayCommand(Logout);
            SaveCredentialCommand = new AsyncRelayCommand(SaveCredentialAsync);
            ImportCsvCommand = new AsyncRelayCommand(ImportCsvAsync);
            ExportCsvCommand = new AsyncRelayCommand(ExportCsvAsync);
            CancelCredentialFormCommand = new RelayCommand(() => NavigationRequested?.Invoke(MainNavigationTarget.Main));
            CopyCredentialCommand = new AsyncRelayCommand<Credential>(CopyCredentialAsync);
            DeleteCredentialCommand = new AsyncRelayCommand<Credential>(DeleteCredentialAsync);
            EditCredentialCommand = new AsyncRelayCommand<Credential>(EditCredentialAsync);
            DismissAccountPasswordReminderCommand = new RelayCommand(DismissAccountPasswordReminder);
            OpenChangePasswordFromReminderCommand = new RelayCommand(OpenChangePasswordFromReminder);

            FilteredCredentials = CollectionViewSource.GetDefaultView(Credentials);
            FilteredCredentials.Filter = FilterCredential;
            ApplySorting();
        }

        public async Task LoadCredentialsAsync()
        {
            Credentials = await CredentialStore.LoadAsync();
            FilteredCredentials = CollectionViewSource.GetDefaultView(Credentials);
            FilteredCredentials.Filter = FilterCredential;
            ApplySorting();
            OnPropertyChanged(nameof(Credentials));
            OnPropertyChanged(nameof(FilteredCredentials));
            await LoadAccountPasswordReminderAsync();
        }

        private void GeneratePassword()
        {
            Password = _passwordGenerator.Generate(16, true);
        }

        private void OpenAdd()
        {
            SelectedCredential = null;
            ClearForm();
            IsEditMode = false;
            NavigationRequested?.Invoke(MainNavigationTarget.CredentialForm);
        }

        private async Task OpenEditAsync()
        {
            if (SelectedCredential == null)
            {
                await NotificationService.ShowInformationAsync("Edit", "Select an entry to edit.");
                return;
            }

            IsEditMode = true;
            NavigationRequested?.Invoke(MainNavigationTarget.CredentialForm);
        }

        private void OpenDetails()
        {
            if (SelectedCredential != null)
            {
                NavigationRequested?.Invoke(MainNavigationTarget.CredentialDetails);
            }
        }

        private void Logout()
        {
            VaultSession.SignOut();
            NavigationRequested?.Invoke(MainNavigationTarget.Login);
        }

        private async Task LoadAccountPasswordReminderAsync()
        {
            var account = VaultSession.CurrentAccount;
            if (account == null)
            {
                AccountPasswordReminderVisible = false;
                return;
            }

            var settings = await AccountSettingsStore.GetOrCreateAsync(account.Id);
            if (!AccountSettingsStore.ShouldRemind(settings))
            {
                AccountPasswordReminderVisible = false;
                return;
            }

            AccountPasswordReminderMessage =
                $"The configured account password change reminder interval has passed ({settings.PasswordReminderMonths} months).";
            AccountPasswordReminderVisible = true;
        }

        private void DismissAccountPasswordReminder()
        {
            AccountPasswordReminderVisible = false;
        }

        private void OpenChangePasswordFromReminder()
        {
            AccountPasswordReminderVisible = false;
            NavigationRequested?.Invoke(MainNavigationTarget.ChangePassword);
        }

        private async Task SaveCredentialAsync()
        {
            var countBeforeSave = Credentials.Count;

            if (IsEditMode)
            {
                await EditAsync();

                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    NavigationRequested?.Invoke(MainNavigationTarget.Main);
                }

                return;
            }

            await AddAsync();

            if (Credentials.Count > countBeforeSave)
            {
                NavigationRequested?.Invoke(MainNavigationTarget.Main);
            }
        }

        private async Task ImportCsvAsync()
        {
            var filePath = _fileDialogService.PickCsvToOpen();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                var importedCredentials = await CredentialCsvService.ImportAsync(filePath);
                var result = new CredentialCsvImportResult();

                foreach (var credential in importedCredentials)
                {
                    if (Credentials.Any(existing =>
                            existing.Category.Equals(credential.Category, StringComparison.OrdinalIgnoreCase) &&
                            existing.Title.Equals(credential.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    if (!CategoryStore.Exists(credential.Category))
                    {
                        await CategoryStore.AddAsync(credential.Category);
                    }

                    await CredentialStore.AddAsync(credential);
                    Credentials.Add(credential);
                    result.ImportedCount++;
                }

                FilteredCredentials?.Refresh();
                await NotificationService.ShowInformationAsync(
                    "Import CSV",
                    $"Import completed. Added: {result.ImportedCount}, skipped: {result.SkippedCount}.");
            }
            catch (Exception ex)
            {
                await NotificationService.ShowErrorAsync(
                    "Import CSV",
                    $"Could not import CSV: {ex.Message}");
            }
        }

        private async Task ExportCsvAsync()
        {
            if (Credentials.Count == 0)
            {
                await NotificationService.ShowInformationAsync("Export CSV", "There are no passwords to export.");
                return;
            }

            var confirmation = await NotificationService.ConfirmWarningAsync(
                "Export CSV",
                "The CSV file will contain plain-text passwords. Save it only in a secure location. Continue?");

            if (confirmation != NotificationResult.Yes)
            {
                return;
            }

            var filePath = _fileDialogService.PickCsvToSave();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                await CredentialCsvService.ExportAsync(Credentials, filePath);
                await NotificationService.ShowInformationAsync("Export CSV", "CSV export completed.");
            }
            catch (Exception ex)
            {
                await NotificationService.ShowErrorAsync(
                    "Export CSV",
                    $"Could not export CSV: {ex.Message}");
            }
        }

        private async Task CopyCredentialAsync(Credential? credential)
        {
            var credentialToCopy = credential ?? SelectedCredential;
            if (credentialToCopy == null)
            {
                await NotificationService.ShowInformationAsync("Copy", "Select an entry to copy the password from.");
                return;
            }

            if (string.IsNullOrEmpty(credentialToCopy.EncryptedPassword))
            {
                await NotificationService.ShowInformationAsync("Copy", "The selected entry does not have a saved password.");
                return;
            }

            Clipboard.SetText(credentialToCopy.EncryptedPassword);
            await NotificationService.ShowInformationAsync("Copy", "Password copied to clipboard.");
        }

        private async Task DeleteCredentialAsync(Credential? credential)
        {
            if (credential != null)
            {
                SelectedCredential = credential;
            }

            await DeleteAsync();
        }

        private async Task EditCredentialAsync(Credential? credential)
        {
            if (credential != null)
            {
                SelectedCredential = credential;
            }

            await OpenEditAsync();
        }

        private bool FilterCredential(object item)
        {
            if (item is not Credential credential)
            {
                return false;
            }

            return MatchesSearch(credential);
        }

        private bool MatchesSearch(Credential credential)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                return true;
            }

            var searchText = SearchText.Trim();
            return SelectedSearchField == "All"
                ? MatchesAnySearchField(credential, searchText)
                : MatchesSearchField(credential, SelectedSearchField, searchText);
        }

        private static bool MatchesAnySearchField(Credential credential, string searchText)
        {
            return MatchesSearchField(credential, "Title", searchText)
                || MatchesSearchField(credential, "Category", searchText)
                || MatchesSearchField(credential, "Username", searchText)
                || MatchesSearchField(credential, "Website", searchText)
                || MatchesSearchField(credential, "Next reminder", searchText)
                || MatchesSearchField(credential, "Modified date", searchText);
        }

        private static bool MatchesSearchField(Credential credential, string field, string searchText)
        {
            return field switch
            {
                "Category" => credential.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase),
                "Username" => credential.Username.Contains(searchText, StringComparison.OrdinalIgnoreCase),
                "Website" => credential.Account.Contains(searchText, StringComparison.OrdinalIgnoreCase),
                "Next reminder" => FormatDate(credential.NextPasswordReminderDate).Contains(searchText, StringComparison.OrdinalIgnoreCase),
                "Modified date" => FormatDate(credential.ModifiedDate).Contains(searchText, StringComparison.OrdinalIgnoreCase),
                _ => credential.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            };
        }

        private static string FormatDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("yyyy-MM-dd") : string.Empty;
        }

        private void ApplySorting()
        {
            if (FilteredCredentials == null)
            {
                return;
            }

            FilteredCredentials.SortDescriptions.Clear();

            var direction = SortDirection == "Descending"
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;

            AddSortDescription(PrimarySortField, direction);

            if (!string.Equals(PrimarySortField, SecondarySortField, StringComparison.Ordinal))
            {
                AddSortDescription(SecondarySortField, direction);
            }

            FilteredCredentials.Refresh();
        }

        private void AddSortDescription(string field, ListSortDirection direction)
        {
            var propertyName = field switch
            {
                "Category" => nameof(Credential.Category),
                "Username" => nameof(Credential.Username),
                "Website" => nameof(Credential.Account),
                "Next reminder" => nameof(Credential.NextPasswordReminderDate),
                "Modified date" => nameof(Credential.ModifiedDate),
                _ => nameof(Credential.Title)
            };

            FilteredCredentials?.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }

        private async Task AddAsync()
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
                ErrorMessage = "Title must be unique in the selected category.";
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
                LastPasswordChangedAt = DateTime.Now,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            await CredentialStore.AddAsync(credential);
            Credentials.Add(credential);

            FilteredCredentials?.Refresh();
            ClearForm();
        }

        private async Task DeleteAsync()
        {
            if (SelectedCredential == null)
            {
                ErrorMessage = "Select an item to delete.";
                return;
            }

            await CredentialStore.RemoveAsync(SelectedCredential);
            Credentials.Remove(SelectedCredential);
            FilteredCredentials?.Refresh();
            ClearForm();
        }

        private async Task EditAsync()
        {
            ErrorMessage = string.Empty;

            if (SelectedCredential == null)
            {
                ErrorMessage = "Select an item to edit.";
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
                ErrorMessage = "Title must be unique in the selected category.";
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
            SelectedCredential.ModifiedDate = DateTime.Now;

            await CredentialStore.UpdateAsync(SelectedCredential);
            OnPropertyChanged(nameof(Credentials));
            FilteredCredentials?.Refresh();
        }

        private bool ValidateCredential()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Username is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                ErrorMessage = "Select a category.";
                return false;
            }

            if (!CategoryStore.Exists(Category.Trim()))
            {
                ErrorMessage = "The selected category does not exist.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password is required.";
                return false;
            }

            if (Password.Length < 8)
            {
                ErrorMessage = "Password must be at least 8 characters long.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Website) && !IsValidWebsite(Website))
            {
                ErrorMessage = "Website must be a valid URL or domain.";
                return false;
            }

            if (!int.TryParse(CredentialPasswordReminderMonths, out var reminderMonths) ||
                reminderMonths < 1 ||
                reminderMonths > 60)
            {
                ErrorMessage = "Password reminder interval must be between 1 and 60 months.";
                return false;
            }

            return true;
        }

        private static bool IsValidWebsite(string website)
        {
            var value = website.Trim();
            if (value.Contains(' '))
            {
                return false;
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
            {
                return HasValidHost(absoluteUri);
            }

            return Uri.TryCreate($"https://{value}", UriKind.Absolute, out var normalizedUri) &&
                HasValidHost(normalizedUri);
        }

        private static bool HasValidHost(Uri uri)
        {
            return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                !string.IsNullOrWhiteSpace(uri.Host) &&
                uri.Host.Contains('.') &&
                !uri.Host.StartsWith('.') &&
                !uri.Host.EndsWith('.');
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

    public enum MainNavigationTarget
    {
        Main,
        Login,
        CredentialForm,
        CredentialDetails,
        History,
        Categories,
        Settings,
        ChangePassword
    }
}
