using SecureVault.Model;
using System;
using System.Security.Cryptography;

namespace SecureVault.Model.Services
{
    public static class PasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;
        private const string Prefix = "PBKDF2";

        public static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = HashPassword(password, salt);
            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            var parts = storedPassword.Split('$');
            if (parts.Length != 4 || parts[0] != Prefix)
            {
                return password == storedPassword;
            }

            var iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var storedHash = Convert.FromBase64String(parts[3]);

            using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var testHash = derive.GetBytes(storedHash.Length);
            return CryptographicOperations.FixedTimeEquals(storedHash, testHash);
        }

        public static string CreateMasterPassword(Account account, string accountPassword)
        {
            return $"{account.Id}:{account.Name}:{accountPassword}";
        }

        public static bool IsHashed(string storedPassword)
        {
            return storedPassword.StartsWith($"{Prefix}$", StringComparison.Ordinal);
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            using var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            return derive.GetBytes(HashSize);
        }
    }
}
