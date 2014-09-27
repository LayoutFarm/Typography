using System;

namespace Sample.ViewModels.Commands
{
    public class DelegatingCommand : AlwaysExecutableCommand
    {
        private readonly Action<object> _action;

        public DelegatingCommand(Action<object> action)
        {
            _action = action;
        }

        public DelegatingCommand(Action action)
        {
            _action = o => action();
        }

        public override void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
