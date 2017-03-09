using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoodJournal.Resources;

namespace FoodJournal.WinPhone.Common.Resources
{


    public static partial class Strings
    {

        public static string FromEnum(Enum value)
        {
#if ANDROID
            return AppResources.FromEnum(value.GetType().Name + "_" + value.ToString());
#else
            return AppResources.ResourceManager.GetString(value.GetType().Name + "_" + value);
#endif
        }

        public static string FromEnum(string EnumName, string Value)
        {
#if ANDROID
            return AppResources.FromEnum(EnumName + "_" + Value);
#else
            return AppResources.ResourceManager.GetString(EnumName + "_" + Value);
#endif
        }

        private static bool EndsWith(string word, string[] endings)
        {
            foreach (var end in endings)
                if (word.EndsWith(end, StringComparison.InvariantCultureIgnoreCase)) return true;
            return false;
        }

        private static bool IsPlural(string word)
        {
            return word.EndsWith("s");
        }

        private static string[] esExt = new string[] { "s", "z", "x", "sh", "ch" };
        private static string[] aysExt = new string[] { "ay", "ey", "iy", "oy", "uy" };
        private static string[] osExt = new string[] { "ao", "eo", "io", "oo", "uo" };
        public static string GetPlural(string word)
        {
            return word;
        }

    }
}
