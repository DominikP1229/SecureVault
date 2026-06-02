using SecureVault.Model.Data;
using System.Windows;

namespace SecureVault
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                DatabaseService.Initialize();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"Nie udało się zainicjalizować bazy danych: {ex.Message}",
                    "Błąd bazy danych",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
