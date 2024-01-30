using SolverApp.Models;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace SolverApp.ViewModels
{
    public class TargetViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public TargetViewModel(Target target)
        {
            _Target = target;
            _IncrementColor = new Command(IncrementColor);
            _DecrementColor = new Command(DecrementColor);
        }

        public Target _Target { get; }

        public EColor _Color
        {
            get
            {
                return _Target._Color;
            }
            set
            {
                if (_Target._Color == value) return;
                else
                {
                    _Target._Color = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Color)));
                }
            }
        }

        public ICommand _IncrementColor { get; }
        public ICommand _DecrementColor { get; }

        public void IncrementColor()
        {
            int type = (int)_Color;
            type++;
            if (type >= Enum.GetNames(typeof(EColor)).Length)
            {
                type = 0;
            }
            _Color = (EColor)type;
        }

        public void DecrementColor()
        {
            int type = (int)_Color;
            type--;
            if (type < 0)
            {
                type = Enum.GetNames(typeof(EColor)).Length - 1;
            }
            _Color = (EColor)type;
        }

        public Position _Position
        {
            get
            {
                return _Target._Position;
            }
            set
            {
                if (_Target._Position == value) return;
                else
                {
                    _Target._Position = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Position)));
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_X)));
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(_Y)));
                }
            }
        }

        public int _X
        {
            get
            {
                return _Target._Position.X;
            }
        }

        public int _Y
        {
            get
            {
                return _Target._Position.Y;
            }
        }

        public void MoveTo(Position pos)
        {
            _Position = pos;
        }

    }
}
