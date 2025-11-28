using SolverApp.Models;
using SolverApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CaseControl : ContentView
    {
        public CaseControl()
        {
            InitializeComponent();
        }

        public void OnDrop(System.Object sender, Microsoft.Maui.Controls.DropEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.Properties.ContainsKey("Target"))
                {
                    var target = e.Data.Properties["Target"] as TargetViewModel;
                    if (target != null)
                        ((CaseViewModel)this.BindingContext).Drop(target);
                }
                if (e.Data.Properties.ContainsKey("Robot"))
                {
                    var robot = e.Data.Properties["Robot"] as RobotViewModel;
                    if (robot != null)
                        ((CaseViewModel)this.BindingContext).Drop(robot);
                }
            }
        }
    }
}