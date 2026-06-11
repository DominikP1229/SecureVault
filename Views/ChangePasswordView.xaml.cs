using SecureVault.Model.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class ChangePasswordView : UserControl
    {
        public ChangePasswordView()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var account = VaultSession.CurrentAccount;
            if (account == null)
            {
                ErrorMessageText.Text = "Brak aktywnej sesji użytkownika.";
                return;
            }

            if (!PasswordService.VerifyPassword(CurrentPasswordBox.Password, account.Password))
            {
                ErrorMessageText.Text = "Aktualne hasło jest niepoprawne.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password) || NewPasswordBox.Password.Length < 8)
            {
                ErrorMessageText.Text = "Nowe hasło musi mieć co najmniej 8 znaków.";
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ErrorMessageText.Text = "Nowe hasła nie są takie same.";
                return;
            }

            if (NewPasswordBox.Password == CurrentPasswordBox.Password)
            {
                ErrorMessageText.Text = "Nowe hasło musi różnić się od aktualnego.";
                return;
            }

            try
            {
                VaultSession.ChangePassword(NewPasswordBox.Password);
                MessageBox.Show("Hasło konta zostało zmienione.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigateBack();
            }
            catch (System.Exception ex)
            {
                ErrorMessageText.Text = $"Nie udało się zmienić hasła: {ex.Message}";
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void ChangePasswordView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Save_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                NavigateBack();
                e.Handled = true;
            }
        }

        private void NavigateBack()
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new SettingsView();
            }
        }
    }
}
