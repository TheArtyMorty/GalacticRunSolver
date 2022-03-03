using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;


namespace SolverApp.Models
{
    public enum EWallType
    {
        None = 0,
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }

    public enum EColor
    {
        Red = 0,
        Green,
        Blue,
        Yellow
    }

    public enum EMoveDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    }

    public static class Utility
    {
        public static EWallType StringToWallType(string t)
        {
            if (t == "TR") return EWallType.TopRight;
            if (t == "BR") return EWallType.BottomRight;
            if (t == "BL") return EWallType.BottomLeft;
            if (t == "TL") return EWallType.TopLeft;
            return EWallType.None;
        }
    }
    

    public class Position
    {
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Position(Position p)
        {
            X = p.X;
            Y = p.Y;
        }

        public int X {get; set; }
        public int Y { get; set; }

        public bool Equals(Position p)
        {
            if (p is null)
            {
                return false;
            }
            else if (Object.ReferenceEquals(this, p))
            {
                return true;
            }
            else if (this.GetType() != p.GetType())
            {
                return false;
            }
            else return (X == p.X) && (Y == p.Y);
        }
        public static bool operator==(Position lhs, Position rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }
                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Position lhs, Position rhs) => !(lhs == rhs);
    }

    public class Case
    {
        public Case(Position pos)
        {
            _Position = pos;
            _WallType = EWallType.None;
        }

        public Case(Case c)
        {
            _Position = new Position(c._Position);
            _WallType = c._WallType;
        }

        public Position _Position { get; }
        public EWallType _WallType { get; set; }

        public string TypeToString()
        {
            switch (_WallType)
            {
                case EWallType.TopRight:
                    return "TR";
                case EWallType.BottomRight:
                    return "BR";
                case EWallType.BottomLeft:
                    return "BL";
                case EWallType.TopLeft:
                    return "TL";
                case EWallType.None:
                default:
                    return "..";
            }
        }
    }

    public class Target
    {
        public Target()
        {
            _Position = new Position(0,5);
            _Color = EColor.Green;
        }
        public Target(Target t)
        {
            _Position = new Position(t._Position);
            _Color = t._Color;
        }

        public Position _Position { get; set; }
        public EColor _Color { get; set; }
    }

    public class Robot
    {
        public Robot(EColor color, Position pos)
        {
            _Position = pos;
            _Color = color;
        }
        public Robot(Robot r)
        {
            _Position = new Position(r._Position);
            _Color = r._Color;
        }

        public void Move(Position increment)
        {
            _Position.X += increment.X;
            _Position.Y += increment.Y;
        }

        public Position _Position { get; set; }
        public EColor _Color { get; set; }
    }

    public class Map
    {
        #region constructors

        static EMoveDirection[] moveDirections = { EMoveDirection.Up, EMoveDirection.Down, EMoveDirection.Left, EMoveDirection.Right };

        public Tuple<int, int> NextCell(int x, int y, EMoveDirection direction)
        {
            switch (direction)
            {
                case EMoveDirection.Down:
                    return new Tuple<int, int>(x, y + 1);
                case EMoveDirection.Up:
                    return new Tuple<int, int>(x, y - 1);
                case EMoveDirection.Left:
                    return new Tuple<int, int>(x - 1, y);
                case EMoveDirection.Right:
                default:
                    return new Tuple<int, int>(x + 1, y);
            }
        }

        public void InitializeHeuristic()
        {
            //Initialize accessibles
            _AccessiblesCells.Clear();
            for (int i = 0; i < _Size; i++)
            {
                var line = new List<Dictionary<EMoveDirection, int>>();
                for (int j = 0; j < _Size; j++)
                {
                    var movesPerDirection = new Dictionary<EMoveDirection, int>();
                    foreach (EMoveDirection dir in moveDirections)
                    {
                        var max = 0;
                        var current = new Tuple<int, int>(j, i);
                        while (IsMoveValid(_Cases[current.Item2][current.Item1], dir, true))
                        {
                            var next = NextCell(current.Item1, current.Item2, dir);
                            current = next;
                        }
                        switch (dir)
                        {
                            case EMoveDirection.Up:
                            case EMoveDirection.Down:
                                max = current.Item2;
                                break;
                            case EMoveDirection.Right:
                            case EMoveDirection.Left:
                            default:
                                max = current.Item1;
                                break;
                        }
                        movesPerDirection.Add(dir, max);
                    }
                    line.Add(movesPerDirection);
                }
                _AccessiblesCells.Add(line);
            }
            //Initialize heuristic
            _Heuristic.Clear();
            int initValue = _Size * _Size;
            for (int i = 0; i < _Size; i++)
            {
                var line = new List<int>();
                for (int j = 0; j < _Size; j++)
                {
                    line.Add(initValue);
                }
                _Heuristic.Add(line);
            }
            _Heuristic[_Target._Position.Y][_Target._Position.X] = 0;

            Queue<Tuple<int, int>> previousCells = new Queue<Tuple<int, int>>();
            previousCells.Enqueue(new Tuple<int, int>(_Target._Position.X, _Target._Position.Y));

            while (previousCells.Count() > 0)
            {
                var cell = previousCells.Dequeue();
                var nextCellHValue = _Heuristic[cell.Item2][cell.Item1] + 1;

                foreach (var move in moveDirections)
                {
                    var current = cell;
                    var next = NextCell(current.Item1, current.Item2, move);
                    while (IsMoveValid(_Cases[current.Item2][current.Item1], move, true) &&
                           _Heuristic[next.Item2][next.Item1] >= nextCellHValue)
                    {
                        _Heuristic[next.Item2][next.Item1] = nextCellHValue;
                        previousCells.Enqueue(next);
                        current = next;
                        next = NextCell(current.Item1, current.Item2, move);
                    }
                }
            }
        }
        public Map(int mapSize)
        {
            _Size = mapSize;
            _Robots.Clear();
            _Robots.Add(new Robot(EColor.Red, new Position(0, 0)));
            _Robots.Add(new Robot(EColor.Green, new Position(0, mapSize - 1)));
            _Robots.Add(new Robot(EColor.Blue, new Position(mapSize - 1, mapSize - 1)));
            _Robots.Add(new Robot(EColor.Yellow, new Position(mapSize - 1, 0)));
            _Target = new Target();
            for (int i = 0; i < mapSize; i++)
            {
                ObservableCollection<Case> Column = new ObservableCollection<Case> { };
                for (int j = 0; j < mapSize; j++)
                {
                    Column.Add(new Case(new Position(j, i)));
                }
                _Cases.Add(Column);
            }
        }
        public Map(Map map)
        {
            _Size = map._Size;
            _Robots = new ObservableCollection<Robot>();
            foreach (Robot r in map._Robots)
            {
                _Robots.Add(new Robot(r));
            }
            _Cases = new ObservableCollection<ObservableCollection<Case>>();
            foreach (ObservableCollection<Case> row in map._Cases)
            {
                var newRow = new ObservableCollection<Case>();
                foreach (Case c in row)
                {
                    newRow.Add(new Case(c));
                }
                _Cases.Add(newRow);
            }
            _Target = new Target(map._Target);
        }
        public Map(string filepath)
        {
            string[] lines = System.IO.File.ReadAllLines(filepath);
            int pos = 0;
            _Size = Int32.Parse(lines[pos++]);
            //Create all cases empty
            _Cases = new ObservableCollection<ObservableCollection<Case>>();
            for (int i = 0; i < _Size; i++)
            {
                ObservableCollection<Case> Column = new ObservableCollection<Case>();
                string[] line = lines[pos++].Split(' ');
                for (int j = 0; j < _Size; j++)
                {
                    Case theCase = new Case(new Position(j, i));
                    theCase._WallType = Utility.StringToWallType(line[j]);
                    Column.Add(theCase);
                }
                _Cases.Add(Column);
            }
            pos++; //skip Line "Robots"
            int robotCount = Int32.Parse(lines[pos++]);
            _Robots = new ObservableCollection<Robot>();
            for (int i = 0; i < robotCount; i++)
            {
                string[] positions = lines[pos++].Split(' ');
                _Robots.Add(new Robot((EColor)i, new Position(Int32.Parse(positions[0]), Int32.Parse(positions[1]))));
            }
            pos++; //skip Line "Target"
            string[] targetInfo = lines[pos++].Split(' ');
            _Target = new Target();
            _Target._Position.X = Int32.Parse(targetInfo[0]);
            _Target._Position.Y = Int32.Parse(targetInfo[1]);
            _Target._Color = (EColor)Int32.Parse(targetInfo[2]);
        }
        #endregion

        #region Properties
        public int _Size { get; }
        public ObservableCollection<Robot> _Robots { get; } = new ObservableCollection<Robot> { };
        public Target _Target { get; set; }
        public ObservableCollection<ObservableCollection<Case>> _Cases { get; } = new ObservableCollection<ObservableCollection<Case>> { };

        public List<List<int>> _Heuristic { get; set; } = new List<List<int>> { };

        public List<List<Dictionary<EMoveDirection, int>>> _AccessiblesCells { get; set; } = new List<List<Dictionary<EMoveDirection, int>>>{};
        #endregion  

        public void Export(string filepath)
        {
            // Size
            List<string> lines = new List<string> { };
            // Cases
            lines.Add(_Size.ToString());
            for (int i = 0; i < _Size; i++)
            {
                string linei = "";
                for (int j = 0; j < _Size; j++)
                {
                    linei += _Cases[i][j].TypeToString() + " ";
                }
                lines.Add(linei);
            }
            // Robots
            lines.Add("Robots");
            lines.Add(_Robots.Count.ToString());
            foreach (Robot r in _Robots)
            {
                lines.Add(r._Position.X.ToString() + " " + r._Position.Y.ToString());
            }
            // Objective
            lines.Add("Target");
            lines.Add(_Target._Position.X.ToString() + " " + _Target._Position.Y.ToString() + " " + ((int)(_Target._Color)).ToString());
            // Write
            System.IO.File.WriteAllLines(filepath, lines);
        }

        public bool IsEmpty(Case c)
        {
            return _Robots.Where(r => r._Position.X == c._Position.X && r._Position.Y == c._Position.Y).Count() == 0;
        }

        public bool IsMoveValid(Case A, EMoveDirection move, bool ignoreRobots = false)
        {
            switch (move)
            {
                case EMoveDirection.Up:
                    {
                        if (A._WallType == EWallType.TopLeft || A._WallType == EWallType.TopRight) return false;
                        if (A._Position.Y == 0) return false;
                        Case B = _Cases[A._Position.Y - 1][A._Position.X];
                        if (B._WallType == EWallType.BottomLeft || B._WallType == EWallType.BottomRight) return false;
                        if (!ignoreRobots && !IsEmpty(B)) return false;
                        break;
                    }
                case EMoveDirection.Down:
                    {
                        if (A._WallType == EWallType.BottomRight || A._WallType == EWallType.BottomLeft) return false;
                        if (A._Position.Y == _Size - 1) return false;
                        Case B = _Cases[A._Position.Y + 1][A._Position.X];
                        if (B._WallType == EWallType.TopLeft || B._WallType == EWallType.TopRight) return false;
                        if (!ignoreRobots && !IsEmpty(B)) return false;
                        break;
                    }
                case EMoveDirection.Left:
                    {
                        if (A._WallType == EWallType.TopLeft || A._WallType == EWallType.BottomLeft) return false;
                        if (A._Position.X == 0) return false;
                        Case B = _Cases[A._Position.Y][A._Position.X - 1];
                        if (B._WallType == EWallType.BottomRight || B._WallType == EWallType.TopRight) return false;
                        if (!ignoreRobots && !IsEmpty(B)) return false;
                        break;
                    }
                case EMoveDirection.Right:
                    {
                        if (A._WallType == EWallType.TopRight || A._WallType == EWallType.BottomRight) return false;
                        if (A._Position.X == _Size - 1) return false;
                        Case B = _Cases[A._Position.Y][A._Position.X + 1];
                        if (B._WallType == EWallType.BottomLeft || B._WallType == EWallType.TopLeft) return false;
                        if (!ignoreRobots && !IsEmpty(B)) return false;
                        break;
                    }
            }
            return true;
        }

        //public bool CanRobotMove(Robot robot, EMoveDirection move)
        //{
        //    Case A = _Cases[robot._Position.Y][robot._Position.X];
        //    return IsMoveValid(A, move);
        //}

        public int GetHeuristicOfPosition(Position position)
        {
            return _Heuristic[position.Y][position.X];
        }

        public List<Robot> GetRobots()
        {
            var result = new List<Robot>();
            foreach(var r in _Robots)
            {
                result.Add(new Robot(r));
            }
            return result;
        }
    }
}