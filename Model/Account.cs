using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureVault.Model
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public ICollection<Credential> Credentials { get; set; } = new List<Credential>();
        public AccountSettings? Settings { get; set; }
        public ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

        public override string ToString()
        {
            return Name;
        }
    }
}
