using FoodJournal.Parsing;
using FoodJournal.WinPhone.Common.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FoodJournal.Resources;
using FoodJournal.Logging;

namespace FoodJournal.Values
{

    public struct Amount
    {

        //    private Single value;
        //    private Unit unit;
        //    private String qualifier;

        //}

        //public struct Amount_Old
        //{

        // kinds: weight, volume, shape, property, time
        // format: [nr] [amount text] (qualifier)
        // uses: db storage, conversion, aggregation, display
        // translated

        // sample amounts:
        // 1 cup, 3 cups, .5 cup
        // 28 gr, 1 liter, 150 ml
        // 2 oz, 15 fl oz, 16 ounces
        // 1 wine glass
        // 25 cookies
        // 1 serving
        // 1 medium potato
        // 12 leaves (dried)
        // 1 small
        // 1.2 g saturated fat
        // 1200 calories
        // 2400 mg sodium
        // 12% of calories
        // 1 hour

        // getfactor (amount1, amount2)
        // amount1 / amount2 ; 
        // ie 2 cups / 5 tbsp = ... (single)
        // 1 cup / 15 ml = ...
        // 1 cup / 28 gr = ... can only be performed when there is a reference volume-to-weigth factor known (tbd)

        public static Amount Zero = new Amount(0, "None");
        public static Amount Empty = new Amount(0, "");
        public static Amount DefaultAmount { get { return "100 g"; } }

        private static List<string> ShapeDescriptions = new List<string>() { "small", "extra small", "medium", "large", "extra large", "big", 
                                                                             "mini", "jumbo", "regular", "round", "square", "rectangular", "miniature"};

        private Single value;

        public readonly Boolean IsStandardUnit;
        private Unit stdUnit;
        public Boolean IsProperty;
        private string nonStdUnit;

        private Amount(Single value, Unit stdUnit) : this(value, true, stdUnit, null) { }
        private Amount(Single value, string nonStdUnit) : this(value, false, null, nonStdUnit) { }
        private Amount(Single value, Boolean isStandardUnit, Unit stdUnit, string nonStdUnit) { this.value = value; this.IsStandardUnit = isStandardUnit; this.stdUnit = stdUnit; this.nonStdUnit = nonStdUnit; this.IsProperty = false; }

        public bool IsValid { get { return (!IsZero) && (IsStandardUnit || (nonStdUnit != null && nonStdUnit.Length > 0 && !char.IsDigit(nonStdUnit[0]))); } }

        public Unit GetStandardUnit() { return IsStandardUnit ? stdUnit : Unit.Parse("g"); }

        public bool IsZero { get { return (value == 0); } }
        public bool IsAlmostZero { get { return (value <= 0.5); } }
        public bool IsConvertable(Amount amt2) { return (this / amt2) != 0; }
        public bool IsSameUnit(Amount amt2) { return (nonStdUnit == amt2.nonStdUnit) && (stdUnit == amt2.stdUnit); }

        public static implicit operator Amount(string value)
        {
            return Parse(value);
        }

        public static bool operator ==(Amount x, Amount y)
        {
            if (x.IsStandardUnit && y.IsStandardUnit) return (x / y) == 1;
            return (x.value == y.value) && (x.nonStdUnit == y.nonStdUnit) && (x.stdUnit == y.stdUnit);
            //return x.ToString() == y.ToString();
        }

        public static bool operator !=(Amount x, Amount y) { return !(x == y); }

        public static bool operator <(Amount x, Amount y)
        {
            if (x == Amount.Zero) return true;
            if (y == Amount.Zero) return false;
            if (x.IsSameUnit(y)) return x.value < y.value;
            return (x / y) < 1;
        }

        public static bool operator >(Amount x, Amount y)
        {
            if (x == Amount.Zero) return false;
            if (y == Amount.Zero) return true;
            if (x.IsSameUnit(y)) return x.value > y.value;
            return (x / y) > 1;
        }

        public override bool Equals(object obj)
        {
            return this == (Amount)obj;
        }

        public override int GetHashCode()
        {
            return ToString(false).GetHashCode();
        }

        public static explicit operator string(Amount value)
        {
            return value.ToString(false);
        }

        public static Amount operator +(Amount value1, Amount value2)
        {
            if (value1.value == 0) return value2;
            if (value2.value == 0) return value1;
            // optimization for + of same unit, this is used a lot to calculate property totals
            if (!value1.IsStandardUnit && value1.nonStdUnit == value2.nonStdUnit)
                return new Amount(value1.value + value2.value, value1.IsStandardUnit, value1.stdUnit, value1.nonStdUnit);
            Single div = value2 / value1;
            return new Amount(value1.value * (1 + div), value1.IsStandardUnit, value1.stdUnit, value1.nonStdUnit);
        }

        public static Amount operator -(Amount value1, Amount value2)
        {
            if (value1.value == 0) return value2;
            if (value2.value == 0) return value1;
            Single div = value2 / value1;
            return new Amount(value1.value * (1 - div), value1.IsStandardUnit, value1.stdUnit, value1.nonStdUnit);
        }

        public static Amount operator *(Amount value, Single multiplier)
        {
            if (value.value == 0) return Amount.Zero;
            return new Amount(value.value * multiplier, value.IsStandardUnit, value.stdUnit, value.nonStdUnit);
        }

        public static Amount operator *(Amount value, double multiplier)
        {
            if (value.value == 0) return Amount.Zero;
            return new Amount((Single)(value.value * multiplier), value.IsStandardUnit, value.stdUnit, value.nonStdUnit);
        }

