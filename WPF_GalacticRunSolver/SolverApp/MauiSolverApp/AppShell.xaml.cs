using MauiSolverApp.Views;
using SolverApp.ViewModels;
using SolverApp.Views;

namespace MauiSolverApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            SolverPage solverPage = new SolverPage();
            var solverVM = new SolverViewModel(solverPage);
            solverPage.BindingContext = solverVM;
            ShellSolverPage.Content = solverPage;

            //NewMapPage newMapPage = new NewMapPage();
            //newMapPage.BindingContext = new NewMapViewModel(solverVM);
            //ShellNewMapPage.Content = newMapPage;

            PhotoHelperPage photoHelperPage = new PhotoHelperPage();
            photoHelperPage.BindingContext = new PhotoHelperViewModel(solverVM);
            ShellPhotoHelperPage.Content = photoHelperPage;

            //TestPage test = new TestPage();
            //ShellTestPage.Content = test;
        }
    }
}
