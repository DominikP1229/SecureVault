using SecureVault.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class LoginView : UserControl
    {
        private readonly LoginViewModel _viewModel = new();

        public LoginView()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _viewModel.NavigationRequested += HandleNavigationRequested;
        }

        private void LoginView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteLogin();
                e.Handled = true;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteLogin();
                e.Handled = true;
            }
        }

        private void ExecuteLogin()
        {
            var password = _viewModel.Password;
            if (_viewModel.LoginCommand.CanExecute(password))
            {
                _viewModel.LoginCommand.Execute(password);
            }
        }

        private void HandleNavigationRequested(LoginNavigationTarget target)
        {
            _viewModel.NavigationRequested -= HandleNavigationRequested;

            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(target == LoginNavigationTarget.Main
                    ? new MainView()
                    : new RegisterView());
            }
        }
    }
}
