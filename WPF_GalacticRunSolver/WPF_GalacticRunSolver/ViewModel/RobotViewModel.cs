using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.ViewModel
{
    public class RobotViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public RobotViewModel(Robot robot)
        {
            _Robot = robot;
        }

        public Robot _Robot { get; }

        public EColor _Color 
        {
            get
            {
                return _Robot._Color;
            }
        }

        public Position _Position
        {
            get
            {
                return _Robot._Position;
            }
            set
            {
                if (_Robot._Position == value) return;
                else
                {
                    _Robot._Position = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Position)));
                }
            }
        }

        public void MoveTo(Position pos)
        {
            _Position = pos;
        }
    }
}
