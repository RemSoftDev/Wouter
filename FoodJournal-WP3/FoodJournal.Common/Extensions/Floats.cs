using FoodJournal.AppModel;
using FoodJournal.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace FoodJournal.Parsing
{
    public static class Floats
    {

        private const int ROUNDDIGITS = 1;

        private static Dictionary<float, string> lookup;

        public static float ParseStorage(string value)
        {
            if (value == null || value.Length == 0) return float.NaN;
            return float.Parse(value, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static string ToStorageString(this float value)
        {
            return value.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        public static float ParseUnknown(string value, float invalid)
        {
            if (AppStats.CultureCommaNotADot)
                return ParseUI(value.Replace(".", AppStats.CultureComma), invalid);
            return ParseUI(value, invalid);
        }

        public static float ParseUI(string value, float invalid)
        {
            if (value == null) return 0;
            int pos = value.IndexOf('/');
            if (pos > 0) 
            {
                float val1 = 0; float val2 = 0;
                if (!float.TryParse(value.Substring(0, pos), out val1)) return invalid;
                if (!float.TryParse(value.Substring(pos + 1), out val2)) return invalid;
                return val1 / val2;
            }
            float result = 0;
			
			if (!float.TryParse(value, NumberStyles.Any,Thread.CurrentThread.CurrentUICulture, out result)) return invalid;
            return result;
        }

        private static void BuildLookup()
        {
            lookup = new Dictionary<float, string>();
            for (int x = 2; x < 11; x++)
                for (float y = 1; y < x; y++)
                    if (!lookup.ContainsKey(y / x))
                        lookup.Add(y / x, string.Format("{0}/{1}", y, x));
        }

        public static string ToUIString(this float value)
        {

            if (float.IsNaN(value)) return "";

            if (lookup == null) BuildLookup();
            if (lookup.ContainsKey(value)) return lookup[value];

            float value2 = (float)Math.Round((double)value, value > 10 ? 0 : 1);

            if (value > 10)
                return value2.ToString("#,##0");
            else
                return value2.ToString("#,##0.##");
        }

    }
}
