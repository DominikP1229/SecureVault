using Microsoft.EntityFrameworkCore;
using SecureVault.Model.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SecureVault.Model.Services
{
    public static class AccountStore
    {
        public static ObservableCollection<Account> Accounts { get; } = new();

        public static bool Exists(string name)
        {
            return Accounts.Any(account => account.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        public static async Task LoadAsync()
        {
            await using var dbContext = await DatabaseService.CreateContextAsync();
            var accounts = await dbContext.Accounts
                .OrderBy(account => account.Name)
                .ToListAsync();

            Accounts.Clear();
            foreach (var account in accounts)
            {
                Accounts.Add(account);
            }
        }

        public static async Task<Account> AddAsync(string name, string password)
        {
            var account = new Account
            {
                Name = name.Trim(),
                Password = PasswordService.HashPassword(password)
            };

            await using var dbContext = await DatabaseService.CreateContextAsync();
            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();
            Accounts.Add(account);
            return account;
        }

        public static async Task UpdatePasswordAsync(Account account, string password)
        {
            account.Password = PasswordService.HashPassword(password);

            await using var dbContext = await DatabaseService.CreateContextAsync();
            dbContext.Accounts.Update(account);
            await dbContext.SaveChangesAsync();
        }
    }
}
