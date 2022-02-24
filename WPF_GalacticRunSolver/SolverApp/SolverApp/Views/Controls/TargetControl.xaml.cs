using SolverApp.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TargetControl : ContentView
    {
        public TargetControl()
        {
            InitializeComponent();
        }

        public void OnDragStarting(System.Object sender, Xamarin.Forms.DragStartingEventArgs e)
        {
            var target = (TargetViewModel)this.BindingContext;
            e.Data.Properties.Add("Target", target);
        }
    }
}