using SecureVault.Model.Data;
using System;
using System.Linq;

namespace SecureVault.Model.Services
{
    public static class AccountSettingsStore
    {
        public static AccountSettings GetOrCreate(int accountId)
        {
            using var dbContext = DatabaseService.CreateContext();
            var settings = dbContext.AccountSettings.SingleOrDefault(item => item.AccountId == accountId);

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
            dbContext.SaveChanges();
            return settings;
        }

        public static void Save(AccountSettings settings)
        {
            using var dbContext = DatabaseService.CreateContext();
            dbContext.AccountSettings.Update(settings);
            dbContext.SaveChanges();
        }

        public static void MarkPasswordChanged(int accountId)
        {
            var settings = GetOrCreate(accountId);
            settings.LastPasswordChangedAt = DateTime.Now;
            Save(settings);
        }

        public static bool ShouldRemind(AccountSettings settings)
        {
            return settings.PasswordReminderEnabled &&
                settings.LastPasswordChangedAt.AddMonths(settings.PasswordReminderMonths) <= DateTime.Now;
        }
    }
}
