using System.Linq;
using System.Security.Cryptography;

namespace SecureVault.Model.Services
{
    public class PasswordGenerator
    {
        public string Generate(int length, bool includeSymbols)
        {
            const string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*()_-+=<>?";
            var chars = letters + digits + (includeSymbols ? symbols : string.Empty);

            if (length <= 0)
            {
                return string.Empty;
            }

            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
                .ToArray());
        }
    }
}
