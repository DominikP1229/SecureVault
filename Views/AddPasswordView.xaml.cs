using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class AddPasswordView : UserControl
    {
        private readonly MainViewModel _viewModel;

        public AddPasswordView()
            : this(new MainViewModel())
        {
        }

        public AddPasswordView(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.NavigationRequested += HandleNavigationRequested;
        }

        public AddPasswordView(MainViewModel viewModel, bool isEditMode)
            : this(viewModel)
        {
            _viewModel.IsEditMode = isEditMode;
        }

        private void HandleNavigationRequested(MainNavigationTarget target)
        {
            switch (target)
            {
                case MainNavigationTarget.Main:
                    CloseOverlayOrSwitchRoot(new MainView(_viewModel));
                    break;
                case MainNavigationTarget.Login:
                    SwitchRoot(new LoginView());
                    break;
                case MainNavigationTarget.CredentialDetails:
                    if (_viewModel.SelectedCredential != null)
                    {
                        SwitchRoot(new PasswordDetailsView(_viewModel, _viewModel.SelectedCredential));
                    }
                    break;
                case MainNavigationTarget.CredentialForm:
                    break;
                case MainNavigationTarget.History:
                case MainNavigationTarget.Categories:
                case MainNavigationTarget.Settings:
                case MainNavigationTarget.ChangePassword:
                    break;
            }
        }

        private void SwitchRoot(UIElement view)
        {
            _viewModel.NavigationRequested -= HandleNavigationRequested;

            if (Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(view);
            }
        }

        private void CloseOverlayOrSwitchRoot(UIElement fallbackView)
        {
            _viewModel.NavigationRequested -= HandleNavigationRequested;

            if (Parent is ContentControl contentControl &&
                contentControl.Parent is Border border &&
                border.Name == "SubViewContainer")
            {
                contentControl.Content = null;
                border.Visibility = Visibility.Collapsed;
                return;
            }

            if (Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(fallbackView);
            }
        }
    }
}
