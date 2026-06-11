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
                MessageBox.Show("Nazwa kategorii jest wymagana.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (categoryType.Length < 2)
            {
                MessageBox.Show("Nazwa kategorii musi miec co najmniej 2 znaki.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryStore.Exists(categoryType))
            {
                MessageBox.Show("Taka kategoria juz istnieje.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await CategoryStore.AddAsync(categoryType);
            CategoryName = string.Empty;
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Wybierz kategorie do usuniecia.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_mainViewModel.Credentials.Any(credential => credential.Category == SelectedCategory.CategoryType))
            {
                MessageBox.Show("Nie mozna usunac kategorii uzywanej przez zapisane hasla.", "Kategorie", MessageBoxButton.OK, MessageBoxImage.Warning);
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
