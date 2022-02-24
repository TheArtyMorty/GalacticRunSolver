using SolverApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace SolverApp.ViewModels
{
    public class SolverViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public SolverViewModel()
        {
            theMap = new MapViewModel(16);
            testTarget = theMap._Target;
            _TEST = new Command(TEST);
            testItems = theMap._Cases;
            testRobots = theMap._Robots;

            testSolutions = new ObservableCollection<SolutionViewModel>();
        }

        public ObservableCollection<ObservableCollection<CaseViewModel>> testItems { get; set; }

        public ObservableCollection<RobotViewModel> testRobots { get; set; }

        public ObservableCollection<SolutionViewModel> testSolutions { get; set; }

        public MapViewModel theMap;
        public TargetViewModel testTarget { get; set; }

        public Command _TEST { get; }
        public void TEST()
        {
            ObservableCollection<SolutionViewModel> result =
                new ObservableCollection<SolutionViewModel>
                (Solver.Solve(theMap._Map).Select(solution => new SolutionViewModel(solution)));
            testSolutions = result;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(testSolutions)));
        }
    }
}