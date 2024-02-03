using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SolverApp.ViewModels
{
    class PhotoHelperViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public PhotoHelperViewModel(SolverViewModel solvervm)
        {
            _SolverVM = solvervm;
        }

        private SolverViewModel _SolverVM;

        public string PhotoPath {  get; set; }
        internal void SetBackGroundImage(string photoPath)
        {
            PhotoPath = photoPath;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(PhotoPath)));
            _SolverVM.SetBackgroundImage(photoPath);
        }
    }
}
