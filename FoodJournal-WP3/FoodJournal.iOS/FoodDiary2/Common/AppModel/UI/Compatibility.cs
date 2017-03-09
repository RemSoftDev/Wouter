using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !WINDOWS_PHONE

namespace System.Windows.Media { }

namespace FoodJournal.AppModel.UI
{
	public enum Visibility
	{
		Visible,
		Collapsed
	}

	public class Color { }

	public class Brush { }
	public class SolidColorBrush : Brush { public SolidColorBrush(Color color) { } }

	public class TranslateTransform {}

}

#endif