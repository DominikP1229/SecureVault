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
using SecureVault.Model;
using SecureVault.Model.Services;
using SecureVault.ViewModel;

namespace SecureVault.Views
{
    /// <summary>
    /// Logika interakcji dla klasy CategoriesView.xaml
    /// </summary>
    public partial class CategoriesView : UserControl
    {
        private readonly MainViewModel _viewModel;

        public CategoriesView()
            : this(new MainViewModel())
        {
        }

        public CategoriesView(MainViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var categoryType = CategoryNameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(categoryType))
            {
                MessageBox.Show("Nazwa kategorii jest wymagana.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (categoryType.Length < 2)
            {
                MessageBox.Show("Nazwa kategorii musi mieć co najmniej 2 znaki.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryStore.Exists(categoryType))
            {
                MessageBox.Show("Taka kategoria już istnieje.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CategoryStore.Add(categoryType);
            CategoryNameBox.Clear();
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryListBox.SelectedItem is not Category category)
            {
                MessageBox.Show("Wybierz kategorię do usunięcia.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_viewModel.Credentials.Any(credential => credential.Category == category.CategoryType))
            {
                MessageBox.Show("Nie można usunąć kategorii używanej przez zapisane hasła.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CategoryStore.Remove(category);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var parent = this.Parent as FrameworkElement;

            while (parent != null && parent.Name != "SubViewContainer")
            {
                parent = parent.Parent as FrameworkElement;
            }

            if (parent != null)
            {
                parent.Visibility = Visibility.Collapsed;

                if (parent is Border border && border.Child is ContentControl content)
                {
                    content.Content = null;
                }
            }
        }
    }
}
