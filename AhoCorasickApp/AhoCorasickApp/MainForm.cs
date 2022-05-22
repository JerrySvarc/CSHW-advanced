namespace AhoCorasickApp
{
    public partial class MainForm : Form
    {

        FileSearcher FileSearcher;
        public MainForm(FileSearcher searcher)
        {
            InitializeComponent();
            searcher.SetOutputForm(this);
            FileSearcher = searcher;
            searcher.SearchAndDisplayFiles();
        }
    }
}