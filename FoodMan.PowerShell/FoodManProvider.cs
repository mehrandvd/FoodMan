using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using FoodMan.Core;

namespace FoodMan.PowerShell
{
    [CmdletProvider("FoodProvider", ProviderCapabilities.ExpandWildcards)]
    public class FoodManProvider : NavigationCmdletProvider
    {
        protected List<Restraunt> RootRestraunts { get; set; }

        public FoodManProvider()
        {
            RootRestraunts = FoodSourceManager.Instance.GetRestraunts();
        }

        protected override bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return true;

            var foodSourceInfo = GetFoodSourceInfo(path);

            if (foodSourceInfo != null)
                return true;

            return false;
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            foreach (var exception in FoodSourceManager.Exceptions)
            {
                var error = new ErrorRecord(exception, "Error in loading FoodSource", ErrorCategory.InvalidData, this);
                WriteWarning(error.ToString());
            }

            var drive = new PSDriveInfo("FoodMan", this.ProviderInfo, "", "", null);
            var drives = new Collection<PSDriveInfo>() { drive };
            return drives;
        }

        protected override bool ItemExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return true;

            var foodSourceInfo = GetFoodSourceInfo(path);

            if (foodSourceInfo != null)
                return true;

            return false;
        }

        protected override bool IsItemContainer(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return true;

            var foodSourceInfo = GetFoodSourceInfo(path);

            if (foodSourceInfo != null)
                return foodSourceInfo is Category || foodSourceInfo is Restraunt;

            return false;
        }

        protected override string[] ExpandPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.StartsWith("Set-Location"))
                return null;

            var caretPart = path.Split('\\').Reverse().ToList()[0];
            var initPath = path.Split('\\').Reverse().Skip(1).Reverse().Aggregate("", (a, b) => a + "\\" + b);

            var regexString = Regex.Escape(caretPart).Replace("\\*", ".*");
            regexString = "^" + regexString + "$";
            var regex = new Regex(regexString);

            List<string> candidateChildren;


            List<FoodSourceInfo> children;
            if (string.IsNullOrWhiteSpace(initPath))
            {
                children = RootRestraunts.Cast<FoodSourceInfo>().ToList();

                candidateChildren = (from child in children
                                     where regex.IsMatch(child.Name)
                                     select child.Name).ToList();
            }
            else
            {
                var foodSourceInfo = GetFoodSourceInfo(initPath);
                children = foodSourceInfo.GetChildren();

                candidateChildren = (from child in children
                                     where regex.IsMatch(child.Name)
                                     select initPath + "\\" + child.Name).ToList();
            }

            if (candidateChildren.Any())
                return candidateChildren.ToArray();

            return null;
        }

        private FoodSourceInfo GetFoodSourceInfo(string path)
        {

            var splitted = path.Trim('\\').Split('\\');
            string restrauntPart = string.Empty;
            string categoryPart = string.Empty;
            string foodPart = string.Empty;

            if (splitted.Length > 0)
            {
                restrauntPart = splitted[0];
                var restraunt = RootRestraunts.FirstOrDefault(rest => rest.Name == restrauntPart);

                if (restraunt == null)
                    return null; //throw new Exception(string.Format("Restraunt is not found: '{0}'", restrauntPart));

                if (splitted.Length > 1)
                {
                    categoryPart = splitted[1];
                    var category = restraunt.Categories.FirstOrDefault(c => c.Name == categoryPart);

                    if (category == null)
                        return null;//throw new Exception(string.Format("Category is not found: '{0}\\{1}'", restrauntPart, categoryPart));

                    if (splitted.Length > 2)
                    {
                        foodPart = splitted[2];
                        var food = category.Foods.FirstOrDefault(f => f.Name == foodPart);

                        if (food == null)
                            return null;//throw new Exception(string.Format("Category is not found: '{0}\\{1}\\{3}'", restrauntPart, categoryPart, foodPart));

                        return food;
                    }
                    else
                    {
                        return category;
                    }
                }
                else
                {
                    return restraunt;
                }
            }

            return null;

        }

        protected override void GetChildItems(string path, bool recurse)
        {
            List<FoodSourceInfo> children, allChildren;

            if (string.IsNullOrWhiteSpace(path))
            {
                children = RootRestraunts.Cast<FoodSourceInfo>().ToList();
            }
            else
            {
                var foodsourceInfo = GetFoodSourceInfo(path);

                if (foodsourceInfo is Food)
                {
                    WriteError(
                        new ErrorRecord(
                            new Exception(string.Format("Food has not any child: '{0}'", foodsourceInfo.Name)),
                            "NoChildren", ErrorCategory.InvalidOperation, foodsourceInfo));
                    return;
                }

                children = foodsourceInfo.GetChildren().ToList();
            }

            if (recurse)
            {
                allChildren = children.SelectMany(child => child.GetAllChildren()).ToList();
            }
            else
            {
                allChildren = children;
            }

            foreach (var child in allChildren)
            {
                WriteItemObject(child, string.Format("{0}\\{1}", path, child.Name), true);
            }
        }
    }
}
