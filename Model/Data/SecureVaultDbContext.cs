using Microsoft.EntityFrameworkCore;
using SecureVault.Model;

namespace SecureVault.Model.Data
{
    public class SecureVaultDbContext : DbContext
    {
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Credential> Credentials => Set<Credential>();
        public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
        public DbSet<AccountSettings> AccountSettings => Set<AccountSettings>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(DatabaseService.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasIndex(account => account.Name)
                .IsUnique();

            modelBuilder.Entity<AccountSettings>()
                .HasIndex(settings => settings.AccountId)
                .IsUnique();

            modelBuilder.Entity<AccountSettings>()
                .HasOne(settings => settings.Account)
                .WithOne(account => account.Settings)
                .HasForeignKey<AccountSettings>(settings => settings.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasIndex(category => category.CategoryType)
                .IsUnique();

            modelBuilder.Entity<Credential>()
                .HasIndex(credential => new { credential.OwnerAccountId, credential.Title, credential.Category })
                .IsUnique();

            modelBuilder.Entity<Credential>()
                .HasOne(credential => credential.OwnerAccount)
                .WithMany(account => account.Credentials)
                .HasForeignKey(credential => credential.OwnerAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordHistory>()
                .HasIndex(history => history.AccountId);

            modelBuilder.Entity<PasswordHistory>()
                .HasOne(history => history.Account)
                .WithMany(account => account.PasswordHistories)
                .HasForeignKey(history => history.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PasswordHistory>()
                .HasIndex(history => history.CredentialId);
        }
    }
}
