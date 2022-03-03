using SolverApp.Models;
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

        public SolverViewModel()
        {
            logger = new AppLogger(BackwardLogValue, () => _Log = "");
            theMap = new MapViewModel(16);
            _SolveMap = new Command(SolveMap);
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
        public void SolveMap()
        {
            Clear();
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var solutions = Solver.Solve(theMap._Map, logger);
            e.Result = solutions;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var workerResult = (List<Solution>)e.Result;
            ObservableCollection<SolutionViewModel> result = new ObservableCollection<SolutionViewModel>
                (workerResult.Select(solution => new SolutionViewModel(solution)));

            _Solutions = result;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Solutions)));
        }
    }
}