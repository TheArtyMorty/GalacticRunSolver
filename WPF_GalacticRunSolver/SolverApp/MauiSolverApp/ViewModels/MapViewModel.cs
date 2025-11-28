using SolverApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace SolverApp.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = (sender, e) => { };

        static int previousID = 0;
        private int ID = 0;

        public MapViewModel(String path)
        {
            if (System.IO.File.Exists(path))
            {
                _Map = new Models.Map(path);
                _InitialMap = new Models.Map(_Map);
            }
            else
            {
                _Map = new Models.Map(8);
                _InitialMap = new Models.Map(_Map);
            }
            ID = previousID++;
            InitObjects();
        }

        public MapViewModel(Models.Map map)
        {
            _Map = map;
            _InitialMap = new Models.Map(_Map);
            ID = previousID++;
            InitObjects();
        }

        public MapViewModel(int mapSize, int robotsCount = 4)
        {
            _Map = new Models.Map(mapSize, robotsCount);
            _InitialMap = new Models.Map(_Map);
            ID = previousID++;
            InitObjects();
        }

        private void InitObjects()
        {
            _Cases = new ObservableCollection<ObservableCollection<CaseViewModel>>
                    (_Map._Cases.Select(row => new ObservableCollection<CaseViewModel>(
                        row.Select(lacase => new CaseViewModel(lacase)))));
            _Robots = new ObservableCollection<RobotViewModel>
                    (_Map._Robots.Select(robot => new RobotViewModel(robot)));
            _Target = new TargetViewModel(_Map._Target);
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Target)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Cases)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
        }

        public void SaveMap(string path)
        {
            _Map.Export(path);
        }

        public void Reset()
        {
            for (int i = 0; i < _InitialMap._Size; i++)
            {
                for (int j = 0; j < _InitialMap._Size; j++)
                {
                    _Map._Cases[i][j]._WallType = _InitialMap._Cases[i][j]._WallType;
                }
            }
            for (int i = 0; i < _InitialMap._Robots.Count; i++)
            {
                _Robots[i]._Position = _InitialMap._Robots[i]._Position;
            }
            _Target._Position = _InitialMap._Target._Position;
            _Target._Color = _InitialMap._Target._Color;
            
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
            var robot = _Robots.Where(r => r._Color == move.color).First();
            var increment = GetMoveIncrement(move);

            while (_Map.CanRobotMove(robot._Robot, move.direction) && !Cancel)
            {
                await Task.Delay(delay);
                robot.IncrementMove(increment);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
            }
        }

        public TargetViewModel _Target {  get; set; }

        public ObservableCollection<RobotViewModel> _Robots { get; set; }

        public ObservableCollection<ObservableCollection<CaseViewModel>> _Cases { get; set; }

        public Models.Map _Map {get; set;}
        public Models.Map _InitialMap {get; set;}

    }
}
