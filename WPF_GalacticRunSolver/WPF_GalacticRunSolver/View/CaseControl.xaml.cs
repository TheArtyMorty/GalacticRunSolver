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
    /// Interaction logic for CaseControl.xaml
    /// </summary>
    public partial class CaseControl : UserControl
    {
        public CaseControl()
        {
            InitializeComponent();
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(RobotViewModel)) &&
                !e.Data.GetDataPresent(typeof(TargetViewModel)))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(RobotViewModel)))
            {
                // do whatever you want do with the dropped element
                RobotViewModel droppedRobot = e.Data.GetData(typeof(RobotViewModel)) as RobotViewModel;
                ((CaseViewModel)this.DataContext).Drop(droppedRobot);
            }
            else if (e.Data.GetDataPresent(typeof(TargetViewModel)))
            {
                // do whatever you want do with the dropped element
                TargetViewModel droppedTarget = e.Data.GetData(typeof(TargetViewModel)) as TargetViewModel;
                ((CaseViewModel)this.DataContext).Drop(droppedTarget);
            }
        }
    }
}
