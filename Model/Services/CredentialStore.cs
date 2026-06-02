using SecureVault.Model.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureVault.Model.Services
{
    public static class CredentialStore
    {
        public static ObservableCollection<Credential> Load()
        {
            var encryption = VaultSession.RequireEncryption();

            using var dbContext = DatabaseService.CreateContext();
            var credentials = dbContext.Credentials
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

        public static void Add(Credential credential)
        {
            var encryption = VaultSession.RequireEncryption();

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

            using var dbContext = DatabaseService.CreateContext();
            var dbCredential = dbContext.Credentials.Single(item => item.Id == credential.Id);
            dbCredential.Title = credential.Title;
            dbCredential.Username = credential.Username;
            dbCredential.Category = credential.Category;
            dbCredential.Account = credential.Account;
            dbCredential.Website = credential.Website;
            dbCredential.Description = credential.Description;
            dbCredential.EncryptedPassword = encryption.Encrypt(credential.EncryptedPassword);
            dbContext.SaveChanges();
            PasswordHistoryStore.Add(credential, "Edited");
        }

        public static void Remove(Credential credential)
        {
            using var dbContext = DatabaseService.CreateContext();
            var dbCredential = dbContext.Credentials.Single(item => item.Id == credential.Id);
            dbContext.Credentials.Remove(dbCredential);
            dbContext.SaveChanges();
            PasswordHistoryStore.Add(credential, "Deleted");
        }

        private static Credential CopyForDatabase(Credential credential)
        {
            return new Credential
            {
                Id = credential.Id,
                Account = credential.Account,
                Title = credential.Title,
                Username = credential.Username,
                Category = credential.Category,
                Website = credential.Website,
                EncryptedPassword = credential.EncryptedPassword,
                Description = credential.Description
            };
        }
    }
}
