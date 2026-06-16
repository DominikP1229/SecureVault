using SecureVault.Model;
using SecureVault.Model.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace SecureVault.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _password = string.Empty;

        public ObservableCollection<Account> Accounts => AccountStore.Accounts;
        public Account? SelectedAccount { get; set; }
        public AsyncRelayCommand<string> LoginCommand { get; }
        public RelayCommand OpenRegisterCommand { get; }

        public event Action<LoginNavigationTarget>? NavigationRequested;

        public LoginViewModel()
        {
            SelectedAccount = Accounts.Count > 0 ? Accounts[0] : null;
            LoginCommand = new AsyncRelayCommand<string>(_ => LoginAsync(Password));
            OpenRegisterCommand = new RelayCommand(() => NavigationRequested?.Invoke(LoginNavigationTarget.Register));
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private async Task LoginAsync(string? password)
        {
            if (SelectedAccount == null)
            {
                MessageBox.Show("Select an account.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Enter a password.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PasswordService.VerifyPassword(password, SelectedAccount.Password))
            {
                MessageBox.Show("Incorrect password.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PasswordService.IsHashed(SelectedAccount.Password))
            {
                await AccountStore.UpdatePasswordAsync(SelectedAccount, password);
            }

            VaultSession.SignIn(SelectedAccount, password);
            await ShowAccountPasswordReminderIfNeededAsync(SelectedAccount);
            NavigationRequested?.Invoke(LoginNavigationTarget.Main);
        }

        private static async Task ShowAccountPasswordReminderIfNeededAsync(Account account)
        {
            var settings = await AccountSettingsStore.GetOrCreateAsync(account.Id);
            if (!AccountSettingsStore.ShouldRemind(settings))
            {
                return;
            }

            MessageBox.Show(
                $"The configured password change reminder interval has passed ({settings.PasswordReminderMonths} months). You can change your password in Settings.",
                "Password change reminder",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public enum LoginNavigationTarget
    {
        Main,
        Register
    }
}
