using Microsoft.EntityFrameworkCore;
using SecureVault.Model.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SecureVault.Model.Services
{
    public static class PasswordHistoryStore
    {
        public static async Task<ObservableCollection<PasswordHistory>> LoadForCurrentAccountAsync()
        {
            var account = VaultSession.CurrentAccount;
            if (account == null)
            {
                return new ObservableCollection<PasswordHistory>();
            }

            await using var dbContext = await DatabaseService.CreateContextAsync();
            var historyItems = await dbContext.PasswordHistories
                .Where(history => history.AccountId == account.Id)
                .OrderByDescending(history => history.ChangedDate)
                .ToListAsync();

            return new ObservableCollection<PasswordHistory>(historyItems);
        }

        public static async Task AddAsync(Credential credential, string action)
        {
            var account = VaultSession.CurrentAccount;
            var encryption = VaultSession.Encryption;

            if (account == null || encryption == null)
            {
                return;
            }

            await using var dbContext = await DatabaseService.CreateContextAsync();
            dbContext.PasswordHistories.Add(new PasswordHistory
            {
                CredentialId = credential.Id,
                AccountId = account.Id,
                Action = action,
                CredentialTitle = credential.Title,
                EncryptedPassword = encryption.Encrypt(credential.EncryptedPassword),
                ChangedDate = DateTime.Now
            });
            await dbContext.SaveChangesAsync();
        }
    }
}
