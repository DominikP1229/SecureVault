using SecureVault.Model;
using SecureVault.Model.Services;
using System;
using System.Windows;

namespace SecureVault.ViewModel
{
    public class PasswordDetailsViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public Credential Credential { get; }
        public RelayCommand CopyPasswordCommand { get; }
        public RelayCommand CopyUrlCommand { get; }
        public RelayCommand EditPasswordCommand { get; }
        public RelayCommand BackCommand { get; }

        public event Action<PasswordDetailsNavigationTarget>? NavigationRequested;

        public PasswordDetailsViewModel(MainViewModel mainViewModel, Credential credential)
        {
            _mainViewModel = mainViewModel;
            Credential = credential;
            CopyPasswordCommand = new RelayCommand(CopyPassword);
            CopyUrlCommand = new RelayCommand(CopyUrl);
            EditPasswordCommand = new RelayCommand(EditPassword);
            BackCommand = new RelayCommand(() => NavigationRequested?.Invoke(PasswordDetailsNavigationTarget.Main));
        }

        private void CopyPassword()
        {
            if (string.IsNullOrEmpty(Credential.EncryptedPassword))
            {
                MessageBox.Show("Ten wpis nie ma zapisanego hasla.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(Credential.EncryptedPassword);
            MessageBox.Show("Haslo skopiowane do schowka.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyUrl()
        {
            if (string.IsNullOrEmpty(Credential.Account))
            {
                MessageBox.Show("Ten wpis nie ma zapisanego URL.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(Credential.Account);
            MessageBox.Show("URL skopiowany do schowka.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
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
