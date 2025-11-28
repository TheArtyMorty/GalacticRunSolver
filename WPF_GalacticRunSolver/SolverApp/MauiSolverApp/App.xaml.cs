namespace MauiSolverApp
{
    public partial class App : Application
    {
        public const int Margin = 10;
        public const int MoveSize = 25;

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
