using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureVault.Model.Services
{
    public class CredentialCsvImportResult
    {
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
    }

    public static class CredentialCsvService
    {
        private static readonly string[] Headers =
        [
            "Title",
            "Username",
            "Password",
            "Website",
            "Category",
            "Description",
            "ReminderEnabled",
            "ReminderMonths"
        ];

        public static async Task ExportAsync(IEnumerable<Credential> credentials, string filePath)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await using var writer = new StreamWriter(fileStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            await writer.WriteLineAsync(string.Join(",", Headers));

            foreach (var credential in credentials)
            {
                var values = new[]
                {
                    credential.Title,
                    credential.Username,
                    credential.EncryptedPassword,
                    credential.Account,
                    credential.Category,
                    credential.Description,
                    credential.PasswordReminderEnabled.ToString(CultureInfo.InvariantCulture),
                    credential.PasswordReminderMonths.ToString(CultureInfo.InvariantCulture)
                };

                await writer.WriteLineAsync(string.Join(",", values.Select(Escape)));
            }
        }

        public static async Task<IReadOnlyList<Credential>> ImportAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
            if (lines.Length == 0)
            {
                return Array.Empty<Credential>();
            }

            var headerMap = ParseLine(lines[0])
                .Select((header, index) => new { Header = header.Trim(), Index = index })
                .ToDictionary(item => item.Header, item => item.Index, StringComparer.OrdinalIgnoreCase);

            foreach (var requiredHeader in Headers[..5])
            {
                if (!headerMap.ContainsKey(requiredHeader))
                {
                    throw new InvalidDataException($"Missing required CSV column: {requiredHeader}.");
                }
            }

            var credentials = new List<Credential>();

            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var fields = ParseLine(lines[i]);
                var title = GetField(fields, headerMap, "Title").Trim();
                var username = GetField(fields, headerMap, "Username").Trim();
                var password = GetField(fields, headerMap, "Password");
                var website = GetField(fields, headerMap, "Website").Trim();
                var category = GetField(fields, headerMap, "Category").Trim();

                if (string.IsNullOrWhiteSpace(title) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(category))
                {
                    continue;
                }

                credentials.Add(new Credential
                {
                    Title = title,
                    Username = username,
                    EncryptedPassword = password,
                    Account = website,
                    Category = category,
                    Description = GetField(fields, headerMap, "Description").Trim(),
                    PasswordReminderEnabled = ParseBool(GetField(fields, headerMap, "ReminderEnabled")),
                    PasswordReminderMonths = ParseReminderMonths(GetField(fields, headerMap, "ReminderMonths")),
                    LastPasswordChangedAt = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                });
            }

            return credentials;
        }

        private static string GetField(IReadOnlyList<string> fields, IReadOnlyDictionary<string, int> headerMap, string header)
        {
            return headerMap.TryGetValue(header, out var index) && index < fields.Count
                ? fields[index]
                : string.Empty;
        }

        private static bool ParseBool(string value)
        {
            return bool.TryParse(value, out var result) && result;
        }

        private static int ParseReminderMonths(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var months) && months is >= 1 and <= 60
                ? months
                : 6;
        }

        private static string Escape(string value)
        {
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        private static List<string> ParseLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var character = line[i];

                if (character == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (character == ',' && !inQuotes)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(character);
            }

            fields.Add(current.ToString());
            return fields;
        }
    }
}
