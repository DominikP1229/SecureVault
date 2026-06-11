using SecureVault.Model.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace SecureVault.ViewModel
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private string _password = string.Empty;

        public string Login { get; set; } = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }
        public AsyncRelayCommand<string> RegisterCommand { get; }
        public RelayCommand CancelCommand { get; }

        public event Action? BackToLoginRequested;

        public RegisterViewModel()
        {
            RegisterCommand = new AsyncRelayCommand<string>(_ => RegisterAsync(Password));
            CancelCommand = new RelayCommand(() => BackToLoginRequested?.Invoke());
        }

        private async Task RegisterAsync(string? password)
        {
            var login = Login.Trim();

            if (string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show("Login jest wymagany.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (login.Length < 3)
            {
                MessageBox.Show("Login musi mieć co najmniej 3 znaki.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Hasło jest wymagane.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Hasło musi mieć co najmniej 8 znaków.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AccountStore.Exists(login))
            {
                MessageBox.Show("Konto o takim loginie już istnieje.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var account = await AccountStore.AddAsync(login, password);
            await AccountSettingsStore.MarkPasswordChangedAsync(account.Id);

            BackToLoginRequested?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
