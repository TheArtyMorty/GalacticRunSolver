using SolverApp.ViewModels;
using System.Diagnostics;

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

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            var bindingContext = this.BindingContext as SolverViewModel;
            if (bindingContext != null)
                bindingContext._SolverModeOn = SwitchMode.IsToggled;
        }

        private void PanMode_Toggled(object sender, ToggledEventArgs e)
        {
            var bindingContext = this.BindingContext as SolverViewModel;
            if (bindingContext != null)
                bindingContext._PanModeOn = !PanMode.IsToggled;
        }

        private void Quadrant_Clicked(object sender, EventArgs e)
        {
            var bindingContext = this.BindingContext as SolverViewModel;
            if (bindingContext == null)
                return;

            var parameters = ((Button)sender).CommandParameter as string;
            if (parameters != null)
            {
                var splitted = parameters.Split(';');
                var quadrant = splitted[1];
                var board = splitted[0];
                bindingContext.SetQuadrant(quadrant, board);
            }
        }

        public static bool AllowCustomWalls = false;

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            AllowCustomWalls = e.Value;
        }
    }
}