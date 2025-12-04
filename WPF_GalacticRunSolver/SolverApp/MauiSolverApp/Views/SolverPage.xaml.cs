using SolverApp.ViewModels;

namespace SolverApp.Views
{
    public partial class SolverPage : ContentPage
    {
        public SolverPage()
        {
            InitializeComponent();

            FakePanPinch.ConnectToRealContainer(MainPanPinch);
        }

        public void GenerateMap(MapViewModel map)
        {
            TheMapControl.GenerateMap(map);
        }
    }
}