using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureVault.Model
{
    public class Credential
    {
        public Credential()
        {
        }

        public string? Title { get; set; }
        public string? Username { get; set; }
        public string? Category { get; set; }
    }
}
