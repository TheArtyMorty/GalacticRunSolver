using SkiaSharp;
using SolverApp.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp.Views.Forms;
using TouchTracking;
using static SkiaSharp.SKImageFilter;

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
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                await LoadPhotoAsync(photo);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        async void ChoosePicture(object sender, EventArgs args)
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();
                await LoadPhotoAsync(photo);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        async Task LoadPhotoAsync(FileResult photo)
        {
            // canceled
            if (photo == null)
            {
                return;
            }
            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
                await stream.CopyToAsync(newStream);

            LoadPhoto(newFile);
        }


        private void LoadPhoto(string path)
        {
            var dataContext = BindingContext as PhotoHelperViewModel;
            dataContext.SetBackGroundImage(path);
            DropArea.SetPhoto(path);
        }

        void Reset(object sender, EventArgs args)
        {
            LoadPhoto(string.Empty);
        }

        void RecognizeMap(object sender, EventArgs args)
        {
            //TO DO
        }
    }
}