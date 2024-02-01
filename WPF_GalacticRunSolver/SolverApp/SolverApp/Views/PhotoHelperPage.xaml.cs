using SolverApp.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SolverApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoHelperPage : ContentPage
    {
        public PhotoHelperPage()
        {
            InitializeComponent();
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

            var dataContext = this.BindingContext as PhotoHelperViewModel;
            dataContext.SetBackGroundImage(PhotoPath);
        }

        async void Reset(object sender, EventArgs args)
        {
            SetPhoto("");
        }
    }
}