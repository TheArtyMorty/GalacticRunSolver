using SolverApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using SolverApp.Models;
using System.Collections.ObjectModel;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapControl : ContentView
    {
        public MapControl()
        {
            InitializeComponent();

            sizeToCaseControl = new Dictionary<int, List<CaseControl>>();
            for (int i = 8; i <= 20; i++)
                sizeToCaseControl.Add(i, new List<CaseControl>());
        }

        Dictionary<int, List<CaseControl>> sizeToCaseControl;

        public void GenerateMap(MapViewModel theMap)
        {
            // RESET
            MapGrid.Children.Clear();
            MapGrid.RowDefinitions.Clear();
            MapGrid.ColumnDefinitions.Clear();

            // Set up
            if (theMap != null)
            {
                for (int i = 0; i < theMap._Map._Size; i++)
                {
                    MapGrid.RowDefinitions.Add(new RowDefinition());
                    MapGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                //Set each case
                for (int i = 0; i < theMap._Map._Size; i++)
                {
                    for (int j = 0; j < theMap._Map._Size; j++)
                    {
                        CaseViewModel theCase = theMap._Cases[j][i];
                        AddCase(theCase);
                    }
                }
                // set target
                TargetControl target = new TargetControl();
                target.BindingContext = theMap._Target;
                target.SetBinding(Grid.RowProperty, "_Y");
                target.SetBinding(Grid.ColumnProperty, "_X");
                MapGrid.Children.Add(target);
                // set robots
                foreach (var robotVM in theMap._Robots)
                {
                    AddRobot(robotVM);
                }
            }
        }

        public void AddCase(CaseViewModel theCase)
        {
            var caseControl = new CaseControl();
            caseControl.BindingContext = theCase;
            var i = theCase._Case._Position.X;
            var j = theCase._Case._Position.Y;
            MapGrid.Add(caseControl, i, j);
            // store for reuse
            var sizeToAdd = Math.Max(8, Math.Max(i + 1, j + 1));
            sizeToCaseControl[sizeToAdd].Add(caseControl);
        }

        public void AddRobot(RobotViewModel robotVM)
        {
            RobotControl robot = new RobotControl();
            robot.BindingContext = robotVM;
            robot.SetBinding(Grid.RowProperty, "_Y");
            robot.SetBinding(Grid.ColumnProperty, "_X");
            MapGrid.Children.Add(robot);
        }

        public void RemoveAdditionalRobot()
        {
            var childToRemove = MapGrid.Children.OfType<RobotControl>().Where(r => ((RobotViewModel)r.BindingContext)._Color == Models.EColor.Gray);
            if (childToRemove.Count() > 0)
            {
                var robotToRemove = childToRemove.Last();
                int indexToRemove = MapGrid.Children.IndexOf(robotToRemove);
                MapGrid.Children.RemoveAt(indexToRemove);
            }
        }

        internal void UpdateSize(MapViewModel theMap)
        {
            var newSize = theMap._Map._Size;
            int currentSize = MapGrid.RowDefinitions.Count;
            if (newSize > currentSize)
            {
                for (int i = currentSize; i < newSize; i++)
                {
                    MapGrid.RowDefinitions.Add(new RowDefinition());
                    MapGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                //Handle adding cases
                for (int i = 0; i < currentSize; i++)
                {
                    for (int j = currentSize; j < newSize; j++)
                    {
                        CaseViewModel theCase = theMap._Cases[j][i];
                        AddCase(theCase);
                    }
                }
                for (int i = currentSize; i < newSize; i++)
                {
                    for (int j = 0; j < newSize; j++)
                    {
                        CaseViewModel theCase = theMap._Cases[j][i];
                        AddCase(theCase);
                    }
                }
            }
            else if (newSize < currentSize)
            {
                for (int i = currentSize - 1; i >= newSize; i--)
                {
                    MapGrid.RowDefinitions.RemoveAt(i);
                    MapGrid.ColumnDefinitions.RemoveAt(i);
                    // Remove case controls
                    foreach (var caseControl in sizeToCaseControl[i+1])
                    {
                        MapGrid.Children.Remove(caseControl);
                    }
                    sizeToCaseControl[i+1].Clear();
                }
            }
        }
    }
}