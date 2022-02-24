using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows.Input;
using SolverApp.Models;

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

        public SolutionViewModel()
        {
        }

        public SolutionViewModel(Solution solution)
        {
            _Moves = new ObservableCollection<MoveViewModel>
                (solution.moves.Select(move => new MoveViewModel(move)));
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
