using SolverApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using MauiSolverApp.Utilities;

namespace SolverApp.ViewModels
{
    public class MapViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = (sender, e) => { };

        static int previousID = 0;
        private int ID = 0;

#pragma warning disable CS8618
        public MapViewModel(String path)
        {
            if (System.IO.File.Exists(path))
            {
                _Map = new Models.Map(path);
                _InitialMap = new Models.Map(_Map);
            }
            else
            {
                _Map = new Models.Map(16);
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
#pragma warning restore CS8618

        private void InitObjects()
        {
            _Cases = new ObservableCollection<ObservableCollection<CaseViewModel>>
                    (_Map._Cases.Select(row => new ObservableCollection<CaseViewModel>(
                        row.Select(lacase => new CaseViewModel(lacase)))));
            _Robots = new ObservableCollection<RobotViewModel>
                    (_Map._Robots.Select(robot => new RobotViewModel(robot)));
            _Target = new TargetViewModel(_Map._Target);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Target)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Cases)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
            }
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

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Target)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Cases)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
            }
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
            switch (move.direction)
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
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
                }
            }
        }

        internal void ResetWalls()
        {
            foreach (var row in _Cases)
            {
                foreach (CaseViewModel caseVM in row)
                {
                    caseVM._WallType = EWallType.None;
                }
            }
        }

        internal void ResetQuadrant(string quadrant)
        {
            int minX = 0;
            int maxX = 16;
            int minY = 0;
            int maxY = 16;
            switch (quadrant)
            {
                case "TopLeft":
                    minX = 0;
                    minY = 0;
                    maxY = 8;
                    maxX = 8;
                    break;
                case "TopRight":
                    minX = 8;
                    minY = 0;
                    maxY = 8;
                    maxX = 16;
                    break;
                case "BottomLeft":
                    minX = 0;
                    minY = 8;
                    maxY = 16;
                    maxX = 8;
                    break;
                case "BottomRight":
                    minX = 8;
                    minY = 8;
                    maxY = 16;
                    maxX = 16;
                    break;
                default:
                    break;
            }

            foreach (var row in _Cases)
            {
                foreach (CaseViewModel caseVM in row)
                {
                    if (caseVM._Case._Position.X >= minX && caseVM._Case._Position.X < maxX &&
                        caseVM._Case._Position.Y >= minY && caseVM._Case._Position.Y < maxY)
                        caseVM._WallType = EWallType.None;
                }
            }
        }

        internal void SetQuadrant(string quadrant, string board, int editionIndex)
        {
            ResetQuadrant(quadrant);

            var wallsToSet = BoardUtilities.GetWallsForQuadrant(board, editionIndex);

            for (int i = 0; i < wallsToSet.Count; i++)
            {
                switch (quadrant)
                {
                    case "TopRight":
                        RotateRight(ref wallsToSet, i);
                        break;
                    case "BottomLeft":
                        RotateLeft(ref wallsToSet, i);
                        break;
                    case "BottomRight":
                        RotateTwice(ref wallsToSet, i);
                        break;
                    case "TopLeft":
                    default:
                        break;
                }
            }

            foreach (var wall in wallsToSet)
            {
                SetCase(wall.Item1, wall.Item2, wall.Item3);
            }
        }

        internal void RotateRight(ref List<Tuple<int, int, EWallType>> wallsToSet, int i)
        {
            var toRotate = wallsToSet[i];
            EWallType wallType = toRotate.Item3;
            int newX = 15 - toRotate.Item1;
            int newY = toRotate.Item2;
            EWallType newWallType;
            if (wallType == EWallType.BottomLeft)
            {
                newWallType = EWallType.TopLeft;
            }
            else
            {
                newWallType = wallType + 1;
            }
            wallsToSet[i] = new Tuple<int, int, EWallType>(newY, newX, newWallType);
        }

        internal void RotateLeft(ref List<Tuple<int, int, EWallType>> wallsToSet, int i)
        {
            var toRotate = wallsToSet[i];
            EWallType wallType = toRotate.Item3;
            int newX = toRotate.Item1;
            int newY = 15 - toRotate.Item2;
            EWallType newWallType;
            if (wallType == EWallType.TopLeft)
            {
                newWallType = EWallType.BottomLeft;
            }
            else
            {
                newWallType = wallType - 1;
            }
            wallsToSet[i] = new Tuple<int, int, EWallType>(newY, newX, newWallType);
        }

        internal void RotateTwice(ref List<Tuple<int, int, EWallType>> wallsToSet, int i)
        {
            var toRotate = wallsToSet[i];
            int newX = 15 - toRotate.Item2;
            int newY = 15 - toRotate.Item1;
            EWallType newWallType = EWallType.None;
            switch (toRotate.Item3)
            {
                case EWallType.TopLeft:
                    newWallType = EWallType.BottomRight;
                    break;
                case EWallType.TopRight:
                    newWallType = EWallType.BottomLeft;
                    break;
                case EWallType.BottomLeft:
                    newWallType = EWallType.TopRight;
                    break;
                case EWallType.BottomRight:
                    newWallType = EWallType.TopLeft;
                    break;
            }
            wallsToSet[i] = new Tuple<int, int, EWallType>(newY, newX, newWallType);
        }

        internal void SetCase(int x, int y, EWallType wallType)
        {
            _Cases[x][y]._WallType = wallType;
        }

        public TargetViewModel _Target { get; set; }

        public ObservableCollection<RobotViewModel> _Robots { get; set; }

        public ObservableCollection<ObservableCollection<CaseViewModel>> _Cases { get; set; }

        public Models.Map _Map { get; set; }
        public Models.Map _InitialMap { get; set; }


        private Position GetFirstAvailablePosition()
        {
            for (int y = 0; y < 5; y++)
            {
                if (_Map._Robots.All(r => r._Position != new Position(0, y)))
                {
                    return new Position(0, y);
                }
            }
            return new Position(0, 0);
        }

        public void CreateAdditionalRobot()
        {
            if (_Map._Robots.Count < 5)
            {
                _Map._Robots.Add(new Robot(EColor.Gray, GetFirstAvailablePosition()));
                var newRobot = _Map._Robots.Last();
                var newRobotVM = new RobotViewModel(newRobot);
                _Robots.Add(newRobotVM);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
                }
            }
        }
        
        public void RemoveAdditionalRobot()
        {
            if (_Map._Robots.Count == 5)
            {
                _Robots.Remove(_Robots.Where(r => r._Color == EColor.Gray).First());
                _Map._Robots.Remove(_Map._Robots.Where(r => r._Color == EColor.Gray).First());
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Robots)));
                }
            }
        }


    }
}
