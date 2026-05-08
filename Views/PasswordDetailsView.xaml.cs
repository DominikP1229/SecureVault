using SecureVault.Model;
using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;

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
            Clipboard.SetText(_credential.EncryptedPassword);
        }

        private void CopyUrl_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_credential.Account);
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

        private void SwitchTo(UIElement view)
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(view);
            }
        }
    }
}
