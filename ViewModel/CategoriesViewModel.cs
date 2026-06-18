using SecureVault.Model;
using SecureVault.Model.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
                await NotificationService.ShowWarningAsync("Categories", "Category name is required.");
                return;
            }

            if (categoryType.Length < 2)
            {
                await NotificationService.ShowWarningAsync("Categories", "Category name must be at least 2 characters long.");
                return;
            }

            if (CategoryStore.Exists(categoryType))
            {
                await NotificationService.ShowWarningAsync("Categories", "This category already exists.");
                return;
            }

            await CategoryStore.AddAsync(categoryType);
            CategoryName = string.Empty;
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null)
            {
                await NotificationService.ShowInformationAsync("Categories", "Select a category to delete.");
                return;
            }

            if (_mainViewModel.Credentials.Any(credential => credential.Category == SelectedCategory.CategoryType))
            {
                await NotificationService.ShowWarningAsync("Categories", "Cannot delete a category used by saved passwords.");
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
