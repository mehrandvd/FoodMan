using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FoodMan.Core;

namespace FoodMan.PowerShell
{
    public class FoodSourceManager
    {
        [ImportMany(typeof(IFoodSource))]
        public List<IFoodSource> FoodSourcePlugins { get; set; }

        public static List<Exception> Exceptions = new List<Exception>(); 

        static FoodSourceManager()
        {
            _instance = new Lazy<FoodSourceManager>(CreateFoodSourceManager);
        }

        private static FoodSourceManager CreateFoodSourceManager()
        {
            var toRemove = new List<IFoodSource>();
            var manager = new FoodSourceManager();
            manager.FoodSourcePlugins.ForEach(p =>
                                              {
                                                  try
                                                  {
                                                      p.Initialize(null);
                                                  }
                                                  catch (Exception exception)
                                                  {
                                                      toRemove.Add(p);
                                                      Exceptions.Add(exception);
                                                  }
                                              });

            foreach (var p in toRemove)
            {
                manager.FoodSourcePlugins.Remove(p);
            }

            return manager;
        }

        private FoodSourceManager()
        {

            var aggregateCatalog = new AggregateCatalog();

            var directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directoryCatalog = new DirectoryCatalog(directoryPath, "*.dll");

            var asmCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            aggregateCatalog.Catalogs.Add(directoryCatalog);
            aggregateCatalog.Catalogs.Add(asmCatalog);
            var container = new CompositionContainer(aggregateCatalog);

            container.ComposeParts(this);
        }

        private static readonly Lazy<FoodSourceManager> _instance;

        public static FoodSourceManager Instance
        {
            get { return _instance.Value; }
        }

        public List<Restraunt> GetRestraunts()
        {
            var allRestraunts = new List<Restraunt>();

            foreach (var plugin in FoodSourcePlugins)
            {
                var pluginRestraunts = plugin.GetRestraunts();
                pluginRestraunts.ForEach(rest => rest.FoodSource = plugin);
                allRestraunts.AddRange(pluginRestraunts);
            }

            return allRestraunts;
        }
    }
}
