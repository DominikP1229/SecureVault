using SecureVault.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureVault.Model.Services
{
    public static class AccountStore
    {
        public static ObservableCollection<Account> Accounts { get; } = new()
        {
            new Account
            {
                Name = "admin",
                Password = "admin"
            }
        };

        public static bool Exists(string name)
        {
            return Accounts.Any(account => account.Name == name);
        }

        public static void Add(string name, string password)
        {
            Accounts.Add(new Account
            {
                Name = name,
                Password = password
            });
        }
    }
}
