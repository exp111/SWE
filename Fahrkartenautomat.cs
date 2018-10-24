using System;
using System.Collections.Generic;
using System.IO.Pipes;

namespace KonsolenPrototyp
{
	class Order
	{
		public List<Typ> Typs = new List<Typ>();
		public Tarif Tarif = (Tarif)(-1);
		public double cost = 6.5;
		public double paid = 0;
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

			ASK_ADMIN_PW,
			ADMIN
		}

		private readonly List<double> acceptedCash = new List<double>{ 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 50 };
		private readonly List<double> outputCash = new List<double> { 0.05, 0.1, 0.2, 0.5, 1, 2 };
		private STATES state = STATES.MENU;
		private Order currentOrder;
		private PriceList prices = new PriceList();
		private String adminPW = "no u"; //it's safe writing this in the code /s

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
				case STATES.ASK_ADMIN_PW:
					state = AskAdminPW();
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
			Console.WriteLine("2. Admin Menu");
			var input = Console.ReadLine();
			switch (input)
			{
				case "1":
					currentOrder = new Order();
					return STATES.MENU + 1;
				case "2":
					return STATES.ASK_ADMIN_PW;
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
			if (input == "c")
				return STATES.MENU;
			if (!Typ.TryParse(input, out intput) || !Enum.IsDefined(typeof(Typ), intput))
				return STATES.SELECT_PERSONS;

			//Add to current order
			currentOrder.Typs.Add(intput);
			Console.WriteLine("Do you want to add another person? (y/n)");
			input = Console.ReadLine();
			switch (input)
			{
				case "y":
					return STATES.SELECT_PERSONS;
				case "c":
					return STATES.MENU;
				default:
				case "n":
					return STATES.SELECT_PERSONS + 1;
			}
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
			if (input == "c")
				return STATES.MENU;
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
					if (input == "c") //cancel
					{
						//if something was already paid
						if (currentOrder.paid > 0)
						{
							currentOrder.cost = 0; //set cost to 0 so we output everything the user has paid
							return STATES.OUTPUT_MONEY;
						}

						return STATES.MENU;

					}
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
			while (toPay > 0.05) //check if we still need to pay something; 0.05 cuz that's the smallest thing we pay & double are not precise
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

			Console.Read();
			if (currentOrder.cost > 0) //check if he gets a ticket
				return STATES.OUTPUT_TICKET;

			//this should not happen
			return 0;
		}

		private STATES OutputTicket()
		{
			//TODO: check if we have enough tickets
			//TODO: log the ticket infos
			//TODO: maybe print a summary?
			Console.WriteLine("Here is your ticket.");
			Console.WriteLine("Have a nice day.");
			Console.Read();
			return STATES.MENU;
		}

		private STATES AskAdminPW()
		{
			Console.WriteLine("Please enter the Admin Password: ");
			string password = "";
			do
			{
				ConsoleKeyInfo key = Console.ReadKey(true);
				// Backspace Should Not Work
				if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
				{
					password += key.KeyChar;
					Console.Write("*");
				}
				else
				{
					if (key.Key == ConsoleKey.Backspace && password.Length > 0)
					{
						password = password.Substring(0, (password.Length - 1));
						Console.Write("\b \b");
					}
					else if (key.Key == ConsoleKey.Enter)
					{
						break;
					}
				}
			} while (true);
			Console.WriteLine();

			if (adminPW == password)
				return STATES.ADMIN;

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