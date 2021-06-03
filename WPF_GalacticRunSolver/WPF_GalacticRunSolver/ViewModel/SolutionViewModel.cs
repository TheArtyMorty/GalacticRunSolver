using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_GalacticRunSolver.Model;
using CLI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WPF_GalacticRunSolver.ViewModel
{
    public class MoveViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public MoveViewModel(ManagedMove move)
        {
            _Move = move;
        }

        public ManagedMove _Move { get; }

        public ERobotColor _Color
        {
            get
            {
                return _Move.Color;
            }
        }

        public EMoveDirection _Direction
        {
            get
            {
                return _Move.Move;
            }
        }
    }

    public class SolutionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public SolutionViewModel(ManagedSolution solution, MapViewModel map)
        {
            _Solution = solution;
            _Map = map;
            _PlaySolution = new RelayCommand(PlaySolution);
        }

        public ManagedSolution _Solution { get; }
        private MapViewModel _Map {get;}

        public ICommand _PlaySolution { get; }
        public void PlaySolution()
        {
            _Map.PlaySolution(this);
        }

        public ObservableCollection<MoveViewModel> _Moves
        {
            get
            {
                ObservableCollection<MoveViewModel> result =
                new ObservableCollection<MoveViewModel>
                (_Solution.Moves.Select(move => new MoveViewModel(move)));
                return result;
            }
        }
    }

    public class SolutionsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public SolutionsViewModel(List<ManagedSolution> solutions, MapViewModel map)
        {
            _ManagedSolutions = solutions;
            _Map = map;
        }

        private MapViewModel _Map { get; }

        public List<ManagedSolution> _ManagedSolutions { get; }

        public ObservableCollection<SolutionViewModel> _Solutions
        {
            get
            {
                ObservableCollection<SolutionViewModel> result =
                new ObservableCollection<SolutionViewModel>
                (_ManagedSolutions.Select(solution => new SolutionViewModel(solution, _Map)));
                return result;
            }
        }
    }
}
