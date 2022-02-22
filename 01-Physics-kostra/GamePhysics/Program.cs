using System;

namespace GamePhysics {

	class Program {
		static void Main(string[] args) {
			var distance = 2.Meters();
			var time = 3.Seconds();
			var speed = distance / time;
			Console.WriteLine($"Speed: {speed}");
		}
	}
}
