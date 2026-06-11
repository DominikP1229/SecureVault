using SecureVault.Model;
using SecureVault.Model.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class SettingsView : UserControl
    {
        private AccountSettings? _settings;

        public SettingsView()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var account = VaultSession.CurrentAccount;
            if (account == null)
            {
                ReminderStatusText.Text = "Brak aktywnej sesji użytkownika.";
                return;
            }

            _settings = AccountSettingsStore.GetOrCreate(account.Id);
            ReminderEnabledCheckBox.IsChecked = _settings.PasswordReminderEnabled;
            ReminderMonthsBox.Text = _settings.PasswordReminderMonths.ToString();
            ReminderStatusText.Text = $"Ostatnia zmiana hasła: {_settings.LastPasswordChangedAt:yyyy-MM-dd}.";
        }

        private void SaveReminderSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_settings == null)
            {
                ReminderStatusText.Text = "Nie można zapisać ustawień bez aktywnego konta.";
                return;
            }

            if (!int.TryParse(ReminderMonthsBox.Text, out var months) || months < 1 || months > 60)
            {
                ReminderStatusText.Text = "Podaj okres od 1 do 60 miesięcy.";
                return;
            }

            _settings.PasswordReminderEnabled = ReminderEnabledCheckBox.IsChecked == true;
            _settings.PasswordReminderMonths = months;
            AccountSettingsStore.Save(_settings);
            ReminderStatusText.Text = "Ustawienia przypomnienia zostały zapisane.";
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new ChangePasswordView();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseSubView();
        }

        private void SettingsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseSubView();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                SaveReminderSettings_Click(sender, e);
                e.Handled = true;
            }
        }

        private void CloseSubView()
        {
            var parent = this.Parent as FrameworkElement;

            while (parent != null && parent.Name != "SubViewContainer")
            {
                parent = parent.Parent as FrameworkElement;
            }

            if (parent is Border border)
            {
                border.Visibility = Visibility.Collapsed;

                if (border.Child is ContentControl content)
                {
                    content.Content = null;
                }
            }
        }
    }
}
