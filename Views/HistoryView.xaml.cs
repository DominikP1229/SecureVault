using SecureVault.Model.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class HistoryView : UserControl
    {
        public ObservableCollection<PasswordHistoryViewItem> HistoryItems { get; } = new();

        public HistoryView()
        {
            InitializeComponent();
            DataContext = this;
            LoadHistory();
        }

        private void LoadHistory()
        {
            var encryption = VaultSession.Encryption;

            foreach (var history in PasswordHistoryStore.LoadForCurrentAccount())
            {
                var maskedPassword = "********";
                if (encryption != null &&
                    encryption.TryDecrypt(history.EncryptedPassword, out var plainPassword) &&
                    !string.IsNullOrEmpty(plainPassword))
                {
                    maskedPassword = new string('*', plainPassword.Length);
                }

                HistoryItems.Add(new PasswordHistoryViewItem
                {
                    ChangedDate = history.ChangedDate,
                    Action = history.Action,
                    CredentialTitle = history.CredentialTitle,
                    MaskedPassword = maskedPassword
                });
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var parent = this.Parent as FrameworkElement;

            while (parent != null && parent.Name != "SubViewContainer")
            {
                parent = parent.Parent as FrameworkElement;
            }

            if (parent != null)
            {
                parent.Visibility = Visibility.Collapsed;

                if (parent is Border border && border.Child is ContentControl content)
                {
                    content.Content = null;
                }
            }
        }
    }

    public class PasswordHistoryViewItem
    {
        public System.DateTime ChangedDate { get; set; }
        public string Action { get; set; } = string.Empty;
        public string CredentialTitle { get; set; } = string.Empty;
        public string MaskedPassword { get; set; } = string.Empty;
    }
}
