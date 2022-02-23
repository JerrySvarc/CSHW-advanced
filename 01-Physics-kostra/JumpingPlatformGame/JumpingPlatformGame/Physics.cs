namespace JumpingPlatformGame {
	struct Seconds
	{
		public float value;
	}

	struct Meters
	{
		public float value;
		public static Speed operator /(Meters meters, Seconds seconds) => new Speed { value = meters.value / seconds.value };
	}


	struct Speed
	{
		public float value;
		public override string ToString() => value + " m/s";

	}

	static class IntExt
	{
		public static Seconds Seconds(this int val) => new Seconds { value = val };
		public static Meters Meters(this int val) => new Meters { value = val };
		public static Speed MeterPerSeconds(this int val) => new Speed { value = val };
	}
}