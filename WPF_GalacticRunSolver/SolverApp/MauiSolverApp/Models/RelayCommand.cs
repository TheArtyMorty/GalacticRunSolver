using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SolverApp.Models
{
    public class RelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged = (sender, e) => { };

        public RelayCommand(Action action)
        {
            _Action = action;
        }

        private Action _Action;

        bool ICommand.CanExecute(object? parameter)
        {
            return true;
        }

        void ICommand.Execute(object? parameter)
        {
            _Action();
        }
    }
}
