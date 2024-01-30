using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace WPF_GalacticRunSolver.Model
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
        Yellow,
        Gray
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
        public Map(int mapSize)
        {
            _Size = mapSize;
            _Robots.Clear();
            _Robots.Add(new Robot(EColor.Red, new Position(0, 0)));
            _Robots.Add(new Robot(EColor.Green, new Position(0, 1)));
            _Robots.Add(new Robot(EColor.Blue, new Position(0, 2)));
            _Robots.Add(new Robot(EColor.Yellow, new Position(0, 3)));
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
            foreach(Robot r in map._Robots)
            {
                _Robots.Add(new Robot(r));
            }
            _Cases = new ObservableCollection<ObservableCollection<Case>> ();
            foreach(ObservableCollection<Case> row in map._Cases)
            {
                var newRow = new ObservableCollection<Case>();
                foreach(Case c in row)
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

        private bool IsEmpty(Case c)
        {
            return _Robots.Where(r => r._Position.X == c._Position.X && r._Position.Y == c._Position.Y).Count() == 0;
        }

        public bool CanRobotMove(Robot robot, CLI.EMoveDirection move)
        {
            Case A = _Cases[robot._Position.Y][robot._Position.X];
            switch (move)
            {
                case CLI.EMoveDirection.Up:
                    {
                        if (A._WallType == EWallType.TopLeft || A._WallType == EWallType.TopRight) return false;
                        if (A._Position.Y == 0) return false;
                        Case B = _Cases[robot._Position.Y - 1][robot._Position.X];
                        if (B._WallType == EWallType.BottomLeft || B._WallType == EWallType.BottomRight) return false;
                        if (!IsEmpty(B)) return false;
                        break;
                    }
                case CLI.EMoveDirection.Down:
                    {
                        if (A._WallType == EWallType.BottomRight || A._WallType == EWallType.BottomLeft) return false;
                        if (A._Position.Y == _Size-1) return false;
                        Case B = _Cases[robot._Position.Y + 1][robot._Position.X];
                        if (B._WallType == EWallType.TopLeft || B._WallType == EWallType.TopRight) return false;
                        if (!IsEmpty(B)) return false;
                        break;
                    }
                case CLI.EMoveDirection.Left:
                    {
                        if (A._WallType == EWallType.TopLeft || A._WallType == EWallType.BottomLeft) return false;
                        if (A._Position.X == 0) return false;
                        Case B = _Cases[robot._Position.Y][robot._Position.X - 1];
                        if (B._WallType == EWallType.BottomRight || B._WallType == EWallType.TopRight) return false;
                        if (!IsEmpty(B)) return false;
                        break;
                    }
                case CLI.EMoveDirection.Right:
                    {
                        if (A._WallType == EWallType.TopRight || A._WallType == EWallType.BottomRight) return false;
                        if (A._Position.X == _Size-1) return false;
                        Case B = _Cases[robot._Position.Y][robot._Position.X + 1];
                        if (B._WallType == EWallType.BottomLeft || B._WallType == EWallType.TopLeft) return false;
                        if (!IsEmpty(B)) return false;
                        break;
                    }
            }
            return true;
        }
    }
}