using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SecureVault.Model;
using SecureVault.Model.Services;

namespace SecureVault.Views
{
    /// <summary>
    /// Logika interakcji dla klasy LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {

        public LoginView()
        {
            InitializeComponent();
            LoginBox.ItemsSource = AccountStore.Accounts;
            LoginBox.DisplayMemberPath = nameof(Account.Name);

            if (AccountStore.Accounts.Count > 0)
            {
                LoginBox.SelectedIndex = 0;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (LoginBox.SelectedItem is not Account selectedAccount)
            {
                MessageBox.Show("Wybierz konto.", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Podaj hasło.", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PasswordService.VerifyPassword(PasswordBox.Password, selectedAccount.Password))
            {
                MessageBox.Show("Niepoprawne hasło.", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PasswordService.IsHashed(selectedAccount.Password))
            {
                AccountStore.UpdatePassword(selectedAccount, PasswordBox.Password);
            }

            VaultSession.SignIn(selectedAccount, PasswordBox.Password);
            ShowPasswordReminderIfNeeded(selectedAccount);
            ShowCredentialPasswordRemindersIfNeeded();

            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new MainView());
            }
        }
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new RegisterView());
            }
        }

        private void LoginView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(sender, e);
                e.Handled = true;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(sender, e);
                e.Handled = true;
            }
        }

        private static void ShowPasswordReminderIfNeeded(Account account)
        {
            var settings = AccountSettingsStore.GetOrCreate(account.Id);
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

        private static void ShowCredentialPasswordRemindersIfNeeded()
        {
            var expiredTitles = CredentialStore.GetExpiredPasswordReminderTitles();
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
    }
}
