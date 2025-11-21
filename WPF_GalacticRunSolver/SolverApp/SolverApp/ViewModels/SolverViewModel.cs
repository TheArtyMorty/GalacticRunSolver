using SolverApp.Models;
using SolverApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace SolverApp.ViewModels
{
    public class AppLogger : ILogger
    {
        public AppLogger(Action<string> setOutput, Action clear)
        {
            logAction = setOutput;
            clearAction = clear;
        }

        Action<string> logAction;
        Action clearAction;

        public override void Clear()
        {
            clearAction();
        }

        public override void Log(string content)
        {
            logAction(content);
            logAction(Environment.NewLine);
        }
    }

    public class SolverViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public SolverViewModel(SolverPage solverPage)
        {
            _solverPage = solverPage;
            logger = new AppLogger(BackwardLogValue, () => _Log = "");
            LoadMapFromAutosave();
            _SolveMap = new Command(SolveMap);

            //background Worker
            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.WorkerReportsProgress = false;
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        public MapViewModel _map;
        public MapViewModel theMap { 
            get 
            { 
                return _map; 
            } 
            set
            {
                if (_map == value) return;
                else
                {
                    _map = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(theMap)));
                    Clear();
                }
            }
        }

        private SolverPage _solverPage;
        public void CreateNewMap(MapViewModel map)
        {
            theMap = map;
            _solverPage.GenerateMap(theMap);
        }

        void BackwardLogValue(string value)
        {
            var currentContent = _Log;
            _Log = value;
            _Log += currentContent;
        }

        public ObservableCollection<SolutionViewModel> _Solutions { get; set; }

        private AppLogger logger;

        public string log;
        public string _Log {
            get
            {
                return log;
            }
            set
            {
                if (log == value) return;
                else
                {
                    log = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Log)));
                }
            }
        }

        private void Clear()
        {
            logger.Clear();
            _Solutions = new ObservableCollection<SolutionViewModel>();
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Solutions)));
        }

        public Command _SolveMap { get; }

        BackgroundWorker _worker;
        public void SolveMap()
        {
            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
            }
            else
            {
                SaveMapToAutosave();
                Clear();
                _worker.RunWorkerAsync();
                theMap._InitialMap = new Map(theMap._Map);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_SolveButtonText)));
            }
        }

        private void SaveMapToAutosave()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoSave.map");
            theMap.SaveMap(fileName);
        }

        private void LoadMapFromAutosave()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoSave.map");
            CreateNewMap(new MapViewModel(fileName));
        }

        public string _SolveButtonText { get { return _worker.IsBusy ? "Stop" : "Solve"; } }
        public bool _SolveButtonEnabled { get { return !simulationRunning; } }

        public void PlaySolution(Solution solution, Action onFinished)
        {
            if (!simulationRunning)
            {
                simulationRunning = true;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_SolveButtonEnabled)));
                foreach (var svm in _Solutions)
                {
                    svm.Refresh();
                }
                _toDo = onFinished;
                theMap.PlaySolution(solution, OnFinished);
            }
        }
        public void StopSolution()
        {
            theMap.StopPlaying();
        }
        public bool simulationRunning = false;
        private Action _toDo;
        private void OnFinished()
        {
            simulationRunning = false;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_SolveButtonEnabled)));
            foreach (var svm in _Solutions)
            {
                svm.Refresh();
            }
            _toDo();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var solutions = Solver.Solve(theMap._Map, logger, ref _worker);
            e.Result = solutions;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var workerResult = (List<Solution>)e.Result;
            ObservableCollection<SolutionViewModel> result = new ObservableCollection<SolutionViewModel>
                (workerResult.Select(solution => new SolutionViewModel(solution, this)));

            _Solutions = result;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Solutions)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_SolveButtonText)));
        }


        private static int defaultSize = 325;
        public int _ZoomSize { get; set; } = defaultSize;
        internal void ZoomInOrOut(double v, double mapControlWidth)
        {
            _ZoomSize = (int)(-50 + mapControlWidth + v * mapControlWidth);
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_ZoomSize)));
        }
        
        public string _backgroundPhoto {  get; set; }
        internal void SetBackgroundImage(string photoPath)
        {
            _backgroundPhoto = photoPath;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_backgroundPhoto)));
        }
    }
}