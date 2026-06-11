using Microsoft.EntityFrameworkCore;
using SecureVault.Model.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SecureVault.Model.Services
{
    public static class CategoryStore
    {
        public static ObservableCollection<Category> Categories { get; } = new();

        public static bool Exists(string categoryType)
        {
            return Categories.Any(category => category.CategoryType.Equals(categoryType, System.StringComparison.OrdinalIgnoreCase));
        }

        public static async Task LoadAsync()
        {
            await using var dbContext = await DatabaseService.CreateContextAsync();
            var categories = await dbContext.Categories
                .OrderBy(category => category.CategoryType)
                .ToListAsync();

            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        public static async Task AddAsync(string categoryType)
        {
            var category = new Category
            {
                CategoryType = categoryType.Trim()
            };

            await using var dbContext = await DatabaseService.CreateContextAsync();
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
            Categories.Add(category);
        }

        public static async Task RemoveAsync(Category category)
        {
            await using var dbContext = await DatabaseService.CreateContextAsync();
            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();
            Categories.Remove(category);
        }
    }
}
