using System.Text.RegularExpressions;

namespace SecureVault.Model.Services
{
    public class PasswordStrengthService
    {
        public int EvaluateStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return 0;
            }

            var score = 0;

            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (Regex.IsMatch(password, "[a-z]")) score++;
            if (Regex.IsMatch(password, "[A-Z]")) score++;
            if (Regex.IsMatch(password, "[0-9]")) score++;
            if (Regex.IsMatch(password, "[^a-zA-Z0-9]")) score++;

            return score * 100 / 6;
        }
    }
}
