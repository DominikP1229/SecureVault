using SecureVault.Model.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureVault.Model.Services
{
    public static class AccountStore
    {
        public static ObservableCollection<Account> Accounts { get; } = new();

        static AccountStore()
        {
            Load();
        }

        public static bool Exists(string name)
        {
            return Accounts.Any(account => account.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        public static void Add(string name, string password)
        {
            var account = new Account
            {
                Name = name.Trim(),
                Password = PasswordService.HashPassword(password)
            };

            using var dbContext = DatabaseService.CreateContext();
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();
            Accounts.Add(account);
        }

        public static void UpdatePassword(Account account, string password)
        {
            account.Password = PasswordService.HashPassword(password);

            using var dbContext = DatabaseService.CreateContext();
            dbContext.Accounts.Update(account);
            dbContext.SaveChanges();
        }

        private static void Load()
        {
            using var dbContext = DatabaseService.CreateContext();
            Accounts.Clear();

            foreach (var account in dbContext.Accounts.OrderBy(account => account.Name))
            {
                Accounts.Add(account);
            }
        }
    }
}
