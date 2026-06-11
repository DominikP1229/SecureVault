using SecureVault.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void RegisterView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteRegister();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel.CancelCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteRegister();
                e.Handled = true;
            }
        }

        private void ExecuteRegister()
        {
            var password = _viewModel.Password;
            if (_viewModel.RegisterCommand.CanExecute(password))
            {
                _viewModel.RegisterCommand.Execute(password);
            }
        }

        private void HandleBackToLoginRequested()
        {
            _viewModel.BackToLoginRequested -= HandleBackToLoginRequested;

            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new LoginView());
            }
        }
    }
}
