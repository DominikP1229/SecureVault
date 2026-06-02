using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureVault.Model.Services
{
    public sealed class EncryptionService
    {
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        private readonly string _masterPassword;

        public EncryptionService(string masterPassword)
        {
            if (string.IsNullOrWhiteSpace(masterPassword))
            {
                throw new ArgumentException("Master password cannot be empty.", nameof(masterPassword));
            }

            _masterPassword = masterPassword;
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var cipherBytes = new byte[plainBytes.Length];
            var tag = new byte[TagSize];

            using var derive = new Rfc2898DeriveBytes(_masterPassword, salt, Iterations, HashAlgorithmName.SHA256);
            var key = derive.GetBytes(KeySize);

            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

            var result = new byte[salt.Length + nonce.Length + tag.Length + cipherBytes.Length];
            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(nonce, 0, result, salt.Length, nonce.Length);
            Buffer.BlockCopy(tag, 0, result, salt.Length + nonce.Length, tag.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, salt.Length + nonce.Length + tag.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            var allBytes = Convert.FromBase64String(cipherText);
            if (allBytes.Length < SaltSize + NonceSize + TagSize)
            {
                throw new CryptographicException("Invalid encrypted payload.");
            }

            var salt = allBytes[..SaltSize];
            var nonce = allBytes[SaltSize..(SaltSize + NonceSize)];
            var tag = allBytes[(SaltSize + NonceSize)..(SaltSize + NonceSize + TagSize)];
            var cipherBytes = allBytes[(SaltSize + NonceSize + TagSize)..];

            using var derive = new Rfc2898DeriveBytes(_masterPassword, salt, Iterations, HashAlgorithmName.SHA256);
            var key = derive.GetBytes(KeySize);

            var plainBytes = new byte[cipherBytes.Length];
            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }

        public bool TryDecrypt(string cipherText, out string plainText)
        {
            try
            {
                plainText = Decrypt(cipherText);
                return true;
            }
            catch (FormatException)
            {
                plainText = cipherText;
                return false;
            }
            catch (CryptographicException)
            {
                plainText = cipherText;
                return false;
            }
        }
    }
}
