using System;

namespace SecureVault.Model
{
    public class PasswordHistory
    {
        public int Id { get; set; }
        public Guid CredentialId { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public string Action { get; set; } = string.Empty;
        public string CredentialTitle { get; set; } = string.Empty;
        public string EncryptedPassword { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; } = DateTime.Now;
    }
}
