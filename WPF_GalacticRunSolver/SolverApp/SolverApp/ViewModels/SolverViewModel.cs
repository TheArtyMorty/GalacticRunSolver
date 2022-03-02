using SolverApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            theMap = new MapViewModel(16);

            _SolveMap = new Command(SolveMap);

            testSolutions = new ObservableCollection<SolutionViewModel>();

            logger = new AppLogger(BackwardLogValue, () => _Log = "");
        }

        void BackwardLogValue(string value)
        {
            var currentContent = _Log;
            _Log = value;
            _Log += currentContent;
        }

        public ObservableCollection<SolutionViewModel> testSolutions { get; set; }

        public MapViewModel theMap { get; set; }

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

        public Command _SolveMap { get; }
        public void SolveMap()
        {
            testSolutions = new ObservableCollection<SolutionViewModel>();
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

            testSolutions = result;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(testSolutions)));
        }
    }
}