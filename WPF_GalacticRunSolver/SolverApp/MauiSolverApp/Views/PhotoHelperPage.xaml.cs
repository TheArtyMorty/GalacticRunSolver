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

        async void TakePicture(object sender, EventArgs args)
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo != null)
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
                if (photo != null)
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
            if (dataContext != null)
                dataContext.SetBackGroundImage(path);
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