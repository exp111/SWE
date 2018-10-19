using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace KonsolenPrototyp
{
	enum Typen : int
	{
		KIND = 0,
		STUDENT = 1,
		ERWACHSENER = 2,
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
		public List<Typen> Typens = new List<Typen>();
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
			SELECT_PERSONS = 1,
			SELECT_TARIF = 2,
			INPUT_MONEY = 3,
			OUTPUT_MONEY = 4,
			OUTPUT_TICKET = 5,

			ADMIN
		}

		private readonly List<double> acceptedCash = new List<double>{ 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 50 };
		private readonly List<double> outputCash = new List<double> { 0.05, 0.1, 0.2, 0.5, 1, 2 };
		private STATES state = STATES.MENU;
		private Order currentOrder;

		public void Run()
		{
			switch (state)
			{
				case STATES.MENU:
					state = Menu();
					break;
				case STATES.SELECT_PERSONS:
					state = SelectPersons();
					break;
				case STATES.SELECT_TARIF:
					state = SelectTarif();
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
					return STATES.SELECT_PERSONS;
					break;
				case "1337":
					return STATES.ADMIN;
					break;
				default:
					Console.WriteLine("Invalid Input!");
					Console.Clear();
					break;
			}
			return STATES.MENU;
		}

		private STATES SelectPersons()
		{
			if (currentOrder.Typens.Count > 0)
			{
				Console.WriteLine("Your current Types:");
				foreach (var typ in currentOrder.Typens)
				{
					Console.WriteLine(typ.ToString());
				}
			}
			Console.WriteLine("Select your Type:");
			foreach (Typen typ in (Typen[]) Enum.GetValues(typeof(Typen)))
			{
				//TODO: maybe show prices?
				Console.WriteLine($"{(int)typ}. {typ.ToString()}");
			}
			var input = Console.ReadLine();
			int intput = -1;
			//TODO: Check
			Int32.TryParse(input, out intput);

			//Add to current order
			currentOrder.Typens.Add((Typen)intput);
			//
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
			int intput = -1;
			//TODO: Check
			Int32.TryParse(input, out intput);

			//Add to current order
			currentOrder.Tarif = (Tarif) intput;
			return STATES.SELECT_TARIF + 1;

			return 0;
		}

		private STATES InputMoney()
		{
			//TODO: get price
			//TODO: write down info about the ticket (tariff, types & price)
			currentOrder.paid = 0;
			while (currentOrder.cost > currentOrder.paid)
			{
				Console.Clear();
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
				Console.WriteLine($"{outputCash[currentCashType]}€");
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