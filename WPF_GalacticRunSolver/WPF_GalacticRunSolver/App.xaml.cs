using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPF_GalacticRunSolver.Utility;
using WPF_GalacticRunSolver.ViewModel;

namespace WPF_GalacticRunSolver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    public partial class App : Application
    {
        public const int CellSize = 40;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                //Open map from path
                _appMap = new MapViewModel(e.Args[0].ToString());
            }
            else
            {
                //create a new map
                _appMap = new MapViewModel(16);
            }
            _appMap._Map = Utilities.GetMapFromWeburl("https://galactic.run/p/23175690829143925");
            MainWindow wnd = new MainWindow(_appMap);
            wnd.Show();
        }

        public MapViewModel _appMap;
    }
}
