using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KonsolenPrototyp
{
	class PriceList
	{
		private double[][] prices = new double[Enum.GetNames(typeof(Tarif)).Length][];
		private string fileName = "prices.xml";

		public PriceList()
		{
			int length = Enum.GetNames(typeof(Typ)).Length;
			for (int i = 0; i < prices.Length; i++)
			{
				prices[i] = new double[length];
			}
		}

		public void LoadPrices()
		{
			XmlReader reader = XmlReader.Create(fileName);
			int current = -1;
			while (reader.Read())
			{
				if (reader.IsStartElement())
				{
					switch (reader.Name.ToLower())
					{
						case "tarif":
						{
							if (reader.HasAttributes)
							{
								Tarif index;
								if (Tarif.TryParse(reader.GetAttribute("type"), true, out index) && Enum.IsDefined(typeof(Tarif), index)) ;
								current = (int)index;
							}

							break;
						}
						default:
						{
							Typ index;
							if (Typ.TryParse(reader.Name, true, out index) && Enum.IsDefined(typeof(Typ), index))
							{
								if (current != -1)
									prices[current][(int)index] = reader.ReadElementContentAsDouble();
							}

							break;
						}
					}
				}
			}

		}

		public double GetPrice(Tarif tariff, Typ typ)
		{
			if ((int)tariff >= prices.Length)
				return 0;
			var list = prices[(int)tariff];
			if ((int)typ >= list.Length)
				return 0;
			return list[(int)typ];
		}
	}
}
