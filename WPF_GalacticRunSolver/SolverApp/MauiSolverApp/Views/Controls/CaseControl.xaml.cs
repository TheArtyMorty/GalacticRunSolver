using SolverApp.ViewModels;

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