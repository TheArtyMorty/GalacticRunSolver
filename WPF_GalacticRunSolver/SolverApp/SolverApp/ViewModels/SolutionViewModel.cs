using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows.Input;
using SolverApp.Models;
using Xamarin.Essentials;

namespace SolverApp.ViewModels
{
    public class MoveViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public MoveViewModel(EColor c, EMoveDirection m)
        {
            _Color = c;
            _Direction = m;
        }
        public MoveViewModel(Move m)
        {
            _Color = m.color;
            _Direction = m.direction;
        }


        public EColor color;
        public EColor _Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Color)));
            }
        }

        public EMoveDirection direction;
        public EMoveDirection _Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Direction)));
            }
        }
    }

    public class SolutionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public ICommand _PlaySolution { get; }
        public void PlaySolution()
        {
            _IsPlaying = true;
            Refresh();
            _solverVM.PlaySolution(_solution, OnFinished);
        }
        private void OnFinished()
        {
            _IsPlaying = false;
            Refresh();
        }
        public ICommand _StopSolution { get; }
        public void StopSolution()
        {
            _IsPlaying = false;
            Refresh();
            _solverVM.StopSolution();
        }

        public void Refresh()
        {
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_IsPlaying)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_IsNotPlaying)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Enabled)));
        }

        SolverViewModel _solverVM;

        public bool _IsPlaying { get; set; } = false;
        public bool _IsNotPlaying { get { return !_IsPlaying; } }

        public bool _Enabled { get { return _IsPlaying || !_solverVM.simulationRunning; } }

        private Solution _solution;

        public SolutionViewModel(Solution solution, SolverViewModel solverVM)
        {
            _solution = solution;
            _Moves = new ObservableCollection<MoveViewModel>
                (solution.moves.Select(move => new MoveViewModel(move)));
            _solverVM = solverVM;
            _PlaySolution = new RelayCommand(PlaySolution);
            _StopSolution = new RelayCommand(StopSolution);
        }

        public ObservableCollection<MoveViewModel> moves = new ObservableCollection<MoveViewModel>();
        public ObservableCollection<MoveViewModel> _Moves
        {
            get
            {
                return moves;
            }
            set
            {
                if (value != moves)
                {
                    moves = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Moves)));
                }
            }
        }
    }
}