        public static Single operator /(Amount value1, Amount value2)
        {
            if (!value1.IsStandardUnit || !value2.IsStandardUnit)
            {
                if (value1.nonStdUnit == value2.nonStdUnit)
                    return (value1.value / value2.value);
                if (value1.nonStdUnit == Strings.GetPlural(value2.nonStdUnit)) // todo: perf improve?
                    return (value1.value / value2.value);
                if (value2.nonStdUnit == Strings.GetPlural(value1.nonStdUnit)) // todo: perf improve?
                    return (value1.value / value2.value);
                return 0;
            }
            return (value1.stdUnit / value2.stdUnit) * (value1.value / value2.value);
        }

        public static Amount FromGram(Single gram)
        {
            return new Amount(gram, Unit.Gram);
        }

        public static Amount FromProperty(Single value, Property property)
        {
            var x = new Amount(value, false, null, property.Extension);
            x.IsProperty = true;
            return x;
        }

        public static Amount ParseEquivalent(string text)
        {
			try{
	            if (string.IsNullOrEmpty(text)) return Amount.Zero;
	            if (text.EndsWith("g") || text.EndsWith("ml")) return Parse(text);
	            if (char.IsNumber(text, text.Length - 1)) return FromGram(Floats.ParseStorage(text));
			} catch (Exception ex) {
				LittleWatson.ReportException (ex, text);
			}
            return Parse(text);
        }

        public static Amount Parse(string text)
        {
            if (text == null) return Amount.Zero;
            Single value = 1;

            // see if text starts with a single value
            int pos = text.IndexOf(" ");

            if (pos < 0) 
            {
                if (text.EndsWith("g"))
                    if (text.Length > 1)
                        if (char.IsDigit(text[text.Length - 2]))
                        {
                            text = text.Replace("g", " g");
                            pos = text.IndexOf(" ");
                        }

                if (text.EndsWith("ml"))
                    if (text.Length > 2)
                        if (char.IsDigit(text[text.Length - 3]))
                        {
                            text = text.Replace("ml", " ml");
                            pos = text.IndexOf(" ");
                        }

            }

            if (pos > 0)
            {
                value = Floats.ParseUnknown(text.Substring(0, pos), -989898);
                if (value == -989898) value = 1; else text = text.Substring(pos + 1);
            }

            //string SubAmount = null;
            pos = text.IndexOf("(");
            if (pos > -1)
                if (text.LastIndexOf(")") < pos)
                    text = text.Replace("(", "").Replace(")", "");
            //else
            //{
            //    var ext = text.Substring(pos, text.LastIndexOf(")") - pos + 1);
            //    text = text.Replace(ext, "").Trim();
            //    SubAmount = ext.Replace("(", "").Replace(")", "");
            //    if (SubAmount.Length == 0) SubAmount = null;
            //    if (((Amount)SubAmount).ToString() == "1 ")
            //    {
            //        //System.Diagnostics.Debug.WriteLine("Invalid SubAmount: " + SubAmount);
            //        SubAmount = null;
            //    }
            //}

            // text now contains only unit, see if its standard, if not, assign to nonStd
            Unit stdUnit = Unit.Parse(text);
            if (stdUnit == null) return new Amount(value, text);

            return new Amount(value, stdUnit);
        }

        public string ToStorageString()
        {
            return ToString(false).Replace("=", "").Replace("|", "");
        }

        public string ToString(bool round)
        {
            if (IsZero) return "";
            //if (value == 0) return None.nonStdUnit;
            if (IsStandardUnit)
                return stdUnit.ToString(value, round);
            var amt = nonStdUnit;
            if (!IsShapeDescription && !IsProperty && value > 1) amt = Strings.GetPlural(amt);

            if (round)
                return string.Format("{0} {1}", Floats.ToUIString(value), amt);
            else
                return string.Format("{0} {1}", Floats.ToStorageString(value), amt);
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public Single ToSingle()
        {
            return value;
        }

        public string ValueString()
        {
            if (value != 0)
                return Floats.ToUIString(value);
            return string.Empty;
        }

        private bool IsShapeDescription { get { return ShapeDescriptions.Contains(nonStdUnit); } }

        public string AppendItemText(string itemText)
        {
            if (AppResources.DatabaseCulture.ToLower() == "en")
                return AppendItemTextEN(itemText);

            string part1 = ToString(true);
            string ext = "";

            int pos = part1.IndexOf("(");
            if (pos > -1)
            {
                ext = part1.Substring(pos - 1, part1.LastIndexOf(")") - pos + 2); // includes ()
                part1 = part1.Replace(ext, "");
            }

            if (part1.Contains(itemText)) return part1 + ext;

            return string.Format("{0} {1}{2}", part1, itemText, ext);

        }

        public string AppendItemTextEN(string itemText)
        {

            string part1 = ToString(true);
            string ext = "";
            //string result;

            int pos = part1.IndexOf("(");
            if (pos > -1)
            {
                ext = part1.Substring(pos - 1, part1.LastIndexOf(")") - pos + 2); // includes ()
                part1 = part1.Replace(ext, "");
            }

            if (part1.Contains("ITEM")) part1 = part1.Replace("ITEM", itemText);

            if (part1.Contains("can")) itemText = itemText.Replace("canned ", "");

            if ((part1 != "can") && (part1 != "oz"))
                if (itemText.Contains(part1)) return ValueString() + " " + itemText + ext;

            if (part1.Contains(itemText)) return part1 + ext;

            pos = part1.IndexOf(",");
            if (pos > -1)
                return part1.Substring(0, pos) + " of" + part1.Substring(pos + 1) + " " + itemText + ext;

            if (IsShapeDescription) return part1 + " " + itemText; // todo: why no ext here?

            return string.Format("{0} of {1}{2}", part1, itemText, ext);

        }

    }
}
