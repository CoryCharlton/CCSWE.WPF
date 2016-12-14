﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CCSWE.Threading.Tasks;

// Based on the article `Patterns for Asynchronous MVVM Applications: Commands` by Stephen Cleary (https://msdn.microsoft.com/en-us/magazine/dn630647.aspx)
namespace CCSWE.Windows.Input
{
	/// <summary>
	/// An <see cref="ICommand"/> implementation around a <see cref="Task"/> that extends the CanExecute() method to check whether or not the <see cref="Task"/> is already executing
	/// </summary>
	/// <typeparam name="TResult">The result type of the target <see cref="Task"/></typeparam>
	public class AsyncCommand<TResult> : AsyncCommandBase
	{
		#region Constructor
		public AsyncCommand(Func<Task<TResult>> execute) : this(execute, null)
		{

		}

	    public AsyncCommand(Func<Task<TResult>> execute, Func<bool> canExecute)
	    {
            Ensure.IsNotNull(nameof(execute), execute);

            _canExecute = canExecute;
            _execute = execute;
	    }
		#endregion

		#region Private Fields
        //TODO: AsyncCommand<TResult> - MVVM Light implements a WeakAction and WeakFunc. Need to look into wether or not something like that is necessary
        private readonly Func<bool> _canExecute;
        private readonly Func<Task<TResult>> _execute;
		private NotifyTaskCompletion<TResult> _execution;
	    #endregion

		#region Public Properties
		public NotifyTaskCompletion<TResult> Execution
		{
			get { return _execution; }
			private set
			{
				_execution = value;

				RaisePropertyChanged();
			}
		}
		#endregion

		#region Public Methods
		public override bool CanExecute(object parameter)
		{
            // Check if already executing
            if (Execution != null && !Execution.IsCompleted)
            {
                return false;
            }

            // There is no function to check so I suppose we can execute :)
            return _canExecute == null || _canExecute();
		}

	    public override async Task ExecuteAsync(object parameter)
	    {
	        if (!CanExecute(parameter))
	        {
	            return;
	        }

	        Execution = new NotifyTaskCompletion<TResult>(_execute());
	        RaiseCanExecuteChanged();

	        await Execution.TaskCompletion;

	        RaiseCanExecuteChanged();
	    }
	    #endregion
	}
}