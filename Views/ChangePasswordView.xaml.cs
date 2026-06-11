using SecureVault.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class ChangePasswordView : UserControl
    {
        private readonly ChangePasswordViewModel _viewModel = new();

        public ChangePasswordView()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _viewModel.BackRequested += NavigateBack;
        }

        private void ChangePasswordView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.SaveCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _viewModel.BackCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void NavigateBack()
        {
            _viewModel.BackRequested -= NavigateBack;

            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new SettingsView();
            }
        }
    }
}
