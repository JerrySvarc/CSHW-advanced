using System;

namespace GamePhysics
{
    public struct Seconds
    {
        public float Value;
    }

    public struct Meters
    {
        public float Value;
        public static Speed operator /(Meters meters, Seconds seconds) => new Speed { Value = meters.Value / seconds.Value };
    }

    public struct Speed
    {
        public float Value;
        public override string ToString() => Value + " m/s";

    }

    public static class IntExt
    {
        public static Seconds Seconds(this int val) => new Seconds { Value = val };
        public static Meters Meters(this int val) => new Meters { Value = val };
        public static Speed MeterPerSeconds(this int val) => new Speed { Value = val };
    }

    public static class DoubleExt
    {
        public static Seconds Seconds(this double val) => new Seconds { Value = (float)val };

    }

    public static class FloatExt
    {
        public static Meters Meters(this float val) => new Meters { Value = val };
        public static Speed MeterPerSeconds(this float val) => new Speed { Value = val };
    }

    class Program
    {
        static void Main(string[] args)
        {
            var distance = 2.Meters();
            var time = 3.Seconds();
            var speed = distance / time;
            Console.WriteLine($"Speed: {speed}"); // Prints 'Speed: 0.6666666666666666 m/s'
        }
    }
}
