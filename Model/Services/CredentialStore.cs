using Microsoft.EntityFrameworkCore;
using SecureVault.Model.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SecureVault.Model.Services
{
    public static class CredentialStore
    {
        public static async Task<ObservableCollection<Credential>> LoadAsync()
        {
            var encryption = VaultSession.RequireEncryption();
            var account = VaultSession.CurrentAccount ?? throw new InvalidOperationException("Vault session is not initialized.");

            await using var dbContext = await DatabaseService.CreateContextAsync();
            var credentials = await dbContext.Credentials
                .Where(credential => credential.OwnerAccountId == account.Id)
                .OrderBy(credential => credential.Title)
                .ToListAsync();

            foreach (var credential in credentials)
            {
                encryption.TryDecrypt(credential.EncryptedPassword, out var plainPassword);
                credential.EncryptedPassword = plainPassword;
            }

            return new ObservableCollection<Credential>(credentials);
        }

        public static async Task AddAsync(Credential credential)
        {
            var encryption = VaultSession.RequireEncryption();
            var account = VaultSession.CurrentAccount ?? throw new InvalidOperationException("Vault session is not initialized.");

            credential.OwnerAccountId = account.Id;
            var dbCredential = CopyForDatabase(credential);
            dbCredential.EncryptedPassword = encryption.Encrypt(credential.EncryptedPassword);

            await using var dbContext = await DatabaseService.CreateContextAsync();
            dbContext.Credentials.Add(dbCredential);
            await dbContext.SaveChangesAsync();
            await PasswordHistoryStore.AddAsync(credential, "Added");
        }

        public static async Task UpdateAsync(Credential credential)
        {
            var encryption = VaultSession.RequireEncryption();
            var account = VaultSession.CurrentAccount ?? throw new InvalidOperationException("Vault session is not initialized.");

            await using var dbContext = await DatabaseService.CreateContextAsync();
            var dbCredential = await dbContext.Credentials.SingleAsync(item => item.Id == credential.Id && item.OwnerAccountId == account.Id);
            dbCredential.Title = credential.Title;
            dbCredential.Username = credential.Username;
            dbCredential.Category = credential.Category;
            dbCredential.Account = credential.Account;
            dbCredential.Website = credential.Website;
            dbCredential.Description = credential.Description;
            dbCredential.PasswordReminderEnabled = credential.PasswordReminderEnabled;
            dbCredential.PasswordReminderMonths = credential.PasswordReminderMonths;
            dbCredential.LastPasswordChangedAt = credential.LastPasswordChangedAt;
            dbCredential.ModifiedDate = DateTime.Now;
            dbCredential.EncryptedPassword = encryption.Encrypt(credential.EncryptedPassword);
            credential.ModifiedDate = dbCredential.ModifiedDate;
            await dbContext.SaveChangesAsync();
            await PasswordHistoryStore.AddAsync(credential, "Edited");
        }

        public static async Task RemoveAsync(Credential credential)
        {
            var account = VaultSession.CurrentAccount ?? throw new InvalidOperationException("Vault session is not initialized.");

            await using var dbContext = await DatabaseService.CreateContextAsync();
            var dbCredential = await dbContext.Credentials.SingleAsync(item => item.Id == credential.Id && item.OwnerAccountId == account.Id);
            dbContext.Credentials.Remove(dbCredential);
            await dbContext.SaveChangesAsync();
            await PasswordHistoryStore.AddAsync(credential, "Deleted");
        }

        public static async Task ReEncryptForCurrentAccountAsync(EncryptionService oldEncryption, EncryptionService newEncryption)
        {
            var account = VaultSession.CurrentAccount ?? throw new InvalidOperationException("Vault session is not initialized.");

            await using var dbContext = await DatabaseService.CreateContextAsync();
            var credentials = await dbContext.Credentials
                .Where(item => item.OwnerAccountId == account.Id)
                .ToListAsync();

            foreach (var credential in credentials)
            {
                oldEncryption.TryDecrypt(credential.EncryptedPassword, out var plainPassword);
                credential.EncryptedPassword = newEncryption.Encrypt(plainPassword);
            }

            await dbContext.SaveChangesAsync();
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
                LastPasswordChangedAt = credential.LastPasswordChangedAt,
                CreatedDate = credential.CreatedDate,
                ModifiedDate = credential.ModifiedDate
            };
        }
    }
}
