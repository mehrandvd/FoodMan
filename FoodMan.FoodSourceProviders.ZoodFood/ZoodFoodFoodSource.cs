using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FoodMan.Core;

namespace FoodMan.FoodSourceProviders.ZoodFood
{
    [Export(typeof(IFoodSource))]
    public class ZoodFoodFoodSource : IFoodSource
    {
        private List<Restraunt> _restraunts;
        public bool IsInitialized { get; set; }
        private Dictionary<Restraunt, List<Category>> _categoryDictionary = new Dictionary<Restraunt, List<Category>>(); 
        private Dictionary<Category, List<Food>> _foodDictionary = new Dictionary<Category, List<Food>>();

        private List<KeyValuePair<Restraunt, string>> GetRestaurantLinks()
        {
            var x = ConfigurationManager.AppSettings;

            var map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = Assembly.GetExecutingAssembly().Location + ".config";
            Configuration libConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            var section = (libConfig.GetSection("appSettings") as AppSettingsSection);

            var result = new List<KeyValuePair<Restraunt, string>>();
            foreach (var setting in section.Settings)
            {
                var keypair = (KeyValueConfigurationElement)setting;
                var restaurant = new Restraunt() { Name = keypair.Key };
                
                var restKeyPair = new KeyValuePair<Restraunt, string>(restaurant, keypair.Value);
                result.Add(restKeyPair);
            }
            
            return result;
        }

        private void Init()
        {
            try
            {
                IsInitialized = false;

                if (_restraunts != null)
                    return;


                _restraunts = new List<Restraunt>();

                var resraurantLinks = GetRestaurantLinks();

                foreach (var restaurantLink in resraurantLinks)
                {
                    var restraunt = restaurantLink.Key;
                    _restraunts.Add(restraunt);

                    using (var client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        string htmlCode =
                            client.DownloadString(restaurantLink.Value);

                        var html = new HtmlAgilityPack.HtmlDocument();
                        html.LoadHtml(htmlCode);

                        var menu = html.GetElementbyId("resMenuCon");

                        var categoryNodes = menu.Descendants("div").Where(n => n.Id.StartsWith("sec-")).ToList();

                        var categories = new List<Category>();
                        _categoryDictionary[restraunt] = categories;

                        foreach (var categoryNode in categoryNodes)
                        {
                            var category = new Category() {Name = categoryNode.InnerText.Trim()};
                            categories.Add(category);

                            var foodNodes = categoryNode
                                .NextSibling
                                .NextSibling
                                .Descendants("div")
                                .Where(n =>
                                       {
                                           var att = n.Attributes
                                               .FirstOrDefault
                                               (a => a.Name == "class");

                                           if (att == null)
                                               return false;

                                           if (att.Value == "food")
                                               return true;

                                           return false;
                                       });

                            _foodDictionary[category] = new List<Food>();
                            foreach (var foodNode in foodNodes)
                            {
                                var food = new Food();

                                food.Name = foodNode.Descendants("h4").FirstOrDefault().InnerText.Trim();
                                food.Price =
                                    int.Parse(
                                        foodNode.Descendants("span")
                                            .FirstOrDefault()
                                            .InnerText.Replace("تومان", "")
                                            .Trim(' '));

                                _foodDictionary[category].Add(food);
                            }
                        }
                    }
                }

                
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in importing ZoodFoodProvider restraunts.", ex);
            }
        }

        public void Initialize(string key)
        {
            Init();
        }

        public List<Restraunt> GetRestraunts()
        {
            return _restraunts;
        }

        public List<Category> GetCategories(Restraunt restraunt)
        {
            return _categoryDictionary[restraunt];
        }

        public List<Food> GetFoods(Category category)
        {
            return _foodDictionary[category];
        }
    }
}
