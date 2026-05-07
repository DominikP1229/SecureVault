using SecureVault.ViewModel;
using SecureVault.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SecureVault.Views
{
    /// <summary>
    /// Logika interakcji dla klasy MainView.xaml
    /// </summary>
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
                mainWindow.SwitchView(new AddPasswordView(viewModel));
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && viewModel.EditCommand.CanExecute(null))
            {
                viewModel.EditCommand.Execute(null);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && viewModel.DeleteCommand.CanExecute(null))
            {
                viewModel.DeleteCommand.Execute(null);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Grid parentGrid && parentGrid.Parent is MainWindow mainWindow)
            {
                mainWindow.SwitchView(new LoginView());
            }
        }

        private void OpenCategories(object sender, RoutedEventArgs e)
        {
            SubViewContent.Content = new CategoriesView();
            SubViewContainer.Visibility = Visibility.Visible;
        }

        private void OpenHistory(object sender, RoutedEventArgs e)
        {
            SubViewContent.Content = new HistoryView();
            SubViewContainer.Visibility = Visibility.Visible;
        }

        public void CloseSubView()
        {
            SubViewContainer.Visibility = Visibility.Collapsed;
            SubViewContent.Content = null;
        }
    }
    }
