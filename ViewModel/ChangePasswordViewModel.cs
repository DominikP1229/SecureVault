using SecureVault.Model.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace SecureVault.ViewModel
{
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private string _errorMessage = string.Empty;

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand BackCommand { get; }
        public event Action? BackRequested;

        public ChangePasswordViewModel()
        {
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            BackCommand = new RelayCommand(() => BackRequested?.Invoke());
        }

        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private async Task SaveAsync()
        {
            var account = VaultSession.CurrentAccount;
            if (account == null)
            {
                ErrorMessage = "Brak aktywnej sesji uzytkownika.";
                return;
            }

            if (!PasswordService.VerifyPassword(CurrentPassword, account.Password))
            {
                ErrorMessage = "Aktualne haslo jest niepoprawne.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
            {
                ErrorMessage = "Nowe haslo musi miec co najmniej 8 znakow.";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Nowe hasla nie sa takie same.";
                return;
            }

            if (NewPassword == CurrentPassword)
            {
                ErrorMessage = "Nowe haslo musi roznic sie od aktualnego.";
                return;
            }

            try
            {
                await VaultSession.ChangePasswordAsync(NewPassword);
                MessageBox.Show("Haslo konta zostalo zmienione.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                BackRequested?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Nie udalo sie zmienic hasla: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
