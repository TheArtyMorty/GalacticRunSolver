using SolverApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SolverApp.ViewModels
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
                _Map = new Map(8);
                _InitialMap = new Map(_Map);
            }
        }

        public MapViewModel(Map map)
        {
            _Map = map;
            _InitialMap = map;
        }

        public MapViewModel(int mapSize, int robotsCount = 4)
        {
            _Map = new Map(mapSize, robotsCount);
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

        public async void PlaySolution(Solution solution, Action onFinished)
        {
            Reset();
            await Task.Delay(300);
            foreach (Move move in solution.moves)
            {
                if (!Cancel)
                {
                    await PlayMove(move);
                    await Task.Delay(200);
                }
            }
            Reset();
            Cancel = false;
            onFinished();
        }

        private bool Cancel = false;

        public void StopPlaying()
        {
            Cancel = true;
        }

        private Position GetMoveIncrement(Move move)
        {
            switch(move.direction)
            {
                case EMoveDirection.Up:
                    return new Position(0, -1);
                case EMoveDirection.Down:
                    return new Position(0, 1);
                case EMoveDirection.Right:
                    return new Position(1, 0);
                case EMoveDirection.Left:
                default:
                    return new Position(-1, 0);
            }
        }

        public async Task PlayMove(Move move, int delay = 25)
        {
            var robot = _Robots.Where(r => r._Color == move.color).First()._Robot;
            var increment = GetMoveIncrement(move);

            while (_Map.CanRobotMove(robot, move.direction) && !Cancel)
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
