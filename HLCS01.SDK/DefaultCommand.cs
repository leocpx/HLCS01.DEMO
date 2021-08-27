using System;
using System.Windows.Input;

namespace SDK
{
    public class DefaultCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        Action _a;
        Func<bool> _b;

        public DefaultCommand(Action _a, Func<bool> _b)
        {
            this._a = _a;
            this._b = _b;
        }

        public bool CanExecute(object parameter)
        {
            return _b();
        }

        public void Execute(object parameter)
        {
            _a();
        }
    }
}
