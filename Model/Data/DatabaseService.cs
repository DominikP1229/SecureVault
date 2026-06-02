using System;
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

            if (!dbContext.Credentials.Any())
            {
                dbContext.Credentials.AddRange(
                    new Credential
                    {
                        Title = "Gmail",
                        Username = "jan.kowalski@gmail.com",
                        Category = "Work",
                        EncryptedPassword = "Password123!",
                        Account = "https://mail.google.com"
                    },
                    new Credential
                    {
                        Title = "Facebook",
                        Username = "janek123",
                        Category = "Social",
                        EncryptedPassword = "Facebook123!",
                        Account = "https://facebook.com"
                    },
                    new Credential
                    {
                        Title = "Bank",
                        Username = "jan_k",
                        Category = "Finance",
                        EncryptedPassword = "BankPassword123!",
                        Account = "https://bank.example.com"
                    });
            }

            dbContext.SaveChanges();
        }
    }
}
