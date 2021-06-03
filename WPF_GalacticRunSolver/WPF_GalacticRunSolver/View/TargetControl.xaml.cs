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
    /// Interaction logic for TargetControl.xaml
    /// </summary>
    public partial class TargetControl : UserControl
    {
        public TargetControl()
        {
            InitializeComponent();
        }

        private void OnStartDrag(object sender, MouseButtonEventArgs e)
        {
            TargetViewModel item = (TargetViewModel)this.DataContext;

            if (item != null)
            {
                DragDrop.DoDragDrop(this, item, DragDropEffects.Move);
            }
        }
    }
}
