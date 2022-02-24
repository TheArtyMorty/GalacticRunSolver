using SolverApp.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RobotControl : ContentView
    {
        public RobotControl()
        {
            InitializeComponent();
        }

        public void OnDragStarting(System.Object sender, Xamarin.Forms.DragStartingEventArgs e)
        {
            var robot = (RobotViewModel)this.BindingContext;
            e.Data.Properties.Add("Robot", robot);
        }
    }
}