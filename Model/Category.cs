using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureVault.Model
{
    public class Category
    {
        public string CategoryType { get; set; } = string.Empty;

        public override string ToString()
        {
            return CategoryType;
        }
    }
}
