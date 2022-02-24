using Xamarin.Forms;

namespace SolverApp
{
    public partial class App : Application
    {
        public const int CellSize = 21;
        public const int CellSpacing = 2;
        public const int TopLeftSpace = 5;
        public const int MoveSize = 25;
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
