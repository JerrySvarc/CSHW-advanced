using Cuni.NPrg038;
using System.Text;

namespace AhoCorasickApp
{
    public class FileSearcher
    {
        int Counter;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        Thread[] consumerThreads;
        string SearchedText { get; set; }
        string DirectoryPath { get; set; }
        int CrawlerCount { get; set; }
        int SearcherCount { get; set; }
        int MaxQueueLength { get; set; }
        int ErrorCount { get; set; } = 0;
        int MatchCount { get; set; } = 0;
        int AllFilesCount { get; set; } = 0;
        int ReadBytes { get; set; }  = 0;
        Queue<string?> FileQueue { get; set; }

        Object MatchLock = new Object();
        Object ErrorLock = new Object();
        Object ReadBytesLock = new Object();

        MainForm mainForm;

        public FileSearcher(string searchedText, string directoryPath, int crawlerCount, int searcherCount, int maxQueueLength)
        {
            SearchedText = searchedText;
            DirectoryPath = directoryPath;
            CrawlerCount = crawlerCount;
            SearcherCount = searcherCount;
            MaxQueueLength = maxQueueLength;
            FileQueue = new Queue<string?>();

        }
        /// <summary>
        /// Starts the producer and consumer threads. Recursively searches the file system and tries to find all files containing the desired text sequence. 
        /// </summary>
        public void SearchAndDisplayFiles()
        {
            //Start the timer
            InitializeTimer();
            //Start the producer
            Thread producerThread = new Thread(EnqueueFiles);
            producerThread.Start();

            if (SearcherCount < 1)
            {
                return;
            }
            //Start the consumers
            consumerThreads = new Thread[SearcherCount];
            for (int i = 0; i < SearcherCount; i++)
            {
                (consumerThreads[i] = new Thread(ConsumeFiles)).Start();
            }
        }

        public void SetOutputForm(MainForm form)
        {
            this.mainForm = form;
        }

        /// <summary>
        /// Returns a prepared instance of the AhoCorasickSearch class. 
        /// </summary>
        /// <returns></returns>
        private AhoCorasickSearch GetAhoCorasickInstance()
        {
            AhoCorasickSearch searcher = new AhoCorasickSearch();
            searcher.AddPattern(Encoding.Default.GetBytes(SearchedText));
            searcher.AddPattern(Encoding.UTF8.GetBytes(SearchedText));
            searcher.AddPattern(Encoding.Unicode.GetBytes(SearchedText));
            searcher.Freeze();
            return searcher;
        }

        /// <summary>
        /// Used by the crawler thread. Recursively search for files in the specified folder. Then keep adding files into the queue if there is space and wake up searcher threads. 
        /// </summary>
        private void EnqueueFiles()
        {
            string[] files = Directory.GetFiles(DirectoryPath, "*.*", new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            });

            AllFilesCount = files.Length;

            int i = 0;
            while (i < files.Length)
            {
                lock (FileQueue)
                {
                    while (i < files.Length && FileQueue.Count < MaxQueueLength)
                    {
                        FileQueue.Enqueue(files[i]);
                        i++;
                    }
                    Monitor.PulseAll(FileQueue);
                }
            }

            lock (FileQueue)
            {
                for (int j = 0; j < SearcherCount; j++)
                {
                    FileQueue.Enqueue(null);
                    Monitor.PulseAll(FileQueue);
                }
            }
        }

        /// <summary>
        /// Prints the desired statistic. 
        /// </summary>
        /// <param name="counter"></param>
        public void UpdateLabel(int counter)
        {
            mainForm.label1.Invoke(() => mainForm.label1.Text = "MATCH " + MatchCount.ToString() + "/ALL " + AllFilesCount.ToString()
                + "/ERROR " + ErrorCount.ToString() + "/READ " + (((float)ReadBytes / 1000000)).ToString() + " MB/ " + "Search time: " + ((float)counter / 10).ToString() + "s");
        }
        private void InitializeTimer()
        {

            Counter = 0;
            timer.Interval = 100;
            timer.Enabled = true;
            this.timer.Tick += new System.EventHandler(timer_Tick);
        }

        private void timer_Tick(object sender, System.EventArgs e)
        {
            Counter++;
            UpdateLabel(Counter);
            if (CheckIfAllSearchersFinished())
            {
                timer.Enabled = false;
            }
        }
        /// <summary>
        /// Used by the searcher threads. If there is nothing in the queue, wait. Then get a file and search. 
        /// </summary>
        private void ConsumeFiles()
        {
            while (true)
            {
                string? fileName;
                lock (FileQueue)
                {
                    if (FileQueue.Count == 0)
                    {
                        Monitor.Wait(FileQueue);
                    }
                    fileName = FileQueue.Dequeue();
                }
                if (fileName == null)
                {
                    return;
                }
                SearchFile((string)fileName);
            }
        }

        /// <summary>
        /// Uses the AhoCorasick library to find whether the file contains a certain text sequence. 
        /// </summary>
        /// <param name="fileName"></param>
        private void SearchFile(string fileName)
        {
            var search = GetAhoCorasickInstance();
            int readBytes = 0;
            try
            {
                var fileStream = File.OpenRead(fileName);
                int character = fileStream.ReadByte();
                readBytes++;
                
                IByteSearchState state = search.InitialState;
                while (character != -1)
                {
                    state = state.GetNextState((byte)character);
                    if (state.HasMatchedPattern)
                    {
                        FileInfo fileInfo = new FileInfo(fileName);
                        mainForm.listBox1.Invoke(() => mainForm.listBox1.Items.Add(fileInfo.Name));
                        lock (MatchLock)
                        {
                            MatchCount++;
                        }
                        lock (ReadBytesLock)
                        {
                            ReadBytes += readBytes;
                        }
                        break;
                    }
                    else
                    {
                        character = fileStream.ReadByte();
                        readBytes++;
                    }
                }
                lock (ReadBytesLock)
                {
                    ReadBytes += readBytes;
                }
            }
            catch (Exception)
            {
                lock (ReadBytesLock)
                {
                    ReadBytes += readBytes;
                }
                lock (ErrorLock)
                {
                    ErrorCount++;
                }
            }
        }

        public bool CheckIfAllSearchersFinished()
        {
            foreach (var thread in consumerThreads)
            {
                if (thread.IsAlive)
                {
                    return false;
                }
            }
            return true;
        }

        public static FileSearcher CreateSearcher(string[] args)
        {
            object[] parsedArgs = ParseArguments(args);
            if (parsedArgs == null)
            {
                return null;
            }
            return new FileSearcher((string)parsedArgs[0], (string)parsedArgs[1], (int)parsedArgs[2], (int)parsedArgs[3], (int)parsedArgs[4]);

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
