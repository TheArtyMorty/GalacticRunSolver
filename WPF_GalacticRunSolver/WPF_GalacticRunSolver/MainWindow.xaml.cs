using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_GalacticRunSolver.Model;
using WPF_GalacticRunSolver.ViewModel;
using CLI;
using Microsoft.Win32;
using System.IO;
using WPF_GalacticRunSolver.Utility;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using WPF_GalacticRunSolver.Bot;

namespace WPF_GalacticRunSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class MyWPFLogger : IManagedLogger
    {
        public MyWPFLogger(TextBox log)
        {
            Debug.WriteLine("The WPF Logger was created.");
            _log = log;
        }

        private TextBox _log;

        private void LogOnUiThread(string input)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input,
                    new Action(() => {
                        _log.Text += Environment.NewLine + input;
                        _log.ScrollToEnd();
                    }));
        }

        private void LogOnCurrentThread(string input)
        {
            _log.Text += Environment.NewLine + input;
        }

        public void Log(string input)
        {
            LogOnUiThread(string.Copy(input));
        }

        ~MyWPFLogger()
        {
            Debug.WriteLine("The WPF Logger was deleted.");
        }
    }


    public class ScreenPosition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public ScreenPosition(int x = 0, int y = 0)
        {
            _Position = new System.Drawing.Point(x, y);
        }

        public System.Drawing.Point _Position { get; set; }

        public string _X
        {
            get
            {
                return _Position.X.ToString();
            }
        }

        public string _Y
        {
            get
            {
                return _Position.Y.ToString();
            }
        }

        public void SetNewPosition(System.Drawing.Point point)
        {
            _Position = point;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_X)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Y)));
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow(MapViewModel mapvm)
        {
            InitializeComponent();

            _Map = mapvm;

            this.Map.DataContext = _Map;

            m_Logger = new MyWPFLogger(DisplayTextBox);

            m_mapArea = new MapArea(this);
            m_mapArea.Hide();

            _theBot.SetParent(this);
        }


        #region Members
        MapArea m_mapArea;
        public ScreenPosition TopLeftCorner { get; set; } = new ScreenPosition(290, 165);
        public ScreenPosition BottomRightCorner { get; set; } = new ScreenPosition(1105, 984);

        public MapViewModel _Map = new MapViewModel(16);

        ManagedMap _managedMap;
        ManagedSolver _managedSolver;
        MyWPFLogger m_Logger;

        public Bot.Bot _theBot { get; set; } = new Bot.Bot(null);

        #endregion
        public void LoadMap(Map map)
        {
            ClearSolverDisplay();
            _Map = new MapViewModel(map);
            this.Map.DataContext = _Map;
        }

        public void Solve()
        {
            Solve(false);
        }

        public SolutionViewModel GetFirstSolution()
        {
            if (Solutions.DataContext != null)
            {
                SolutionsViewModel solutions = Solutions.DataContext as SolutionsViewModel;
                if (solutions._Solutions.Count != 0)
                {
                    return solutions._Solutions.First();
                }
            }
            return null;
        }

        private void Load_Map(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text File | *.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                LoadMap(new Map(openFileDialog.FileName));
            }
        }

        private void Bot_ConnectToGame(object sender, RoutedEventArgs e)
        {
            _theBot.ConnectToGame(GameID.Text, BotName.Text);
        }

        private void Bot_Solve(object sender, RoutedEventArgs e)
        {
            RecognizeTest(sender, e);
            Solve(true);
        }

        private void RecognizeTest(object sender, RoutedEventArgs e)
        {
            System.Drawing.Size size = new System.Drawing.Size(
                Math.Abs(BottomRightCorner._Position.X - TopLeftCorner._Position.X),
                Math.Abs(BottomRightCorner._Position.Y - TopLeftCorner._Position.Y));
            Bitmap memoryImage = new Bitmap(size.Width, size.Height);
            Graphics test = Graphics.FromImage(memoryImage);
            test.CopyFromScreen(TopLeftCorner._Position.X, TopLeftCorner._Position.Y, 0, 0, size);

            LoadMap(Utilities.GetMapFromImage(memoryImage));
        }


        private void Load_Map_From_URL(object sender, RoutedEventArgs e)
        {
            string url = UrlTextBox.Text;
            if (Utilities.IsValidMapUrl(url))
            {
                LoadMap(Utilities.GetMapFromWeburl(url));
            }
            else
            {
                MessageBox.Show("Could not load map from this url...", "Invalid URL", MessageBoxButton.OK);
            }
        }

        private void Set_Area(object sender, RoutedEventArgs e)
        {
            m_mapArea.StartSelection();
        }

        private void Send_Solution(object sender, RoutedEventArgs e)
        {
            if (Solutions.DataContext != null)
            {
                SolutionsViewModel solutions = Solutions.DataContext as SolutionsViewModel;
                if (solutions._Solutions.Count != 0)
                {
                    Reset_Map(sender, e);
                    Utilities.SendSolutionToFirefox(solutions._Solutions.First(), _Map);
                }
            }
        }

        private void Save_Map(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text File | *.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                _Map.SaveMap(saveFileDialog.FileName);
                MessageBox.Show("Map was saved", "Map was saved...", MessageBoxButton.OK);
            }
        }

        private void Reset_Map(object sender, RoutedEventArgs e)
        {
            _Map.Reset();
        }

        private void Clear_Map(object sender, RoutedEventArgs e)
        {
            _Map = new MapViewModel(16);
            this.Map.DataContext = _Map;
        }

        private void OnMouseClickMapUrl(object sender, RoutedEventArgs e)
        {
            UrlTextBox.SelectAll();
        }

        private void OnKeyDownMapUrl(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Load_Map_From_URL(sender, e);
            }
        }


        private void ClearSolverDisplay()
        {
            DisplayTextBox.Text = "";
            SolutionTextBox.Text = "";
            this.Solutions.DataContext = new SolutionsViewModel(new List<ManagedSolution>(), _Map);
        }

        private List<ManagedSolution> SolveMap()
        {
            _managedSolver = new ManagedSolver(30, m_Logger);
            //Save map to temp file that will be deleted
            string tempFileName = System.IO.Path.GetTempFileName();
            _Map.SaveMap(tempFileName);
            _managedMap = new ManagedMap(tempFileName);
            var solutions = _managedSolver.GetAllSolutions(_managedMap);
            File.Delete(tempFileName);
            return solutions;
        }

        private void DisplaySolutions(List<ManagedSolution> solutions)
        {
            if (solutions.Count > 0)
            {
                var solution = solutions.First();
                SolutionTextBox.Text = solutions.Count.ToString() + " solution" + (solutions.Count == 1 ? "" : "s");
                SolutionTextBox.Text += " in " + solution.Moves.Count.ToString() + " moves exist.";
                this.Solutions.DataContext = new SolutionsViewModel(solutions, _Map);
            }
            else
            {
                SolutionTextBox.Text = "No solutions where found in under 8 moves...";
            }
        }


        private void Solve(bool sendToBrowser = false)
        {
            _Map._InitialMap = new Map(_Map._Map);
            ClearSolverDisplay();

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            if (sendToBrowser) worker.RunWorkerCompleted += worker_SendAfterCompleted;
            worker.RunWorkerAsync();
        }

        private void Solve_Map(object sender, RoutedEventArgs e)
        {
            Solve();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var solutions = SolveMap();
            e.Result = solutions;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DisplaySolutions((List<ManagedSolution>)e.Result);
        }

        void worker_SendAfterCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Send_Solution(sender, null);
        }
    }
}