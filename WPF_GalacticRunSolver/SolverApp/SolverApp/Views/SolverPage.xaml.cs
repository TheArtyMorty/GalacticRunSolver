using SolverApp.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SolverApp.Views
{
    public partial class SolverPage : ContentPage
    {
        public SolverPage()
        {
            InitializeComponent();
        }

        public void GenerateMap(MapViewModel map)
        {
            TheMapControl.GenerateMap(map);
        }

        public void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            var SolverPageVM = BindingContext as SolverViewModel;
            if (SolverPageVM != null)
            {
                SolverPageVM.ZoomInOrOut(e.NewValue/1000, Math.Max(TheMapView.Width, TheMapView.Height));
            }
        }
    }
}