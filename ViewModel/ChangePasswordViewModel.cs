using SecureVault.Model.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
                ErrorMessage = "No active user session.";
                return;
            }

            if (!PasswordService.VerifyPassword(CurrentPassword, account.Password))
            {
                ErrorMessage = "The current password is incorrect.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 8)
            {
                ErrorMessage = "The new password must be at least 8 characters long.";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "The new passwords do not match.";
                return;
            }

            if (NewPassword == CurrentPassword)
            {
                ErrorMessage = "The new password must be different from the current password.";
                return;
            }

            try
            {
                await VaultSession.ChangePasswordAsync(NewPassword);
                await NotificationService.ShowInformationAsync("Settings", "Account password has been changed.");
                BackRequested?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Could not change the password: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
