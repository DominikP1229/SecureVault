using SecureVault.Model.Data;
using SecureVault.Model.Services;
using System.Windows;

namespace SecureVault
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await DatabaseService.InitializeAsync();
                await AccountStore.LoadAsync();
                await CategoryStore.LoadAsync();
            }
            catch (System.Exception ex)
            {
                await NotificationService.ShowErrorAsync(
                    "Database error",
                    $"Could not initialize the database: {ex.Message}");
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
