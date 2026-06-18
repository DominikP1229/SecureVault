using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class RegisterView : UserControl
    {
        private readonly RegisterViewModel _viewModel = new();

        public RegisterView()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _viewModel.BackToLoginRequested += HandleBackToLoginRequested;
        }

        private void HandleBackToLoginRequested()
        {
            _viewModel.BackToLoginRequested -= HandleBackToLoginRequested;

            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new LoginView());
            }
        }
    }
}
