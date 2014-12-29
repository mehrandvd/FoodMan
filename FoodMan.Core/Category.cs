using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoodMan.Core
{
    public class Category : FoodSourceInfo
    {
        public Category()
        {
            _foods = new Lazy<List<Food>>(() =>
                                          {
                                              var foods = FoodSource.GetFoods(this);
                                              foods.ForEach(f=>f.FoodSource = FoodSource);
                                              return foods;
                                          });
        }

        private readonly Lazy<List<Food>> _foods;

        public List<Food> Foods
        {
            get { return _foods.Value; }
        }

        public Restraunt Restraunt { get; set; }

        public override List<FoodSourceInfo> GetChildren()
        {
            return Foods.Cast<FoodSourceInfo>().ToList();
        }
    }
}
