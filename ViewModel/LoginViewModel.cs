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
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }
        public AsyncRelayCommand<string> LoginCommand { get; }
        public RelayCommand OpenRegisterCommand { get; }

        public event Action<LoginNavigationTarget>? NavigationRequested;

        public LoginViewModel()
        {
            SelectedAccount = Accounts.Count > 0 ? Accounts[0] : null;
            LoginCommand = new AsyncRelayCommand<string>(_ => LoginAsync(Password));
            OpenRegisterCommand = new RelayCommand(() => NavigationRequested?.Invoke(LoginNavigationTarget.Register));
        }

        private async Task LoginAsync(string? password)
        {
            if (SelectedAccount == null)
            {
                MessageBox.Show("Wybierz konto.", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Podaj hasło.", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PasswordService.VerifyPassword(password, SelectedAccount.Password))
            {
                MessageBox.Show("Niepoprawne hasło.", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PasswordService.IsHashed(SelectedAccount.Password))
            {
                await AccountStore.UpdatePasswordAsync(SelectedAccount, password);
            }

            VaultSession.SignIn(SelectedAccount, password);
            await ShowPasswordReminderIfNeededAsync(SelectedAccount);
            await ShowCredentialPasswordRemindersIfNeededAsync();
            NavigationRequested?.Invoke(LoginNavigationTarget.Main);
        }

        private static async Task ShowPasswordReminderIfNeededAsync(Account account)
        {
            var settings = await AccountSettingsStore.GetOrCreateAsync(account.Id);
            if (!AccountSettingsStore.ShouldRemind(settings))
            {
                return;
            }

            MessageBox.Show(
                $"Minął ustawiony okres przypomnienia o zmianie hasła ({settings.PasswordReminderMonths} mies.). Możesz zmienić hasło w Settings.",
                "Przypomnienie o zmianie hasła",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private static async Task ShowCredentialPasswordRemindersIfNeededAsync()
        {
            var expiredTitles = await CredentialStore.GetExpiredPasswordReminderTitlesAsync();
            if (expiredTitles.Length == 0)
            {
                return;
            }

            MessageBox.Show(
                "Warto zmienić hasła dla tych wpisów:\n\n" + string.Join("\n", expiredTitles),
                "Przypomnienie o zapisanych hasłach",
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
