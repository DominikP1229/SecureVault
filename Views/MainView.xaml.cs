using SecureVault.Model;
using SecureVault.Model.Services;
using SecureVault.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SecureVault.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
            : this(new MainViewModel())
        {
        }

        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel &&
                this.Parent is Grid parentGrid &&
                parentGrid.Parent is MainWindow mainWindow)
            {
                viewModel.SelectedCredential = null;
                viewModel.ClearForm();
                mainWindow.SwitchView(new AddPasswordView(viewModel));
            }
        }

        private void PasswordGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel { SelectedCredential: not null } viewModel &&
                this.Parent is Grid parentGrid &&
                parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new PasswordDetailsView(viewModel, viewModel.SelectedCredential));
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            if (sender is FrameworkElement { DataContext: Credential credential })
            {
                viewModel.SelectedCredential = credential;
            }

            if (viewModel.SelectedCredential == null)
            {
                MessageBox.Show("Wybierz wpis do edycji.", "Edycja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new AddPasswordView(viewModel, true));
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }

            if (sender is FrameworkElement { DataContext: Credential credential })
            {
                viewModel.SelectedCredential = credential;
            }

            if (viewModel.DeleteCommand.CanExecute(null))
            {
                viewModel.DeleteCommand.Execute(null);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel || viewModel.SelectedCredential == null)
            {
                MessageBox.Show("Wybierz wpis, z którego chcesz skopiować hasło.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrEmpty(viewModel.SelectedCredential.EncryptedPassword))
            {
                MessageBox.Show("Wybrany wpis nie ma zapisanego hasła.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(viewModel.SelectedCredential.EncryptedPassword);
            MessageBox.Show("Hasło skopiowane do schowka.", "Kopiowanie", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                VaultSession.SignOut();
                mainWindow.SwitchView(new LoginView());
            }
        }

        private void OpenCategories(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                SubViewContent.Content = new CategoriesView(viewModel);
            }

            SubViewContainer.Visibility = Visibility.Visible;
        }

        private void OpenHistory(object sender, RoutedEventArgs e)
        {
            SubViewContent.Content = new HistoryView();
            SubViewContainer.Visibility = Visibility.Visible;
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SubViewContent.Content = new SettingsView();
            SubViewContainer.Visibility = Visibility.Visible;
        }

        public void CloseSubView()
        {
            SubViewContainer.Visibility = Visibility.Collapsed;
            SubViewContent.Content = null;
        }
    }
}
