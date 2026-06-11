using System;

namespace SecureVault.Model
{
    public class AccountSettings
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public bool PasswordReminderEnabled { get; set; } = true;
        public int PasswordReminderMonths { get; set; } = 6;
        public DateTime LastPasswordChangedAt { get; set; } = DateTime.Now;
    }
}
