using SecureVault.Model.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureVault.Model.Services
{
    public static class CategoryStore
    {
        public static ObservableCollection<Category> Categories { get; } = new();

        static CategoryStore()
        {
            Load();
        }

        public static bool Exists(string categoryType)
        {
            return Categories.Any(category => category.CategoryType.Equals(categoryType, System.StringComparison.OrdinalIgnoreCase));
        }

        public static void Add(string categoryType)
        {
            var category = new Category
            {
                CategoryType = categoryType.Trim()
            };

            using var dbContext = DatabaseService.CreateContext();
            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            Categories.Add(category);
        }

        public static void Remove(Category category)
        {
            using var dbContext = DatabaseService.CreateContext();
            dbContext.Categories.Remove(category);
            dbContext.SaveChanges();
            Categories.Remove(category);
        }

        private static void Load()
        {
            using var dbContext = DatabaseService.CreateContext();
            Categories.Clear();

            foreach (var category in dbContext.Categories.OrderBy(category => category.CategoryType))
            {
                Categories.Add(category);
            }
        }
    }
}
