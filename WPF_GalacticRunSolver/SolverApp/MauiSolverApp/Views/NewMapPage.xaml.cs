using SolverApp.ViewModels;
using System;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace SolverApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewMapPage : ContentPage
    {
        public NewMapPage()
        {
            InitializeComponent();
            SizePicker.SelectedIndex = 0;
            RobotCountPicker.SelectedIndex = 2;
            CustomSize.IsVisible = false;

        }

        void OnMapSizeChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;
            CustomSize.IsVisible = (selectedIndex == 2);
        }

        async void CreateMap(object sender, EventArgs args)
        {
            var dataContext = this.BindingContext as NewMapViewModel;
            if (dataContext != null)
            {
                int size;
                int robotCount = RobotCountPicker.SelectedIndex + 2;

                switch(SizePicker.SelectedIndex)
                {
                    case 2:
                        if (!int.TryParse(CustomSize.Text, out size))
                        {
                            size = 8;
                        }
                        if (size > 20 || size < 4)
                        {
                            size = 8;
                        }
                        break;
                    case 1:
                        size = 16;
                        break;
                    default:
                    case 0:
                        size = 8;
                        break;
                }

                dataContext.CreateNewMap(size, robotCount);
                await Shell.Current.GoToAsync("//SolverPage");
            }
        }
        async void SaveMap(object sender, EventArgs args)
        {
            string result = await DisplayPromptAsync("Save file as...", "fileName");
            if (result != null && result.Length > 0)
            {
                var dataContext = this.BindingContext as NewMapViewModel;
                string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), result + ".map");
                dataContext.SaveMap(_fileName);
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
                var dataContext = this.BindingContext as NewMapViewModel;
                dataContext.LoadMap(_fileName);
                await Shell.Current.GoToAsync("//SolverPage");
            }
        }
    }
}