using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SecureVault.Model
{
    public class Credential
    {
        public Credential()
        {
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public int OwnerAccountId { get; set; }
        public string Account {  get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Website {get; set; } = string.Empty;
        public string EncryptedPassword { get; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public bool PasswordReminderEnabled { get; set; } = false;
        public int PasswordReminderMonths { get; set; } = 6;
        public DateTime LastPasswordChangedAt { get; set; } = DateTime.Now;
    }
}
