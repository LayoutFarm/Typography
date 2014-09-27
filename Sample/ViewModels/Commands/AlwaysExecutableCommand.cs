using System;
using System.Windows.Input;

namespace Sample.ViewModels.Commands
{
    public abstract class AlwaysExecutableCommand : ICommand
    {
#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public abstract void Execute(object parameter);
    }
}
