using System;
using System.Windows.Input;

namespace TPC_GUI.ViewModels
{
    public class RelayCommand : ICommand
    {
        #region Private
        private Action _action;
        #endregion

        #region Constructor
        public RelayCommand(Action action)
        {
            _action = action;
        }
        #endregion

        #region Public
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            _action();
        }
        #endregion
    }
}
