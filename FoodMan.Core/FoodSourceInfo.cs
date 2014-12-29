using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodMan.Core
{
    public class FoodSourceInfo
    {
        public IFoodSource FoodSource { get; set; }
        public string Name { get; set; }

        public virtual List<FoodSourceInfo> GetChildren()
        {
            //throw new Exception("GetChildred must be overrided but it's not.");
            return null;
        }

        public List<FoodSourceInfo> GetAllChildren()
        {
            var list = new List<FoodSourceInfo>();

            var children = GetChildren();
            
            if (children == null)
                return new List<FoodSourceInfo>();

            list.AddRange(children);
            
            foreach (var child in children)
            {
                list.AddRange(child.GetAllChildren());
            }

            return list;
        }
    }
}
