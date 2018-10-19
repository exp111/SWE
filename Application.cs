using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonsolenPrototyp
{
	class Application
	{
		static void Main(string[] args)
		{
			Fahrkartenautomat automat = new Fahrkartenautomat();
			while (true)
			{
				automat.Run();
			}

		}
	}
}
