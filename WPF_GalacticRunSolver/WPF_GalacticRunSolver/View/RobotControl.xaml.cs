using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_GalacticRunSolver.ViewModel;

namespace WPF_GalacticRunSolver.View
{
    /// <summary>
    /// Interaction logic for RobotControl.xaml
    /// </summary>
    public partial class RobotControl : UserControl
    {
        public RobotControl()
        {
            InitializeComponent();
        }

        private void OnStartDrag(object sender, MouseButtonEventArgs e)
        {
            RobotViewModel item = (RobotViewModel)this.DataContext;

            if (item != null)
            {
                DragDrop.DoDragDrop(this, item, DragDropEffects.Move);
            }
        }
    }
}
