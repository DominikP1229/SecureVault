using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly SettingsViewModel _viewModel = new();

        public SettingsView()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _viewModel.CloseRequested += CloseSubView;
            _viewModel.ChangePasswordRequested += OpenChangePasswordView;
        }

        private void SettingsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _viewModel.CloseCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                _viewModel.SaveReminderSettingsCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void OpenChangePasswordView()
        {
            if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = new ChangePasswordView();
            }
        }

        private void CloseSubView()
        {
            _viewModel.CloseRequested -= CloseSubView;
            _viewModel.ChangePasswordRequested -= OpenChangePasswordView;

            var parent = this.Parent as FrameworkElement;

            while (parent != null && parent.Name != "SubViewContainer")
            {
                parent = parent.Parent as FrameworkElement;
            }

            if (parent is Border border)
            {
                border.Visibility = Visibility.Collapsed;

                if (border.Child is ContentControl content)
                {
                    content.Content = null;
                }
            }
        }
    }
}
