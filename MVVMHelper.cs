using System;
using System.Diagnostics;
using System.Windows.Input;

namespace BiliRaffle
{
    public class RelayCommand : ICommand

    {
        #region Private Fields

        private readonly Func<bool> _canExecute;

        private readonly Action _execute;

        #endregion Private Fields

        #region Public Constructors

        public RelayCommand(Action execute)

                : this(execute, null)

        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute)

        {
            _execute = execute ?? throw new ArgumentNullException("execute");

            _canExecute = canExecute;
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler CanExecuteChanged

        {
            add

            {
                if (_canExecute != null)

                    CommandManager.RequerySuggested += value;
            }

            remove

            {
                if (_canExecute != null)

                    CommandManager.RequerySuggested -= value;
            }
        }

        #endregion Public Events

        #region Public Methods

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)

        {
            return _canExecute == null ? true : _canExecute();
        }

        public void Execute(object parameter)

        {
            _execute();
        }

        #endregion Public Methods
    }

    public class RelayCommand<T> : ICommand

    {
        #region Private Fields

        private readonly Func<object,bool> _canExecute;

        private readonly Action<T> _execute;

        #endregion Private Fields

        #region Public Constructors

        public RelayCommand(Action<T> execute)

                : this(execute, null)

        {
        }

        public RelayCommand(Action<T> execute, Func<object,bool> canExecute)

        {
            _execute = execute ?? throw new ArgumentNullException("execute");

            _canExecute = canExecute;
        }

        #endregion Public Constructors

        #region Public Events

        public event EventHandler CanExecuteChanged

        {
            add

            {
                if (_canExecute != null)

                    CommandManager.RequerySuggested += value;
            }

            remove

            {
                if (_canExecute != null)

                    CommandManager.RequerySuggested -= value;
            }
        }

        #endregion Public Events

        #region Public Methods

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if(typeof(T) == typeof(int) && parameter.GetType() == typeof(string))
            {
                parameter = (string)parameter == "" ? "-1" : parameter;
                object i = int.Parse((string)parameter);
                _execute((T)i);
            }else
            _execute((T)parameter);
        }

        #endregion Public Methods
    }
}