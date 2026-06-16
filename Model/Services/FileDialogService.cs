using Microsoft.Win32;

namespace SecureVault.Model.Services
{
    public interface IFileDialogService
    {
        string? PickCsvToOpen();
        string? PickCsvToSave();
    }

    public class FileDialogService : IFileDialogService
    {
        private const string CsvFilter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

        public string? PickCsvToOpen()
        {
            var dialog = new OpenFileDialog
            {
                Filter = CsvFilter,
                DefaultExt = ".csv",
                CheckFileExists = true
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? PickCsvToSave()
        {
            var dialog = new SaveFileDialog
            {
                Filter = CsvFilter,
                DefaultExt = ".csv",
                FileName = "securevault-export.csv",
                OverwritePrompt = true
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
