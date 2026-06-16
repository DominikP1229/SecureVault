using SecureVault.Model;
using SecureVault.Model.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace SecureVault.ViewModel
{
    public class CategoriesViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;
        private string _categoryName = string.Empty;
        private Category? _selectedCategory;

        public ObservableCollection<Category> Categories => CategoryStore.Categories;
        public AsyncRelayCommand AddCategoryCommand { get; }
        public AsyncRelayCommand DeleteCategoryCommand { get; }
        public RelayCommand CloseCommand { get; }

        public event Action? CloseRequested;

        public CategoriesViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync);
            CloseCommand = new RelayCommand(() => CloseRequested?.Invoke());
        }

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                _categoryName = value;
                OnPropertyChanged();
            }
        }

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        private async Task AddCategoryAsync()
        {
            var categoryType = CategoryName.Trim();

            if (string.IsNullOrWhiteSpace(categoryType))
            {
                MessageBox.Show("Category name is required.", "Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (categoryType.Length < 2)
            {
                MessageBox.Show("Category name must be at least 2 characters long.", "Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryStore.Exists(categoryType))
            {
                MessageBox.Show("This category already exists.", "Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await CategoryStore.AddAsync(categoryType);
            CategoryName = string.Empty;
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Select a category to delete.", "Categories", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_mainViewModel.Credentials.Any(credential => credential.Category == SelectedCategory.CategoryType))
            {
                MessageBox.Show("Cannot delete a category used by saved passwords.", "Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await CategoryStore.RemoveAsync(SelectedCategory);
            SelectedCategory = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
