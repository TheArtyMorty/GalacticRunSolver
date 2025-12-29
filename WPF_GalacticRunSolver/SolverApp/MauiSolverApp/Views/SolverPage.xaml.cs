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

            BoardEdition.SelectedIndex = 0;
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
                bindingContext.SetQuadrant(quadrant, board, BoardEdition.SelectedIndex);
            }
        }

        public static bool AllowCustomWalls = false;

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            AllowCustomWalls = e.Value;
        } 
        
        private void AdditionalRobot_checkedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                var bindingContext = this.BindingContext as SolverViewModel;
                if (bindingContext != null && bindingContext.theMap._Robots.Count == 4)
                {
                    bindingContext.theMap.CreateAdditionalRobot();
                    TheMapControl.AddRobot(bindingContext.theMap._Robots.Last());
                }
            }
            else
            {
                var bindingContext = this.BindingContext as SolverViewModel;
                if (bindingContext != null && bindingContext.theMap._Robots.Count == 5)
                {
                    bindingContext.theMap.RemoveAdditionalRobot();
                    TheMapControl.RemoveAdditionalRobot();
                }
            }
        }

        private void CustomMapSize_checkedChanged(object sender, CheckedChangedEventArgs e)
        {
            CustomMapSize.IsVisible = e.Value;
        }

        private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var dataContext = this.BindingContext as SolverViewModel;
            var newSize = (int)e.NewValue;
            if (dataContext != null)
                dataContext.ChangeSize(newSize);
            TheMapControl.UpdateSize(newSize);
            MapSize.Text = newSize.ToString();
        }

        async void SaveMap(object sender, EventArgs args)
        {
            string result = await DisplayPromptAsync("Save file as...", "fileName");
            if (result != null && result.Length > 0)
            {
                var dataContext = this.BindingContext as SolverViewModel;
                string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), result + ".map");
                if (dataContext != null)
                    dataContext.theMap.SaveMap(_fileName);
            }
        }
        async void LoadMap(object sender, EventArgs args)
        {
            var files = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "*.map");
            var fileNames = files.Select(path => System.IO.Path.GetFileName(path)).ToArray();
            string result = await DisplayActionSheet("Open file...", "cancel", "exit", fileNames);
            string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), result);
            if (File.Exists(_fileName))
            {
                var dataContext = this.BindingContext as SolverViewModel;
                if (dataContext != null)
                    dataContext.CreateNewMap(new MapViewModel(_fileName));
            }
        }


    }
}