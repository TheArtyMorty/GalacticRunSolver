using Microsoft.Maui.Controls.Shapes;
using SolverApp.ViewModels;

namespace MauiSolverApp.Views;

public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();

        vm = new MapViewModel(16);
        TheMapControl.BindingContext = vm;
        TheMapControl.GenerateMap(vm);
    }

    MapViewModel vm;

    private void Button_Clicked(object sender, EventArgs e)
    {
        vm.ChangeSize(vm._Map._Size + 1);
        TheMapControl.UpdateSize(vm);
    }
}