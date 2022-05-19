namespace AhoCorasickApp
{
    public partial class MainForm : Form
    {
        public MainForm(FileSearcher searcher)
        {
            InitializeComponent();
            searcher.SearchAndDisplayFiles();
        }
    }
}