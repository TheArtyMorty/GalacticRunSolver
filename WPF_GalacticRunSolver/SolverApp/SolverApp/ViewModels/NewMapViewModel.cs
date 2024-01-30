using System;
using System.Collections.Generic;
using System.Text;

namespace SolverApp.ViewModels
{
    internal class NewMapViewModel
    {
        public NewMapViewModel(SolverViewModel solvervm)
        {
            _SolverVM = solvervm;
        }

        private SolverViewModel _SolverVM;

        internal void CreateNewMap(int size, int robotCount)
        {
            _SolverVM.theMap = new MapViewModel(size, robotCount);
        }
    }
}
