namespace AhoCorasickApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Wrong arguments! Please try again.");
                return;
            }

            var searcher = FileSearcher.CreateSearcher(args);

            if (searcher == null)
            {
                Console.WriteLine("Wrong arguments! Please try again.");
                return;
            }
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(searcher));

        }
    }
}