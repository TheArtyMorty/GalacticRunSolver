using SolverApp.Models;
using SolverApp.ViewModels;
using SolverApp.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SolverApp
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            SolverPage solverPage = new SolverPage();
            var solverVM = new SolverViewModel(solverPage);
            solverPage.BindingContext = solverVM;
            ShellSolverPage.Content = solverPage;

            NewMapPage newMapPage = new NewMapPage();
            newMapPage.BindingContext = new NewMapViewModel(solverVM);
            ShellNewMapPage.Content = newMapPage;
        }

        //public async void SaveMap(object sender, EventArgs e)
        //{
        //    string result = await DisplayPromptAsync("Save file as...", "fileName");
        //    if (result != null && result.Length > 0)
        //    {
        //        var solverVM = this.CurrentPage.BindingContext as SolverViewModel;
        //        string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), result + ".txt");
        //        solverVM.theMap.SaveMap(_fileName);
        //    }
        //    this.FlyoutIsPresented = false;
        //}

        //public async void OpenFromFile(object sender, EventArgs e)
        //{
        //    var files = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "*.txt");
        //    var fileNames = files.Select(path => System.IO.Path.GetFileName(path)).ToArray();
        //    string result = await DisplayActionSheet("Open file...", "cancel", "exit", fileNames);
        //    string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), result);
        //    if (File.Exists(_fileName))
        //    {
        //        var solverVM = this.CurrentPage.BindingContext as SolverViewModel;
        //        solverVM.theMap = new MapViewModel(_fileName);
        //    }
        //    this.FlyoutIsPresented = false;
        //}
    }
}
