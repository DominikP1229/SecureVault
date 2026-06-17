using System.Linq;
using System.Security.Cryptography;

namespace SecureVault.Model.Services
{
    public class PasswordGenerator
    {
        public string Generate(int length, bool includeSymbols)
        {
            const string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerLetters = "abcdefghijklmnopqrstuvwxyz";
            const string upperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*()_-+=<>?";
            var chars = letters + digits + (includeSymbols ? symbols : string.Empty);

            if (length <= 0)
            {
                return string.Empty;
            }

            var requiredChars = new[]
            {
                GetRandomChar(lowerLetters),
                GetRandomChar(upperLetters),
                GetRandomChar(digits)
            }.ToList();

            if (includeSymbols)
            {
                requiredChars.Add(GetRandomChar(symbols));
            }

            var passwordChars = requiredChars
                .Take(length)
                .Concat(Enumerable.Range(0, Math.Max(0, length - requiredChars.Count))
                    .Select(_ => GetRandomChar(chars)))
                .ToArray();

            Shuffle(passwordChars);
            return new string(passwordChars);
        }

        private static char GetRandomChar(string chars)
        {
            return chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }

        private static void Shuffle(char[] chars)
        {
            for (var i = chars.Length - 1; i > 0; i--)
            {
                var j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
        }
    }
}
