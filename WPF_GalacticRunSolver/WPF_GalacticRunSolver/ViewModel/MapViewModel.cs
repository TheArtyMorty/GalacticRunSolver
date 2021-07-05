using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WPF_GalacticRunSolver.Model;

namespace WPF_GalacticRunSolver.ViewModel
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public MapViewModel(String path)
        {
            if (System.IO.File.Exists(path))
            {
                _Map = new Map(path);
                _InitialMap = new Map(_Map);
            }
            else
            {
                _Map = new Map(16);
                _InitialMap = new Map(_Map);
            }
        }

        public MapViewModel(Map map)
        {
            _Map = map;
            _InitialMap = map;
        }

        public MapViewModel(int mapSize)
        {
            _Map = new Map(mapSize);
            _InitialMap = new Map(_Map);
        }

        public void SaveMap(string path)
        {
            _Map.Export(path);
        }

        public void Reset()
        {
            _Map = new Map(_InitialMap);
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Target)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Cases)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
        }

        public async void PlaySolution(SolutionViewModel solution)
        {
            Reset();
            await Task.Delay(300);
            foreach (MoveViewModel move in solution._Moves)
            {
                await PlayMove(move);
                await Task.Delay(200);
            }
        }

        private Position GetMoveIncrement(MoveViewModel move)
        {
            switch(move._Direction)
            {
                case CLI.EMoveDirection.Up:
                    return new Position(0, -1);
                case CLI.EMoveDirection.Down:
                    return new Position(0, 1);
                case CLI.EMoveDirection.Right:
                    return new Position(1, 0);
                case CLI.EMoveDirection.Left:
                default:
                    return new Position(-1, 0);
            }
        }

        public async Task PlayMove(MoveViewModel move, int delay = 25)
        {
            var robot = _Robots.Where(r => r._Color == (EColor)move._Color).First()._Robot;
            var increment = GetMoveIncrement(move);

            while (_Map.CanRobotMove(robot, move._Direction))
            {
                await Task.Delay(delay);
                robot.Move(increment);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
            }
        }

        public TargetViewModel _Target
        {
            get
            {
                return new TargetViewModel(_Map._Target);
            }
        }

        public ObservableCollection<RobotViewModel> _Robots
        {
            get
            {
                ObservableCollection<RobotViewModel> result =
                    new ObservableCollection<RobotViewModel>
                    (_Map._Robots.Select(robot => new RobotViewModel(robot)));
                return result;
            }
        }

        public ObservableCollection<ObservableCollection<CaseViewModel>> _Cases
        {
            get
            {
                ObservableCollection<ObservableCollection<CaseViewModel>> result =
                    new ObservableCollection<ObservableCollection<CaseViewModel>>
                    (_Map._Cases.Select(row => new ObservableCollection<CaseViewModel>(
                        row.Select(lacase => new CaseViewModel(lacase)))));
                return result;
            }
        }

        public Map _Map {get; set;}
        public Map _InitialMap {get; set;}

    }
}
