using SecureVault.Model.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureVault.Model.Services
{
    public static class CredentialStore
    {
        public static ObservableCollection<Credential> Load()
        {
            var encryption = VaultSession.RequireEncryption();
            var account = VaultSession.CurrentAccount ?? throw new System.InvalidOperationException("Vault session is not initialized.");

            using var dbContext = DatabaseService.CreateContext();
            var credentials = dbContext.Credentials
                .Where(credential => credential.OwnerAccountId == account.Id)
                .OrderBy(credential => credential.Title)
                .ToList()
                .Select(credential =>
                {
                    encryption.TryDecrypt(credential.EncryptedPassword, out var plainPassword);
                    credential.EncryptedPassword = plainPassword;
                    return credential;
                });

            return new ObservableCollection<Credential>(credentials);
        }

        public static string[] GetExpiredPasswordReminderTitles()
        {
            var account = VaultSession.CurrentAccount ?? throw new System.InvalidOperationException("Vault session is not initialized.");

            using var dbContext = DatabaseService.CreateContext();
            return dbContext.Credentials
                .Where(credential =>
                    credential.OwnerAccountId == account.Id &&
                    credential.PasswordReminderEnabled &&
                    credential.LastPasswordChangedAt.AddMonths(credential.PasswordReminderMonths) <= DateTime.Now)
                .OrderBy(credential => credential.Title)
                .Select(credential => credential.Title)
                .ToArray();
        }

        public static void Add(Credential credential)
        {
            var encryption = VaultSession.RequireEncryption();
            var account = VaultSession.CurrentAccount ?? throw new System.InvalidOperationException("Vault session is not initialized.");

            credential.OwnerAccountId = account.Id;
            var dbCredential = CopyForDatabase(credential);
            dbCredential.EncryptedPassword = encryption.Encrypt(credential.EncryptedPassword);

            using var dbContext = DatabaseService.CreateContext();
            dbContext.Credentials.Add(dbCredential);
            dbContext.SaveChanges();
            PasswordHistoryStore.Add(credential, "Added");
        }

        public static void Update(Credential credential)
        {
            var encryption = VaultSession.RequireEncryption();
            var account = VaultSession.CurrentAccount ?? throw new System.InvalidOperationException("Vault session is not initialized.");

            using var dbContext = DatabaseService.CreateContext();
            var dbCredential = dbContext.Credentials.Single(item => item.Id == credential.Id && item.OwnerAccountId == account.Id);
            dbCredential.Title = credential.Title;
            dbCredential.Username = credential.Username;
            dbCredential.Category = credential.Category;
            dbCredential.Account = credential.Account;
            dbCredential.Website = credential.Website;
            dbCredential.Description = credential.Description;
            dbCredential.PasswordReminderEnabled = credential.PasswordReminderEnabled;
            dbCredential.PasswordReminderMonths = credential.PasswordReminderMonths;
            dbCredential.LastPasswordChangedAt = credential.LastPasswordChangedAt;
            dbCredential.EncryptedPassword = encryption.Encrypt(credential.EncryptedPassword);
            dbContext.SaveChanges();
            PasswordHistoryStore.Add(credential, "Edited");
        }

        public static void Remove(Credential credential)
        {
            var account = VaultSession.CurrentAccount ?? throw new System.InvalidOperationException("Vault session is not initialized.");

            using var dbContext = DatabaseService.CreateContext();
            var dbCredential = dbContext.Credentials.Single(item => item.Id == credential.Id && item.OwnerAccountId == account.Id);
            dbContext.Credentials.Remove(dbCredential);
            dbContext.SaveChanges();
            PasswordHistoryStore.Add(credential, "Deleted");
        }

        public static void ReEncryptForCurrentAccount(EncryptionService oldEncryption, EncryptionService newEncryption)
        {
            var account = VaultSession.CurrentAccount ?? throw new System.InvalidOperationException("Vault session is not initialized.");

            using var dbContext = DatabaseService.CreateContext();
            var credentials = dbContext.Credentials
                .Where(item => item.OwnerAccountId == account.Id)
                .ToList();

            foreach (var credential in credentials)
            {
                oldEncryption.TryDecrypt(credential.EncryptedPassword, out var plainPassword);
                credential.EncryptedPassword = newEncryption.Encrypt(plainPassword);
            }

            dbContext.SaveChanges();
        }

        private static Credential CopyForDatabase(Credential credential)
        {
            return new Credential
            {
                Id = credential.Id,
                OwnerAccountId = credential.OwnerAccountId,
                Account = credential.Account,
                Title = credential.Title,
                Username = credential.Username,
                Category = credential.Category,
                Website = credential.Website,
                EncryptedPassword = credential.EncryptedPassword,
                Description = credential.Description,
                PasswordReminderEnabled = credential.PasswordReminderEnabled,
                PasswordReminderMonths = credential.PasswordReminderMonths,
                LastPasswordChangedAt = credential.LastPasswordChangedAt
            };
        }
    }
}
