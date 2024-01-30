using SolverApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
                        MapGrid.Children.Add(caseControl, i, j);
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
                    RobotControl robot = new RobotControl();
                    robot.BindingContext = robotVM;
                    robot.SetBinding(Grid.RowProperty, "_Y");
                    robot.SetBinding(Grid.ColumnProperty, "_X");
                    MapGrid.Children.Add(robot);
                }
            }
        }
    }
}