using System;
using System.Data;
using System.IO;
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

        public static SecureVaultDbContext CreateContext()
        {
            Initialize();
            return new SecureVaultDbContext();
        }

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            using var dbContext = new SecureVaultDbContext();
            dbContext.Database.EnsureCreated();
            EnsureSchema(dbContext);
            Seed(dbContext);
            _initialized = true;
        }

        private static void EnsureSchema(SecureVaultDbContext dbContext)
        {
            if (!ColumnExists(dbContext, "Credentials", "OwnerAccountId"))
            {
                dbContext.Database.ExecuteSqlRaw("""
                    ALTER TABLE Credentials
                    ADD COLUMN OwnerAccountId INTEGER NOT NULL DEFAULT 0;
                    """);
            }

            if (!ColumnExists(dbContext, "Credentials", "PasswordReminderEnabled"))
            {
                dbContext.Database.ExecuteSqlRaw("""
                    ALTER TABLE Credentials
                    ADD COLUMN PasswordReminderEnabled INTEGER NOT NULL DEFAULT 0;
                    """);
            }

            if (!ColumnExists(dbContext, "Credentials", "PasswordReminderMonths"))
            {
                dbContext.Database.ExecuteSqlRaw("""
                    ALTER TABLE Credentials
                    ADD COLUMN PasswordReminderMonths INTEGER NOT NULL DEFAULT 6;
                    """);
            }

            if (!ColumnExists(dbContext, "Credentials", "LastPasswordChangedAt"))
            {
                dbContext.Database.ExecuteSqlRaw("""
                    ALTER TABLE Credentials
                    ADD COLUMN LastPasswordChangedAt TEXT NOT NULL DEFAULT '2000-01-01 00:00:00';
                    """);

                dbContext.Database.ExecuteSqlRaw("""
                    UPDATE Credentials
                    SET LastPasswordChangedAt = datetime('now')
                    WHERE LastPasswordChangedAt = '2000-01-01 00:00:00';
                    """);
            }

            dbContext.Database.ExecuteSqlRaw("""
                UPDATE Credentials
                SET OwnerAccountId = (SELECT Id FROM Accounts ORDER BY Id LIMIT 1)
                WHERE OwnerAccountId = 0
                  AND EXISTS (SELECT 1 FROM Accounts);
                """);

            dbContext.Database.ExecuteSqlRaw("""
                DROP INDEX IF EXISTS IX_Credentials_Title_Category;
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE UNIQUE INDEX IF NOT EXISTS IX_Credentials_OwnerAccountId_Title_Category
                ON Credentials (OwnerAccountId, Title, Category);
                """);

            dbContext.Database.ExecuteSqlRaw("""
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

            dbContext.Database.ExecuteSqlRaw("""
                CREATE INDEX IF NOT EXISTS IX_PasswordHistories_AccountId
                ON PasswordHistories (AccountId);
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE INDEX IF NOT EXISTS IX_PasswordHistories_CredentialId
                ON PasswordHistories (CredentialId);
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS AccountSettings (
                    Id INTEGER NOT NULL CONSTRAINT PK_AccountSettings PRIMARY KEY AUTOINCREMENT,
                    AccountId INTEGER NOT NULL,
                    PasswordReminderEnabled INTEGER NOT NULL DEFAULT 1,
                    PasswordReminderMonths INTEGER NOT NULL DEFAULT 6,
                    LastPasswordChangedAt TEXT NOT NULL
                );
                """);

            dbContext.Database.ExecuteSqlRaw("""
                CREATE UNIQUE INDEX IF NOT EXISTS IX_AccountSettings_AccountId
                ON AccountSettings (AccountId);
                """);
        }

        private static bool ColumnExists(SecureVaultDbContext dbContext, string tableName, string columnName)
        {
            var connection = dbContext.Database.GetDbConnection();
            var shouldClose = connection.State == ConnectionState.Closed;

            if (shouldClose)
            {
                connection.Open();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info({tableName});";

                using var reader = command.ExecuteReader();
                while (reader.Read())
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
                    connection.Close();
                }
            }
        }

        private static void Seed(SecureVaultDbContext dbContext)
        {
            if (!dbContext.Accounts.Any())
            {
                dbContext.Accounts.Add(new Account
                {
                    Name = "admin",
                    Password = PasswordService.HashPassword("admin")
                });
            }

            if (!dbContext.Categories.Any())
            {
                dbContext.Categories.AddRange(
                    new Category { CategoryType = "Social" },
                    new Category { CategoryType = "Work" },
                    new Category { CategoryType = "Finance" });
            }

            dbContext.SaveChanges();

            dbContext.Database.ExecuteSqlRaw("""
                INSERT OR IGNORE INTO AccountSettings
                    (AccountId, PasswordReminderEnabled, PasswordReminderMonths, LastPasswordChangedAt)
                SELECT Id, 1, 6, datetime('now')
                FROM Accounts;
                """);

            if (!dbContext.Credentials.Any())
            {
                var ownerAccountId = dbContext.Accounts
                    .OrderBy(account => account.Id)
                    .Select(account => account.Id)
                    .First();

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

            dbContext.SaveChanges();
        }
    }
}
