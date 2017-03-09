using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.Parsing
{
    public static class Ints
    {

        public static int ParseStorage(string value)
        {
            return int.Parse(value, System.Globalization.NumberStyles.None);
        }

    }
}
