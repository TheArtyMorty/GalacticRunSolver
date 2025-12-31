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

        internal void SetMaxSizeCheckbox(int size)
        {
            CheckCustomMapSize.IsChecked = (size != 16);
            MapSizeStepper.Value = size;
            MapSize.Text = size.ToString();
        }

        private void CustomMapSize_checkedChanged(object sender, CheckedChangedEventArgs e)
        {
            CustomMapSize.IsVisible = e.Value;
            var dataContext = this.BindingContext as SolverViewModel;
            if (dataContext != null)
            {
                if (e.Value)
                {
                    MapSizeStepper.Value = dataContext.theMap._Map._Size;
                }
                else
                {
                    ValueChangedEventArgs args = new ValueChangedEventArgs(MapSizeStepper.Value, 16);
                    Stepper_ValueChanged(MapSizeStepper, args);
                }
            }
        }

        private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var dataContext = this.BindingContext as SolverViewModel;
            var newSize = (int)e.NewValue;
            if (dataContext != null)
            {
                dataContext.ChangeSize(newSize);
                TheMapControl.UpdateSize(dataContext.theMap);
                MapSize.Text = newSize.ToString();
            }
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

        private void BoardEdition_SelectedIndexChanged(object sender, EventArgs e)
        {
            var numberOfBoardsInEdition = 8;
            if (BoardEdition.SelectedIndex == 0)
                numberOfBoardsInEdition = 16;

            TopLeftStepper.Maximum = numberOfBoardsInEdition;
            TopRightStepper.Maximum = numberOfBoardsInEdition;
            BottomLeftStepper.Maximum = numberOfBoardsInEdition;
            BottomRightStepper.Maximum = numberOfBoardsInEdition;
        }

        private void TopLeftStepper_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var bindingContext = this.BindingContext as SolverViewModel;
            if (bindingContext == null)
                return;

            TopLeftBoard.Text = "Board " + e.NewValue.ToString();
            bindingContext.SetQuadrant("TopLeft", (int)e.NewValue, BoardEdition.SelectedIndex);
        }

        private void TopRightStepper_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var bindingContext = this.BindingContext as SolverViewModel;
            if (bindingContext == null)
                return;

            TopRightBoard.Text = "Board " + e.NewValue.ToString();
            bindingContext.SetQuadrant("TopRight", (int)e.NewValue, BoardEdition.SelectedIndex);
        }

        private void BottomLeftStepper_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var bindingContext = this.BindingContext as SolverViewModel;
            if (bindingContext == null)
                return;

            BottomLeftBoard.Text = "Board " + e.NewValue.ToString();
            bindingContext.SetQuadrant("BottomLeft", (int)e.NewValue, BoardEdition.SelectedIndex);
        }

        private void BottomRightStepper_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var bindingContext = this.BindingContext as SolverViewModel;
            if (bindingContext == null)
                return;

            BottomRightBoard.Text = "Board " + e.NewValue.ToString();
            bindingContext.SetQuadrant("BottomRight", (int)e.NewValue, BoardEdition.SelectedIndex);
        }
    }
}