using SolverApp.Models;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace SolverApp.ViewModels
{
    public class RobotViewModel : INotifyPropertyChanged, IComparable
    {
        public event PropertyChangedEventHandler? PropertyChanged = (sender, e) => { };

        static int idIncrement = 0;

        public int ID = 0;
        public RobotViewModel(Robot robot)
        {
            _Robot = robot;
            ID = idIncrement++;
        }

        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;
            RobotViewModel other = (RobotViewModel)obj;
            if (other._Robot._Position.X > _Robot._Position.X)
            {
                return -1;
            }
            else if (other._Robot._Position.X < _Robot._Position.X)
            {
                return 1;
            }
            else
            {
                return _Robot._Position.Y - other._Robot._Position.Y;
            }
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
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Position)));
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_X)));
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Y)));
                    }
                }
            }
        }

        public int _X
        {
            get
            {
                return _Robot._Position.X;
            }
        }
        
        public int _Y
        {
            get
            {
                return _Robot._Position.Y;
            }
        }

        public void MoveTo(Position pos)
        {
            _Position = pos;
        }

        public void IncrementMove(Position increment)
        {
            _Position = new Position(_Position.X + increment.X, _Position.Y + increment.Y);
        }
    }
}
