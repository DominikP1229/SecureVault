using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureVault.Model.Services;

namespace SecureVault.Model.Data
{
    public static class DatabaseService
    {
        private static bool _initialized;

        public static string DatabasePath
        {
            get
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appDirectory = Path.Combine(appDataPath, "SecureVault");
                Directory.CreateDirectory(appDirectory);
                return Path.Combine(appDirectory, "securevault.db");
            }
        }

        public static string ConnectionString => $"Data Source={DatabasePath}";

        public static async Task<SecureVaultDbContext> CreateContextAsync()
        {
            await InitializeAsync();
            return new SecureVaultDbContext();
        }

        public static async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            await using var dbContext = new SecureVaultDbContext();
            await dbContext.Database.EnsureCreatedAsync();
            await EnsureSchemaAsync(dbContext);
            await SeedAsync(dbContext);
            _initialized = true;
        }

        private static async Task EnsureSchemaAsync(SecureVaultDbContext dbContext)
        {
            if (!await ColumnExistsAsync(dbContext, "Credentials", "OwnerAccountId"))
            {
                await dbContext.Database.ExecuteSqlRawAsync("""
                    ALTER TABLE Credentials
                    ADD COLUMN OwnerAccountId INTEGER NOT NULL DEFAULT 0;
                    """);
            }

            if (!await ColumnExistsAsync(dbContext, "Credentials", "PasswordReminderEnabled"))
            {
                await dbContext.Database.ExecuteSqlRawAsync("""
                    ALTER TABLE Credentials
                    ADD COLUMN PasswordReminderEnabled INTEGER NOT NULL DEFAULT 0;
                    """);
            }

            if (!await ColumnExistsAsync(dbContext, "Credentials", "PasswordReminderMonths"))
            {
                await dbContext.Database.ExecuteSqlRawAsync("""
                    ALTER TABLE Credentials
                    ADD COLUMN PasswordReminderMonths INTEGER NOT NULL DEFAULT 6;
                    """);
            }

            if (!await ColumnExistsAsync(dbContext, "Credentials", "LastPasswordChangedAt"))
            {
                await dbContext.Database.ExecuteSqlRawAsync("""
                    ALTER TABLE Credentials
                    ADD COLUMN LastPasswordChangedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';
                    """);

                await dbContext.Database.ExecuteSqlRawAsync("""
                    UPDATE Credentials
                    SET LastPasswordChangedAt = datetime('now')
                    WHERE LastPasswordChangedAt = '2000-01-01 00:00:00';
                    """);
            }

            await dbContext.Database.ExecuteSqlRawAsync("""
                UPDATE Credentials
                SET OwnerAccountId = (SELECT Id FROM Accounts ORDER BY Id LIMIT 1)
                WHERE OwnerAccountId = 0
                  AND EXISTS (SELECT 1 FROM Accounts);
                """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                DROP INDEX IF EXISTS IX_Credentials_Title_Category;
                """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                CREATE UNIQUE INDEX IF NOT EXISTS IX_Credentials_OwnerAccountId_Title_Category
                ON Credentials (OwnerAccountId, Title, Category);
                """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS PasswordHistories (
                    Id INTEGER NOT NULL CONSTRAINT PK_PasswordHistories PRIMARY KEY AUTOINCREMENT,
                    CredentialId TEXT NOT NULL,
                    AccountId INTEGER NOT NULL,
                    Action TEXT NOT NULL,
                    CredentialTitle TEXT NOT NULL,
                    EncryptedPassword TEXT NOT NULL,
                    ChangedDate TEXT NOT NULL
                );
                """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                CREATE INDEX IF NOT EXISTS IX_PasswordHistories_AccountId
                ON PasswordHistories (AccountId);
                """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                CREATE INDEX IF NOT EXISTS IX_PasswordHistories_CredentialId
                ON PasswordHistories (CredentialId);
                """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS AccountSettings (
                    Id INTEGER NOT NULL CONSTRAINT PK_AccountSettings PRIMARY KEY AUTOINCREMENT,
                    AccountId INTEGER NOT NULL,
                    PasswordReminderEnabled INTEGER NOT NULL DEFAULT 1,
                    PasswordReminderMonths INTEGER NOT NULL DEFAULT 6,
                    LastPasswordChangedAt TEXT NOT NULL
                );
                """);

            await dbContext.Database.ExecuteSqlRawAsync("""
                CREATE UNIQUE INDEX IF NOT EXISTS IX_AccountSettings_AccountId
                ON AccountSettings (AccountId);
                """);
        }

        private static async Task<bool> ColumnExistsAsync(SecureVaultDbContext dbContext, string tableName, string columnName)
        {
            var connection = dbContext.Database.GetDbConnection();
            var shouldClose = connection.State == ConnectionState.Closed;

            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info({tableName});";

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (reader["name"]?.ToString() == columnName)
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static async Task SeedAsync(SecureVaultDbContext dbContext)
        {
            if (!await dbContext.Accounts.AnyAsync())
            {
                dbContext.Accounts.Add(new Account
                {
                    Name = "admin",
                    Password = PasswordService.HashPassword("admin")
                });
            }

            if (!await dbContext.Categories.AnyAsync())
            {
                dbContext.Categories.AddRange(
                    new Category { CategoryType = "Social" },
                    new Category { CategoryType = "Work" },
                    new Category { CategoryType = "Finance" });
            }

            await dbContext.SaveChangesAsync();

            await dbContext.Database.ExecuteSqlRawAsync("""
                INSERT OR IGNORE INTO AccountSettings
                    (AccountId, PasswordReminderEnabled, PasswordReminderMonths, LastPasswordChangedAt)
                SELECT Id, 1, 6, datetime('now')
                FROM Accounts;
                """);

            if (!await dbContext.Credentials.AnyAsync())
            {
                var ownerAccountId = await dbContext.Accounts
                    .OrderBy(account => account.Id)
                    .Select(account => account.Id)
                    .FirstAsync();

                dbContext.Credentials.AddRange(
                    new Credential
                    {
                        OwnerAccountId = ownerAccountId,
                        Title = "Gmail",
                        Username = "jan.kowalski@gmail.com",
                        Category = "Work",
                        EncryptedPassword = "Password123!",
                        Account = "https://mail.google.com",
                        PasswordReminderEnabled = true
                    },
                    new Credential
                    {
                        OwnerAccountId = ownerAccountId,
                        Title = "Facebook",
                        Username = "janek123",
                        Category = "Social",
                        EncryptedPassword = "Facebook123!",
                        Account = "https://facebook.com",
                        PasswordReminderEnabled = true
                    },
                    new Credential
                    {
                        OwnerAccountId = ownerAccountId,
                        Title = "Bank",
                        Username = "jan_k",
                        Category = "Finance",
                        EncryptedPassword = "BankPassword123!",
                        Account = "https://bank.example.com",
                        PasswordReminderEnabled = true
                    });
            }

            await dbContext.SaveChangesAsync();
        }

    }
}
