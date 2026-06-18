using SecureVault.Model;
using SecureVault.Model.Services;
using System;
using System.Windows;
using System.Threading.Tasks;

namespace SecureVault.ViewModel
{
    public class PasswordDetailsViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public Credential Credential { get; }
        public AsyncRelayCommand CopyPasswordCommand { get; }
        public AsyncRelayCommand CopyUrlCommand { get; }
        public RelayCommand EditPasswordCommand { get; }
        public RelayCommand BackCommand { get; }

        public event Action<PasswordDetailsNavigationTarget>? NavigationRequested;

        public PasswordDetailsViewModel(MainViewModel mainViewModel, Credential credential)
        {
            _mainViewModel = mainViewModel;
            Credential = credential;
            CopyPasswordCommand = new AsyncRelayCommand(CopyPasswordAsync);
            CopyUrlCommand = new AsyncRelayCommand(CopyUrlAsync);
            EditPasswordCommand = new RelayCommand(EditPassword);
            BackCommand = new RelayCommand(() => NavigationRequested?.Invoke(PasswordDetailsNavigationTarget.Main));
        }

        private async Task CopyPasswordAsync()
        {
            if (string.IsNullOrEmpty(Credential.EncryptedPassword))
            {
                await NotificationService.ShowInformationAsync("Copy", "This entry does not have a saved password.");
                return;
            }

            Clipboard.SetText(Credential.EncryptedPassword);
            await NotificationService.ShowInformationAsync("Copy", "Password copied to clipboard.");
        }

        private async Task CopyUrlAsync()
        {
            if (string.IsNullOrEmpty(Credential.Account))
            {
                await NotificationService.ShowInformationAsync("Copy", "This entry does not have a saved URL.");
                return;
            }

            Clipboard.SetText(Credential.Account);
            await NotificationService.ShowInformationAsync("Copy", "URL copied to clipboard.");
        }

        private void EditPassword()
        {
            _mainViewModel.SelectedCredential = Credential;
            _mainViewModel.IsEditMode = true;
            NavigationRequested?.Invoke(PasswordDetailsNavigationTarget.Edit);
        }
    }

    public enum PasswordDetailsNavigationTarget
    {
        Main,
        Edit
    }
}
