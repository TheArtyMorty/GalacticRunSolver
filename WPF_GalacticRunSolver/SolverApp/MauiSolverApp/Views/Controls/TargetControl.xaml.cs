using SolverApp.ViewModels;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TargetControl : ContentView
    {
        public TargetControl()
        {
            InitializeComponent();
        }

        public void OnDragStarting(System.Object sender, Microsoft.Maui.Controls.DragStartingEventArgs e)
        {
            var target = (TargetViewModel)this.BindingContext;
            e.Data.Properties.Add("Target", target);
        }
    }
}