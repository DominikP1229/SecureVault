using SecureVault.Model;
using SecureVault.Model.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SecureVault.ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private AccountSettings? _settings;
        private bool _reminderEnabled;
        private string _reminderMonths = "6";
        private string _reminderStatus = string.Empty;

        public AsyncRelayCommand SaveReminderSettingsCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand CloseCommand { get; }

        public event Action? ChangePasswordRequested;
        public event Action? CloseRequested;

        public SettingsViewModel()
        {
            SaveReminderSettingsCommand = new AsyncRelayCommand(SaveReminderSettingsAsync);
            ChangePasswordCommand = new RelayCommand(() => ChangePasswordRequested?.Invoke());
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
            _ = LoadSettingsAsync();
        }

        public bool ReminderEnabled
        {
            get => _reminderEnabled;
            set
            {
                _reminderEnabled = value;
                OnPropertyChanged();
            }
        }

        public string ReminderMonths
        {
            get => _reminderMonths;
            set
            {
                _reminderMonths = value;
                OnPropertyChanged();
            }
        }

        public string ReminderStatus
        {
            get => _reminderStatus;
            set
            {
                _reminderStatus = value;
                OnPropertyChanged();
            }
        }

        private async Task LoadSettingsAsync()
        {
            var account = VaultSession.CurrentAccount;
            if (account == null)
            {
                ReminderStatus = "Brak aktywnej sesji uzytkownika.";
                return;
            }

            _settings = await AccountSettingsStore.GetOrCreateAsync(account.Id);
            ReminderEnabled = _settings.PasswordReminderEnabled;
            ReminderMonths = _settings.PasswordReminderMonths.ToString();
            ReminderStatus = $"Ostatnia zmiana hasla: {_settings.LastPasswordChangedAt:yyyy-MM-dd}.";
        }

        private async Task SaveReminderSettingsAsync()
        {
            if (_settings == null)
            {
                ReminderStatus = "Nie mozna zapisac ustawien bez aktywnego konta.";
                return;
            }

            if (!int.TryParse(ReminderMonths, out var months) || months < 1 || months > 60)
            {
                ReminderStatus = "Podaj okres od 1 do 60 miesiecy.";
                return;
            }

            _settings.PasswordReminderEnabled = ReminderEnabled;
            _settings.PasswordReminderMonths = months;
            await AccountSettingsStore.SaveAsync(_settings);
            ReminderStatus = "Ustawienia przypomnienia zostaly zapisane.";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
