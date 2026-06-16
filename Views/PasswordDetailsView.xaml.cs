using SecureVault.Model;
using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class PasswordDetailsView : UserControl
    {
        private readonly MainViewModel _mainViewModel;
        private readonly PasswordDetailsViewModel _viewModel;

        public PasswordDetailsView(MainViewModel mainViewModel, Credential credential)
        {
            InitializeComponent();
            _mainViewModel = mainViewModel;
            _viewModel = new PasswordDetailsViewModel(mainViewModel, credential);
            DataContext = _viewModel;
            _viewModel.NavigationRequested += HandleNavigationRequested;
        }

        private void PasswordDetailsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _viewModel.BackCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void HandleNavigationRequested(PasswordDetailsNavigationTarget target)
        {
            _viewModel.NavigationRequested -= HandleNavigationRequested;

            if (target == PasswordDetailsNavigationTarget.Edit)
            {
                ShowInOverlayOrSwitchTo(new AddPasswordView(_mainViewModel, true));
                return;
            }

            CloseOverlayOrSwitchTo(new MainView(_mainViewModel));
        }

        private void SwitchTo(UIElement view)
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(view);
            }
        }

        private void ShowInOverlayOrSwitchTo(UIElement view)
        {
            if (Parent is ContentControl contentControl)
            {
                contentControl.Content = view;
                return;
            }

            SwitchTo(view);
        }

        private void CloseOverlayOrSwitchTo(UIElement fallbackView)
        {
            if (Parent is ContentControl contentControl &&
                contentControl.Parent is Border border &&
                border.Name == "SubViewContainer")
            {
                contentControl.Content = null;
                border.Visibility = Visibility.Collapsed;
                return;
            }

            SwitchTo(fallbackView);
        }
    }
}
