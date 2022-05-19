using Cuni.NPrg038;
using System.IO;
using System.Text;
using System.Threading;

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

    public class FileSearcher
    {
        string SearchedText { get; set; }
        string DirectoryPath { get; set; }
        int CrawlerCount { get; set; }
        int SearcherCount { get; set; }
        int MaxQueueLength { get; set; }

        Queue<string> FileQueue { get; set; }
        public FileSearcher(string searchedText, string directoryPath, int crawlerCount, int searcherCount, int maxQueueLength)
        {
            SearchedText = searchedText;
            DirectoryPath = directoryPath;
            CrawlerCount = crawlerCount;
            SearcherCount = searcherCount;
            MaxQueueLength = maxQueueLength;
            FileQueue = new Queue<string>();
        }
        
        private AhoCorasickSearch GetAhoCorasickInstance()
        {
            AhoCorasickSearch searcher = new AhoCorasickSearch();
            searcher.AddPattern(Encoding.Default.GetBytes(SearchedText));
            searcher.AddPattern(Encoding.UTF8.GetBytes(SearchedText));
            searcher.AddPattern(Encoding.Unicode.GetBytes(SearchedText));
            searcher.Freeze();
            return searcher;
        }

        public void SearchAndDisplayFiles()
        {
            
        }

        public static FileSearcher CreateSearcher(string[] args)
        {
            object[] parsedArgs = ParseArguments(args);
            if (parsedArgs == null)
            {
                return null;
            }
            return new FileSearcher((string)parsedArgs[0], (string)parsedArgs[1], (int) parsedArgs[2], (int)parsedArgs[3], (int)parsedArgs[4]);
            
        }

        public static object[] ParseArguments(string[] args)
        {
            object[] parsedVals = new object[args.Length];

            parsedVals[0] = args[0];
            parsedVals[1] = args[1];

            if (!int.TryParse(args[2], out int crawlerCount))
            {
                return null;
            }
            parsedVals[2] = crawlerCount;

            if (!int.TryParse(args[3], out int searcherCount))
            {
                return null;
            }
            parsedVals[3] = searcherCount;

            if (!int.TryParse(args[4], out int queueLength))
            {
                return null;
            }
            parsedVals[4] = queueLength;
            return parsedVals;
        }
    }


}