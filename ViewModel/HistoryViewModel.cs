using SecureVault.Model.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SecureVault.ViewModel
{
    public class HistoryViewModel
    {
        public ObservableCollection<PasswordHistoryViewItem> HistoryItems { get; } = new();
        public RelayCommand CloseCommand { get; }

        public event Action? CloseRequested;

        public HistoryViewModel()
        {
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
            _ = LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            var encryption = VaultSession.Encryption;

            foreach (var history in await PasswordHistoryStore.LoadForCurrentAccountAsync())
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
    }

    public class PasswordHistoryViewItem
    {
        public DateTime ChangedDate { get; set; }
        public string Action { get; set; } = string.Empty;
        public string CredentialTitle { get; set; } = string.Empty;
        public string MaskedPassword { get; set; } = string.Empty;
    }
}
