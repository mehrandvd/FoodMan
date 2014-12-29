using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FoodMan.Core
{
    public interface IFoodSource
    {
        bool IsInitialized { get; set; }
        void Initialize(string key);

        List<Restraunt> GetRestraunts();

        List<Category> GetCategories(Restraunt restraunt);

        List<Food> GetFoods(Category category);
    }
}
