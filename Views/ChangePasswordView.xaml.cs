using SecureVault.ViewModel;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class ChangePasswordView : UserControl
    {
        private readonly ChangePasswordViewModel _viewModel = new();
        private readonly bool _returnToSettings;

        public ChangePasswordView()
            : this(true)
        {
        }

        public ChangePasswordView(bool returnToSettings)
        {
            InitializeComponent();
            _returnToSettings = returnToSettings;
            DataContext = _viewModel;
            _viewModel.BackRequested += NavigateBack;
        }

        private void NavigateBack()
        {
            _viewModel.BackRequested -= NavigateBack;

            if (this.Parent is ContentControl contentControl)
            {
                if (_returnToSettings)
                {
                    contentControl.Content = new SettingsView();
                    return;
                }

                contentControl.Content = null;

                if (contentControl.Parent is System.Windows.Controls.Border border &&
                    border.Name == "SubViewContainer")
                {
                    border.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
    }
}
