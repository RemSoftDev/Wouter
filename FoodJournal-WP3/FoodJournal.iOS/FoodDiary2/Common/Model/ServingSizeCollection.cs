using FoodJournal.Model;
using FoodJournal.Model.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FoodJournal.Values
{

	public interface IServingSizeTarget
	{
		string ServingSizesDB { get; set; }
	}

	public class ServingSize
	{
		public Amount amount1;
		public Amount amount2;
		public ServingSize(Amount amount1) { this.amount1 = amount1; }
		public ServingSize(Amount amount1, Amount amount2) { this.amount1 = amount1; this.amount2 = amount2; }
	}

	public class ServingSizeCollection
	{

		private FoodItem item;
		private IServingSizeTarget data;
		private List<ServingSize> values = new List<ServingSize>();
		private bool loading;

		public Amount FirstAmount
		{
			get
			{
				if (values.Count > 0) return values[0].amount1;
				return Amount.Empty;
			}
		}

		public ServingSizeCollection(FoodItem item, IServingSizeTarget data)
		{

			loading = true;

			this.item = item;
			this.data = data;
			string value = data.ServingSizesDB;

			Add(item.LastAmount);

			if (value == null || value.Length == 0) { loading = false; return; }
			var x = value.Split(new char[] { '|' });

			for (int i = 0; i < x.Length; i++)
			{
				if (x[i].IndexOf("=") > 0)
				{
					var y = x[i].Split(new char[] { '=' });
					Add(y[0], Amount.ParseEquivalent(y[1]));
				}
				else
					Add(x[i]);
			}

			loading = false;

		}

		public IEnumerable<ServingSize> Amounts { get { foreach (var ss in values) yield return ss; } }

		public void RenameAmount(Amount from, Amount to, Amount newEquivalent)
		{
			for (int i = 0; i < values.Count; i++)
			{
				var ss = values[i];
				if (ss.amount1 == from)
					values[i].amount1 = to;
			}
			if (newEquivalent.IsValid)
				SetConversion(to, newEquivalent);
			UpdateDataString();
		}

		public void Delete(Amount amount)
		{
			for (int i = 0; i < values.Count; i++)
			{
				var ss = values[i];
				if (ss.amount1 == amount)
				{
					values.RemoveAt(i); i--;
					break;
				}
			}
			UpdateDataString();
		}

		public void Add(Amount amount1)
		{
			Add(amount1, GetEquivalent(amount1));
		}

		// 2 srv = 50 g
		// ss1 1 srv (25 g)
		public void SetConversion(Amount amount1, Amount amount2)
		{
			Add(amount1, amount2);
			for (int i = 0; i < values.Count; i++)
			{
				var ss = values[i];
				if (ss.amount1 != amount1)
				{
					var div = amount1 / ss.amount1;
					if (div != 0)
						Add(ss.amount1, amount2 * (1 / div));
				}
			}

		}

		public void InsertAt0(Amount amount)
		{
			if (!amount.IsValid) return;
			values.Insert(0, new ServingSize(amount, GetEquivalent(amount)));
			UpdateDataString();
		}

		public void Add(Amount amount1, Amount amount2)
		{
			if (!amount1.IsValid) return;

			for (int i = 0; i < values.Count; i++)
			{
				var ss = values[i];
				if (ss.amount1 == amount1)
				if (ss.amount2 == amount2)
				{
					return;
				}
				else
				{
					ss.amount2 = amount2;
					UpdateDataString();
					return;
				}
			}
			values.Add(new ServingSize(amount1, amount2));
			UpdateDataString();
		}

		public Amount GetEquivalent(Amount amount)
		{
			foreach (var ss in values)
			{
				float div = amount / ss.amount1;
				if (div > 0) return ss.amount2 * div;
			}
			return Amount.Empty;
		}

		/// <summary>
		/// Sample usage: CalculateScale(1 slice, 100 g) -> if (1 slice = 20 g) -> returns 0.2
		/// </summary>
		/// <returns></returns>
		public Single CalculateScale(Amount amount1, Amount amount2)
		{
			Single result = amount1 / amount2;
			if (result > 0) return result;


			foreach (var ss in values)
				if (ss.amount2.IsValid)
				{
					Single part1 = amount1 / ss.amount1;
					if (part1 > 0)
					{
						Single part2 = ss.amount2 / amount2;
						if (part2 > 0) return part1 * part2;
					}
				}

			return 0;
		}

		private void UpdateDataString()
		{
			if (!loading)
			{
				data.ServingSizesDB = GetDataString();
				item.OnPropertyChanged("ServingSize");
			}
		}

		public override string ToString() { return GetDataString(); }

		public string GetDataString()
		{
			var s = new StringBuilder();
			var b = false;
			foreach (var x in values)
			{
				if (b) { s.Append("|"); } else { b = true; }
				s.Append(x.amount1.ToStorageString());
				s.Append("=");
				s.Append(x.amount2.ToStorageString());
			}
			return s.ToString();
		}

	}

}
