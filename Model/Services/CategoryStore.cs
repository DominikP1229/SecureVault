using SecureVault.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace SecureVault.Model.Services
{
    public static class CategoryStore
    {
        public static ObservableCollection<Category> Categories { get; } = new()
        {
            new Category { CategoryType = "Social" },
            new Category { CategoryType = "Work" },
            new Category { CategoryType = "Finance" }
        };

        public static bool Exists(string categoryType)
        {
            return Categories.Any(category => category.CategoryType == categoryType);
        }

        public static void Add(string categoryType)
        {
            Categories.Add(new Category
            {
                CategoryType = categoryType
            });
        }
    }
}
