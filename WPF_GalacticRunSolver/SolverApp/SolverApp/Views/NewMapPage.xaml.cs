using SolverApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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

        async void TakePicture(object sender, EventArgs args)
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            await LoadPhotoAsync(photo);
        }

        async void ChoosePicture(object sender, EventArgs args)
        {
            var photo = await MediaPicker.PickPhotoAsync();
            await LoadPhotoAsync(photo);
        }

        public string PhotoPath { get; set; }
        async Task LoadPhotoAsync(FileResult photo)
        {
            // canceled
            if (photo == null)
            {
                PhotoPath = null;
                return;
            }
            // save the file into local storage
            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
                await stream.CopyToAsync(newStream);

            SetPhoto(newFile);
        }

        private void SetPhoto(string photoPath)
        {
            PhotoPath = photoPath;
            TheBoardPreview.Source = PhotoPath;

            var dataContext = this.BindingContext as NewMapViewModel;
            dataContext.SetBackGroundImage(PhotoPath);
        }

        async void Reset(object sender, EventArgs args)
        {
            SetPhoto("");
        }
    }
}