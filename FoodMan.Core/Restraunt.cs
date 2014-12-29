using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodMan.Core
{
    public class Restraunt : FoodSourceInfo
    {
        public Restraunt()
        {
            _categories = new Lazy<List<Category>>(() =>
                                                   {
                                                       var categories = FoodSource.GetCategories(this);
                                                       categories.ForEach(c =>
                                                                          {
                                                                              c.FoodSource = FoodSource;
                                                                              c.Restraunt = this;
                                                                          });
                                                       return categories;
                                                   });
        }

        private readonly Lazy<List<Category>> _categories;

        public List<Category> Categories
        {
            get { return _categories.Value; }
        }

        public override List<FoodSourceInfo> GetChildren()
        {
            return Categories.Cast<FoodSourceInfo>().ToList();
        }
    }
}
