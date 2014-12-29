using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodMan.Core
{
    public class Food : FoodSourceInfo
    {
        public string Title { get; set; }
        public int Price { get; set; }
    }
}
