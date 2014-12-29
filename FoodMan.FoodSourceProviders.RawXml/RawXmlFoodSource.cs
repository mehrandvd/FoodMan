using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FoodMan.Core;

namespace FoodMan.FoodSourceProviders.RawXml
{
    [Export(typeof(IFoodSource))]
    public class RawXmlFoodSource : IFoodSource
    {
        private XElement _rootXml;
        public bool IsInitialized { get; set; }
        public void Initialize(string key)
        {
            // ToDo: I should read the file names from the config.
            IsInitialized = false;
            _rootXml = XElement.Load(@"C:\Users\Mehran\Source\Workspaces\CodePlex\foodman\FoodMan\FoodMan.PowerShell.Test\bin\Debug\SampleFoodSource.xml");
            IsInitialized = true;
        }

        public List<Restraunt> GetRestraunts()
        {
            var restraunts = _rootXml.DescendantsAndSelf("Restraunt");

            var result = from rest in restraunts
                select new Restraunt {Name = rest.Attribute("Name").Value};

            return result.ToList();
        }

        public List<Category> GetCategories(Restraunt restraunt)
        {
            var restrauntNode = _rootXml.Elements("Restraunt").FirstOrDefault(el => el.Attribute("Name").Value == restraunt.Name);

            if (restrauntNode == null)
                throw new Exception(string.Format("There's no restraunt: '{0}' in the Xml.", restraunt.Name));

            var result = from category in restrauntNode.Elements("Category")
                         select new Category() { Name = category.Attribute("Name").Value };

            return result.ToList();
        }

        public List<Food> GetFoods(Category category)
        {
            var restrauntName = category.Restraunt.Name;
            var restrauntNode = _rootXml.Elements("Restraunt").FirstOrDefault(el => el.Attribute("Name").Value == restrauntName);
            
            if (restrauntNode == null)
                throw new Exception(string.Format("There's no restraunt: '{0}' in the Xml.", restrauntName));

            var categoryNode = restrauntNode.Descendants("Category").FirstOrDefault(el => el.Attribute("Name").Value == category.Name);

            if (categoryNode == null)
                throw new Exception(string.Format("There's no category: '{0}' in the Xml.", category.Name));

            var result = from food in categoryNode.Elements("Food")
                         select new Food() { Name = food.Attribute("Name").Value, Price = int.Parse(food.Attribute("Price").Value) };

            return result.ToList();
        }
    }
}
