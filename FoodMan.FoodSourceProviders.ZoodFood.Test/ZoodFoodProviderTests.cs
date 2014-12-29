using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FoodMan.FoodSourceProviders.ZoodFood.Test
{
    [TestClass]
    public class ZoodFoodProviderTests
    {
        [TestMethod]
        public void GetRestrauntMustWork()
        {
            var zood = new ZoodFoodFoodSource();
            zood.Initialize(null);
            var rests = zood.GetRestraunts();
        }
    }
}
