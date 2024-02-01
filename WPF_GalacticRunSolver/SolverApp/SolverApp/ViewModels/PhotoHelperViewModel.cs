using System;
using System.Collections.Generic;
using System.Text;

namespace SolverApp.ViewModels
{
    class PhotoHelperViewModel
    {
        public PhotoHelperViewModel(SolverViewModel solvervm)
        {
            _SolverVM = solvervm;
        }

        private SolverViewModel _SolverVM;

        internal void SetBackGroundImage(string photoPath)
        {
            _SolverVM.SetBackgroundImage(photoPath);
        }
    }
}
