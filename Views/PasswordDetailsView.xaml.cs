using SecureVault.Model;
using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class PasswordDetailsView : UserControl
    {
        private readonly MainViewModel _viewModel;
        private readonly Credential _credential;

        public PasswordDetailsView(MainViewModel viewModel, Credential credential)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _credential = credential;
            DataContext = _credential;
        }

        private void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_credential.EncryptedPassword))
            {
                MessageBox.Show("Ten wpis nie ma zapisanego hasła.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(_credential.EncryptedPassword);
            MessageBox.Show("Hasło skopiowane do schowka.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_credential.Account))
            {
                MessageBox.Show("Ten wpis nie ma zapisanego URL.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(_credential.Account);
            MessageBox.Show("URL skopiowany do schowka.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditPassword_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedCredential = _credential;
            SwitchTo(new AddPasswordView(_viewModel, true));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            SwitchTo(new MainView(_viewModel));
        }

        private void PasswordDetailsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SwitchTo(new MainView(_viewModel));
                e.Handled = true;
            }
        }

        private void SwitchTo(UIElement view)
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(view);
            }
        }
    }
}
