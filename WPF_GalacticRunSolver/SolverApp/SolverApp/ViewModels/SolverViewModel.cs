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

            _SolveMap = new Command(SolveMap);

            testSolutions = new ObservableCollection<SolutionViewModel>();
        }

        public ObservableCollection<SolutionViewModel> testSolutions { get; set; }

        public MapViewModel theMap { get; set; }



        public Command _SolveMap { get; }
        public void SolveMap()
        {
            ObservableCollection<SolutionViewModel> result =
                new ObservableCollection<SolutionViewModel>
                (Solver.Solve(theMap._Map).Select(solution => new SolutionViewModel(solution)));
            testSolutions = result;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(testSolutions)));
        }
    }
}