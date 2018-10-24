using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Xml;

namespace KonsolenPrototyp
{
	enum Typ : int
	{
		KIND = 0,
		ERMAESSIGT = 1,
		STANDARD = 2,
		SENIOR = 3
	}

	enum Tarif : int
	{
		A = 0,
		B = 1,
		C = 2
	}

	class Order
	{
		public List<Typ> Typs = new List<Typ>();
		public Tarif Tarif = (Tarif)(-1);
		public double cost = 6.5;
		public double paid = 0;
	}

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
								Tarif.TryParse(reader.GetAttribute("type"), true, out index);
								current = (int)index;
							}

							break;
						}
						default:
						{
							Typ index;
							if (Typ.TryParse(reader.Name, true, out index))
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

	// ReSharper disable once IdentifierTypo
	class Fahrkartenautomat
	{
		enum STATES : int
		{
			MENU = 0,
			SELECT_TARIF = 1,
			SELECT_PERSONS = 2,
			INPUT_MONEY = 3,
			OUTPUT_MONEY = 4,
			OUTPUT_TICKET = 5,

			ADMIN
		}

		private readonly List<double> acceptedCash = new List<double>{ 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 50 };
		private readonly List<double> outputCash = new List<double> { 0.05, 0.1, 0.2, 0.5, 1, 2 };
		private STATES state = STATES.MENU;
		private Order currentOrder;
		private PriceList prices = new PriceList();

		public Fahrkartenautomat()
		{
			prices.LoadPrices();
		}

		public void Run()
		{
			switch (state)
			{
				case STATES.MENU:
					state = Menu();
					break;
				case STATES.SELECT_TARIF:
					state = SelectTarif();
					break;
				case STATES.SELECT_PERSONS:
					state = SelectPersons();
					break;
				case STATES.INPUT_MONEY:
					state = InputMoney();
					break;
				case STATES.OUTPUT_MONEY:
					state = OutputMoney();
					break;
				case STATES.OUTPUT_TICKET:
					state = OutputTicket();
					break;
				case STATES.ADMIN:
					state = Admin();
					break;
				default:
					state = STATES.MENU;
					break;
			}
		}

		private STATES Menu()
		{
			Console.Clear();
			Console.WriteLine("What do you want to do:");
			Console.WriteLine("1. Buy a ticket");
			var input = Console.ReadLine();
			switch (input)
			{
				case "1":
					currentOrder = new Order();
					return STATES.MENU + 1;
				case "1337":
					return STATES.ADMIN;
				default:
					Console.WriteLine("Invalid Input!");
					Console.Clear();
					break;
			}
			return STATES.MENU;
		}

		private STATES SelectPersons()
		{
			if (currentOrder.Typs.Count > 0)
			{
				Console.WriteLine("Your current Types:");
				foreach (var typ in currentOrder.Typs)
				{
					Console.WriteLine(typ.ToString());
				}
			}
			Console.WriteLine("Select your Type:");
			foreach (Typ typ in (Typ[]) Enum.GetValues(typeof(Typ)))
			{
				//TODO: maybe show prices?
				Console.WriteLine($"{(int)typ}. {typ.ToString()}");
			}
			var input = Console.ReadLine();
			Typ intput;
			//Check
			if (!Typ.TryParse(input, out intput) || !Enum.IsDefined(typeof(Typ), intput))
				return STATES.SELECT_PERSONS;

			//Add to current order
			currentOrder.Typs.Add(intput);
			Console.WriteLine("Do you want to add another person? (y/n)");
			input = Console.ReadLine();
			if (input == "y")
			{
				return STATES.SELECT_PERSONS;
			}
			else
			{
				return STATES.SELECT_PERSONS + 1;
			}
			return 0;
		}

		private STATES SelectTarif()
		{
			Console.WriteLine("Select your Tariff:");
			foreach (Tarif tarif in (Tarif[])Enum.GetValues(typeof(Tarif)))
			{
				//TODO: maybe show prices?
				Console.WriteLine($"{(int)tarif}. {tarif.ToString()}");
			}
			var input = Console.ReadLine();
			Tarif intput;
			//Check
			if (!Tarif.TryParse(input, out intput) || !Enum.IsDefined(typeof(Tarif), intput))
				return STATES.SELECT_TARIF;

			//Add to current order
			currentOrder.Tarif = intput;
			return STATES.SELECT_TARIF + 1;
		}

		private STATES InputMoney()
		{
			//Get price
			currentOrder.cost = 0;
			string info = "";
			for (int i = 0; i < currentOrder.Typs.Count; i++)
			{
				//add single cost of tarif & type to total cost
				currentOrder.cost += prices.GetPrice(currentOrder.Tarif, currentOrder.Typs[i]);
				//ticket info string
				info += currentOrder.Typs[i];
				info += i < currentOrder.Typs.Count - 1 ? ", " : ""; //check if last one
			}
	
			currentOrder.paid = 0;
			while (currentOrder.cost > currentOrder.paid)
			{
				Console.Clear();
				//write down info about the ticket (tariff, types & price)
				Console.WriteLine($"You want to buy {currentOrder.Typs.Count} Tarif {currentOrder.Tarif} Tickets ({info}).");
				Console.WriteLine($"Please insert {currentOrder.cost - currentOrder.paid} Euro:");

				string input = Console.ReadLine();
				double intput = 0;
				//TODO: check if is correct
				Double.TryParse(input, out intput);
				if (acceptedCash.Contains(intput))
					//TODO: check if we have enough space
					currentOrder.paid += intput;
				else
				{
					Console.WriteLine("We don't accept this type of cash.");
				}
			}

			if (currentOrder.paid == currentOrder.cost)
				return STATES.INPUT_MONEY + 2; //2 cuz +1 is output money
			
			return STATES.OUTPUT_MONEY;
		}

		private STATES OutputMoney()
		{
			//TODO: check if we have enough cash to give
			double toPay = currentOrder.paid - currentOrder.cost;
			if (toPay <= 0) //TODO: something is fucky
				return STATES.OUTPUT_TICKET;

			Console.WriteLine("You get back: ");
			int currentCashType = outputCash.Count - 1;
			while (toPay > 0)
			{
				while (toPay - outputCash[currentCashType] < 0)
				{
					if (currentCashType == 0) //TODO: this shouldn't happen either
						return 0;
					currentCashType--;
				}
				Console.WriteLine($"{outputCash[currentCashType]} Euro");
				toPay -= outputCash[currentCashType];
			}
			if (toPay == 0)
				return STATES.OUTPUT_TICKET;

			//TODO: this should not happen
			return 0;
		}

		private STATES OutputTicket()
		{
			//TODO: check if we have enough tickets
			//TODO: log the ticket infos
			Console.WriteLine("Here is your ticket.");
			Console.WriteLine("Have a nice day.");
			Console.Read();
			return STATES.MENU;
		}

		private STATES Admin()
		{
			//TODO: Admin Menu
			Console.WriteLine("Hello Admin!");
			Console.WriteLine("1. Reset");
			
			var input = Console.ReadLine();
			switch (input)
			{
				case "1":
					return STATES.MENU;
					break;
				default:
					Console.WriteLine("Invalid Input!");
					Console.Clear();
					break;
			}
			return 0;
		}
	}
}