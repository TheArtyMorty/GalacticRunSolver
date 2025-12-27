using SolverApp.Models;
using SolverApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Android.Test.Suitebuilder.Annotation;

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
        public event PropertyChangedEventHandler? PropertyChanged = (sender, e) => { };

#pragma warning disable CS8618
        public SolverViewModel(SolverPage solverPage)
        {
            _solverPage = solverPage;
            logger = new AppLogger(BackwardLogValue, () => _Log = "");
            LoadMapFromAutosave();
            _SolveMap = new Command(SolveMap);

            _ResetWalls = new Command(ResetWalls);

            //background Worker
            _worker = new BackgroundWorker();
            _worker.WorkerSupportsCancellation = true;
            _worker.WorkerReportsProgress = false;
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }
#pragma warning restore CS8618

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
                    if (PropertyChanged != null)
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
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Log)));
                }
            }
        }

        private void Clear()
        {
            logger.Clear();
            _Solutions = new ObservableCollection<SolutionViewModel>();
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Solutions)));
        }


        private void ResetWalls()
        {
            theMap.ResetWalls();
            Clear();
        }

        public Command _SolveMap { get; }
        public Command _ResetWalls { get; }

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
                theMap._InitialMap = new Models.Map(theMap._Map);
                if (PropertyChanged != null)
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

        public bool solverModeOn = false;
        public bool _SolverModeOn
        {
            get
            {
                return solverModeOn;
            }
            set
            {
                if (solverModeOn == value) return;
                else
                {
                    solverModeOn = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_SolverModeOn)));
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_SolverModeOff)));
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_PanEnabled)));
                    }
                }
            }
        }

        public bool _SolverModeOff { get { return !solverModeOn; } }

        public bool panModeOn = true;
        public bool _PanModeOn
        {
            get
            {
                return panModeOn;
            }
            set
            {
                if (panModeOn == value) return;
                else
                {
                    panModeOn = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_PanModeOn)));
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_PanEnabled)));
                    }
                }
            }
        }

        public bool _PanEnabled { get { return solverModeOn || _PanModeOn; } }

        public string _SolveButtonText { get { return _worker.IsBusy ? "Stop" : "Solve"; } }
        public bool _SolveButtonEnabled { get { return !simulationRunning; } }

        public void PlaySolution(Solution solution, Action onFinished)
        {
            if (!simulationRunning)
            {
                simulationRunning = true;
                if (PropertyChanged != null)
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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_SolveButtonEnabled)));
            foreach (var svm in _Solutions)
            {
                svm.Refresh();
            }
            _toDo();
        }

        void worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var solutions = Solver.Solve(theMap._Map, logger, ref _worker);
            e.Result = solutions;
        }

        void worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
                return;
            var workerResult = (List<Solution>)e.Result;
            ObservableCollection<SolutionViewModel> result = new ObservableCollection<SolutionViewModel>
                (workerResult.Select(solution => new SolutionViewModel(solution, this)));

            _Solutions = result;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Solutions)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_SolveButtonText)));
            }
        }


        private static int defaultSize = 325;
        public int _ZoomSize { get; set; } = defaultSize;
        internal void ZoomInOrOut(double v, double mapControlWidth)
        {
            _ZoomSize = (int)(-50 + mapControlWidth + v * mapControlWidth);
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_ZoomSize)));
        }
        
        public string _backgroundPhoto {  get; set; }
        internal void SetBackgroundImage(string photoPath)
        {
            _backgroundPhoto = photoPath;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_backgroundPhoto)));
        }

        internal void SetQuadrant(string quadrant, string board, int editionIndex)
        {
            theMap.SetQuadrant(quadrant, board, editionIndex);
        }
    }
}