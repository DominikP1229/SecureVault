using SecureVault.Model.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureVault.Model.Services
{
    public static class PasswordHistoryStore
    {
        public static ObservableCollection<PasswordHistory> LoadForCurrentAccount()
        {
            var account = VaultSession.CurrentAccount;
            if (account == null)
            {
                return new ObservableCollection<PasswordHistory>();
            }

            using var dbContext = DatabaseService.CreateContext();
            return new ObservableCollection<PasswordHistory>(
                dbContext.PasswordHistories
                    .Where(history => history.AccountId == account.Id)
                    .OrderByDescending(history => history.ChangedDate)
                    .ToList());
        }

        public static void Add(Credential credential, string action)
        {
            var account = VaultSession.CurrentAccount;
            var encryption = VaultSession.Encryption;

            if (account == null || encryption == null)
            {
                return;
            }

            using var dbContext = DatabaseService.CreateContext();
            dbContext.PasswordHistories.Add(new PasswordHistory
            {
                CredentialId = credential.Id,
                AccountId = account.Id,
                Action = action,
                CredentialTitle = credential.Title,
                EncryptedPassword = encryption.Encrypt(credential.EncryptedPassword),
                ChangedDate = DateTime.Now
            });
            dbContext.SaveChanges();
        }
    }
}
