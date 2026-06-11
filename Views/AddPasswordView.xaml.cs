using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private void AddPasswordView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _viewModel.CancelCredentialFormCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _viewModel.SaveCredentialCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void HandleNavigationRequested(MainNavigationTarget target)
        {
            switch (target)
            {
                case MainNavigationTarget.Main:
                    SwitchRoot(new MainView(_viewModel));
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
    }
}
