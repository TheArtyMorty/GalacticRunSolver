﻿using Xamarin.Forms;

namespace SolverApp
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
