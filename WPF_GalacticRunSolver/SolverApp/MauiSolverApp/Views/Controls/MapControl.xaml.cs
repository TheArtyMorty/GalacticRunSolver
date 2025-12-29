using SolverApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapControl : ContentView
    {
        public MapControl()
        {
            InitializeComponent();
        }

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
                        var caseControl = new CaseControl();
                        var theCase = theMap._Cases[j][i];
                        caseControl.BindingContext = theCase;
                        MapGrid.Add(caseControl, i, j);
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

        internal void UpdateSize(int sliderCorrectValue)
        {
            int currentSize = MapGrid.RowDefinitions.Count;
            if (sliderCorrectValue > currentSize)
            {
                for (int i = currentSize; i < sliderCorrectValue; i++)
                {
                    MapGrid.RowDefinitions.Add(new RowDefinition());
                    MapGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
            }
            else if (sliderCorrectValue < currentSize)
            {
                for (int i = currentSize - 1; i >= sliderCorrectValue; i--)
                {
                    MapGrid.RowDefinitions.RemoveAt(i);
                    MapGrid.ColumnDefinitions.RemoveAt(i);
                }
            }
        }
    }
}