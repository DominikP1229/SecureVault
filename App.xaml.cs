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
                MessageBox.Show(
                    $"Could not initialize the database: {ex.Message}",
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
