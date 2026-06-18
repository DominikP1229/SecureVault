using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;

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

        private void HandleNavigationRequested(LoginNavigationTarget target)
        {
            _viewModel.NavigationRequested -= HandleNavigationRequested;

            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.SwitchView(target == LoginNavigationTarget.Main
                    ? new MainView()
                    : new RegisterView());
            }
        }
    }
}
