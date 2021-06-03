using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.ViewModel
{
    public class CaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public CaseViewModel(Case theCase)
        {
            _Case = theCase;
            _IncrementWallType = new RelayCommand(IncrementWallType);
            _DecrementWallType = new RelayCommand(DecrementWallType);
        }

        public Case _Case { get; }

        public EWallType _WallType
        {
            get 
            { 
                return _Case._WallType; 
            }
            set
            {
                if (_Case._WallType == value) return;
                else
                {
                    _Case._WallType = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_WallType)));
                }
            }
        }

        public ICommand _IncrementWallType { get; }
        public ICommand _DecrementWallType { get; }

        public void IncrementWallType()
        {
            int type = (int)_WallType;
            type++;
            if (type >= Enum.GetNames(typeof(EWallType)).Length)
            {
                type = 0;
            }
            _WallType = (EWallType)type;
        }

        public void DecrementWallType()
        {
            int type = (int)_WallType;
            type--;
            if (type < 0)
            {
                type = Enum.GetNames(typeof(EWallType)).Length - 1;
            }
            _WallType = (EWallType)type;
        }

        public void Drop(RobotViewModel robot)
        {
            robot.MoveTo(_Case._Position);
        }

        public void Drop(TargetViewModel target)
        {
            target.MoveTo(_Case._Position);
        }
    }
}
