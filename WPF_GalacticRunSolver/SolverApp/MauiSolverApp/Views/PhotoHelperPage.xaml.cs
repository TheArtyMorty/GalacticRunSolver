using SolverApp.ViewModels;

namespace SolverApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhotoHelperPage : ContentPage
    {
        public PhotoHelperPage()
        {
            InitializeComponent();
        }

        public async Task<PermissionStatus> CheckAndRequestPhotosPermission()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status == PermissionStatus.Granted)
            {
                return status;
            }
            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings On iOS once a
                // permission has been denied it may not be requested again from
                // the application
                return status;
            }
            if (Permissions.ShouldShowRationale<Permissions.Photos>())
            {
            }
            status = await Permissions.RequestAsync<Permissions.Photos>();
            return status;
        }

        async void TakePicture(object sender, EventArgs args)
        {
            try
            {
                PermissionStatus status = await CheckAndRequestPhotosPermission();
                if (status == PermissionStatus.Granted || status == PermissionStatus.Limited)
                {
                    var photo = await MediaPicker.CapturePhotoAsync();
                    if (photo != null)
                        await LoadPhotoAsync(photo);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await DisplayAlert("Error", e.ToString(), "Ok");
            }
        }

        async void ChoosePicture(object sender, EventArgs args)
        {
            try
            {
                PermissionStatus status = await CheckAndRequestPhotosPermission();
                if (status == PermissionStatus.Granted || status == PermissionStatus.Limited)
                {
                    var photo = await MediaPicker.PickPhotoAsync();
                    if (photo != null)
                        await LoadPhotoAsync(photo);
                }   

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await DisplayAlert("Error", e.ToString(), "Ok");
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
            {
                await stream.CopyToAsync(newStream);
            }

            LoadPhoto(newFile);
        }


        private void LoadPhoto(string path)
        {
            var dataContext = BindingContext as PhotoHelperViewModel;
            if (dataContext != null)
                dataContext.Refresh(path != string.Empty);
            DropArea.SetPhoto(path);
        }

        void Reset(object sender, EventArgs args)
        {
            LoadPhoto(string.Empty);
        }

        void RecognizeMap(object sender, EventArgs args)
        {
            DropArea.StartRecognition();
        }

        private void SelectCorner(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var indexAsString = button.CommandParameter.ToString();
            if (indexAsString != null)
            {
                DropArea.SelectCorner(int.Parse(indexAsString));
            }
        }
    }
}