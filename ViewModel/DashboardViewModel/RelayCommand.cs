using System;
using System.Windows.Input;

namespace ViewModel.DashboardViewModel
{
    /// <summary>
    /// A basic command that runs an Action
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> mAction;

        public RelayCommand(Action<object> action)
        {
            mAction = action;
        }

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => mAction(parameter);
    }
}