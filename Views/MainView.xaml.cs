using SecureVault.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class MainView : UserControl
    {
        private readonly MainViewModel _viewModel;

        public MainView()
            : this(new MainViewModel())
        {
        }

        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.NavigationRequested += HandleNavigationRequested;
            _ = _viewModel.LoadCredentialsAsync();
        }

        private void PasswordGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.OpenDetailsCommand.CanExecute(null))
            {
                _viewModel.OpenDetailsCommand.Execute(null);
            }
        }

        private void HandleNavigationRequested(MainNavigationTarget target)
        {
            switch (target)
            {
                case MainNavigationTarget.Main:
                    if (SubViewContainer.Visibility == System.Windows.Visibility.Visible)
                    {
                        CloseOverlay();
                    }
                    else
                    {
                        SwitchRoot(new MainView(_viewModel));
                    }
                    break;
                case MainNavigationTarget.Login:
                    SwitchRoot(new LoginView());
                    break;
                case MainNavigationTarget.CredentialForm:
                    ShowOverlay(new AddPasswordView(_viewModel));
                    break;
                case MainNavigationTarget.CredentialDetails:
                    if (_viewModel.SelectedCredential != null)
                    {
                        ShowOverlay(new PasswordDetailsView(_viewModel, _viewModel.SelectedCredential));
                    }
                    break;
                case MainNavigationTarget.History:
                    ShowOverlay(new HistoryView());
                    break;
                case MainNavigationTarget.Categories:
                    ShowOverlay(new CategoriesView(_viewModel));
                    break;
                case MainNavigationTarget.Settings:
                    ShowOverlay(new SettingsView());
                    break;
            }
        }

        private void SwitchRoot(System.Windows.UIElement view)
        {
            _viewModel.NavigationRequested -= HandleNavigationRequested;

            if (Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(view);
            }
        }

        private void ShowOverlay(UserControl view)
        {
            SubViewContent.Content = view;
            SubViewContainer.Visibility = System.Windows.Visibility.Visible;
        }

        private void CloseOverlay()
        {
            SubViewContent.Content = null;
            SubViewContainer.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
