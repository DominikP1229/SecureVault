using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

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

        public static string ConnectionString => $"Data Source={DatabasePath};Foreign Keys=True";

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
            _initialized = true;
        }
    }
}
