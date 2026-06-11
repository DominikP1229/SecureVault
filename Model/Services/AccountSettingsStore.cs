using Microsoft.EntityFrameworkCore;
using SecureVault.Model.Data;
using System;
using System.Threading.Tasks;

namespace SecureVault.Model.Services
{
    public static class AccountSettingsStore
    {
        public static async Task<AccountSettings> GetOrCreateAsync(int accountId)
        {
            await using var dbContext = await DatabaseService.CreateContextAsync();
            var settings = await dbContext.AccountSettings.SingleOrDefaultAsync(item => item.AccountId == accountId);

            if (settings != null)
            {
                return settings;
            }

            settings = new AccountSettings
            {
                AccountId = accountId,
                PasswordReminderEnabled = true,
                PasswordReminderMonths = 6,
                LastPasswordChangedAt = DateTime.Now
            };

            dbContext.AccountSettings.Add(settings);
            await dbContext.SaveChangesAsync();
            return settings;
        }

        public static async Task SaveAsync(AccountSettings settings)
        {
            await using var dbContext = await DatabaseService.CreateContextAsync();
            dbContext.AccountSettings.Update(settings);
            await dbContext.SaveChangesAsync();
        }

        public static async Task MarkPasswordChangedAsync(int accountId)
        {
            var settings = await GetOrCreateAsync(accountId);
            settings.LastPasswordChangedAt = DateTime.Now;
            await SaveAsync(settings);
        }

        public static bool ShouldRemind(AccountSettings settings)
        {
            return settings.PasswordReminderEnabled &&
                settings.LastPasswordChangedAt.AddMonths(settings.PasswordReminderMonths) <= DateTime.Now;
        }
    }
}
