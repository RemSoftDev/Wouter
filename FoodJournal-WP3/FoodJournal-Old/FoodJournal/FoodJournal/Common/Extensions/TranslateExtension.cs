using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using FoodJournal.Common.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FoodJournal.Common.Extensions
{
    [ContentProperty("Text")]
        public class TranslateExtension : IMarkupExtension
        {
            readonly CultureInfo ci;
            const string ResourceId = "UsingResxLocalization.Resx.AppResources";

            public TranslateExtension()
            {
                ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
            }

            public string Text { get; set; }

            public object ProvideValue(IServiceProvider serviceProvider)
            {
                if (Text == null)
                    return "";

            ResourceManager temp = new ResourceManager("FoodJournal.Common.Resources.AppResources", typeof(Localize).GetTypeInfo().Assembly);

            var translation = temp.GetString(Text, ci);

                if (translation == null)
                {
#if DEBUG
                    throw new ArgumentException(
                        String.Format("Key '{0}' was not found in resources '{1}' for culture '{2}'.", Text, ResourceId, ci.Name),
                        "Text");
#else
				translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
#endif
                }
                return translation;
            }
        }
    }

