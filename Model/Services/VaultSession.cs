using SecureVault.Model;
using System;

namespace SecureVault.Model.Services
{
    public static class VaultSession
    {
        public static Account? CurrentAccount { get; private set; }
        public static EncryptionService? Encryption { get; private set; }

        public static bool IsSignedIn => CurrentAccount != null && Encryption != null;

        public static void SignIn(Account account, string accountPassword)
        {
            CurrentAccount = account;
            var masterPassword = PasswordService.CreateMasterPassword(account, accountPassword);
            Encryption = new EncryptionService(masterPassword);
        }

        public static void ChangePassword(string newAccountPassword)
        {
            var account = CurrentAccount ?? throw new InvalidOperationException("Vault session is not initialized.");
            var oldEncryption = RequireEncryption();
            var newMasterPassword = PasswordService.CreateMasterPassword(account, newAccountPassword);
            var newEncryption = new EncryptionService(newMasterPassword);

            CredentialStore.ReEncryptForCurrentAccount(oldEncryption, newEncryption);
            AccountStore.UpdatePassword(account, newAccountPassword);
            AccountSettingsStore.MarkPasswordChanged(account.Id);
            Encryption = newEncryption;
        }

        public static void SignOut()
        {
            CurrentAccount = null;
            Encryption = null;
        }

        public static EncryptionService RequireEncryption()
        {
            return Encryption ?? throw new InvalidOperationException("Vault session is not initialized.");
        }
    }
}
