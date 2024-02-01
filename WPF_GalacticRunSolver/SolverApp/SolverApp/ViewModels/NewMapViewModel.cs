using SolverApp.Models;
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
            _SolverVM.CreateNewMap(new MapViewModel(size, robotCount));
        }

        internal void SaveMap(string fileName)
        {
            _SolverVM.theMap.SaveMap(fileName);
        }

        internal void LoadMap(string fileName)
        {
            _SolverVM.CreateNewMap(new MapViewModel(fileName));
        }
    }
}
