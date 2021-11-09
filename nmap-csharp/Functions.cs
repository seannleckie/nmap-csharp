using System;
using System.Collections.Generic;
namespace nmapcsharp
{
	public static class Functions
	{
		private static readonly Dictionary<int, ConsoleColor> Colors = new Dictionary<int, ConsoleColor>();
		public static void InitColors()
		{
			Colors.Add(1, ConsoleColor.Blue);
			Colors.Add(2, ConsoleColor.Green);
			Colors.Add(3, ConsoleColor.Red);
			Colors.Add(4, ConsoleColor.Yellow);
			Colors.Add(5, ConsoleColor.Cyan);
		}
			
		public static void Log(string data, int type)
		{
			Console.Write("[{0}]", DateTime.Now);
			Console.ForegroundColor = Colors[type];
			Console.WriteLine(data);
			Console.ForegroundColor = ConsoleColor.White;
		}
	}
}

