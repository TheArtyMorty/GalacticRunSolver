using SolverApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
    }
}