using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Winria.Util
{
    public class Settings
    {
        public static ResourceDictionary LanguageResource { get; set; }
        private static string getLanguageName()
        {
            return System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName; 
        }
        public static ResourceDictionary GetLanguage(ResourceDictionary resourceDictionary)
        {
            string languageName = getLanguageName();
            ResourceDictionary resource;
            try
            {
                resource = Application.LoadComponent(new Uri(@"Resources\Language\" + languageName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            }
            catch (Exception)
            {
                resource = Application.LoadComponent(new Uri(@"Resources\Language\enUS.xaml", UriKind.Relative)) as ResourceDictionary;
            }
            if(resourceDictionary.MergedDictionaries.Count == 0)
                resourceDictionary.MergedDictionaries.Add(LanguageResource);
            return resource;
        }
        public static void SetLanguage(string languageName,ResourceDictionary resourceDictionary)
        {
            languageName = getLanguageName();
            if (LanguageResource != null)
            {
                resourceDictionary.MergedDictionaries.Clear();
            }
            try
            {
                LanguageResource = Application.LoadComponent(new Uri(@"Resources\Language\" + languageName + ".xaml", UriKind.Relative)) as ResourceDictionary;
            }
            catch (Exception)
            {
                LanguageResource = Application.LoadComponent(new Uri(@"Resources\Language\enUS.xaml", UriKind.Relative)) as ResourceDictionary;
            }
            resourceDictionary.MergedDictionaries.Add(LanguageResource);
        }
    }
}
