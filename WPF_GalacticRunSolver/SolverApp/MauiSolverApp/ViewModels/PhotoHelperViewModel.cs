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

        internal void SetBackGroundImage(string photoPath)
        {
            _SolverVM.SetBackgroundImage(photoPath);
            Refresh(photoPath.Length > 0);
        }

        public bool PhotoLoaded { get; set; } = false;
        public bool NoPhotoLoaded => !PhotoLoaded;

        public void Refresh(bool photoLoaded)
        {
            PhotoLoaded = photoLoaded;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(PhotoLoaded)));
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(NoPhotoLoaded)));
        }
    }
}
