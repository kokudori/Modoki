using System;

using Livet;
using Livet.Commands;
using Livet.Messaging.Windows;

namespace Modoki.ViewModels
{
	sealed class ConfigViewModel : ViewModel
	{
		public ConfigViewModel()
		{
			inputCommand = new ViewModelCommand(Input, () => !String.IsNullOrEmpty(PinCode));
		}

		public void Initialize()
		{

		}

		async void Input()
		{
			var message = new WindowActionMessage(WindowAction.Close, "Close");
			await Messenger.RaiseAsync(message);
		}


		#region PinCode変更通知プロパティ
		private string pinCode;

		public string PinCode
		{
			get
			{ return pinCode; }
			set
			{
				if (pinCode == value)
					return;
				pinCode = value;
				RaisePropertyChanged();
				InputCommand.RaiseCanExecuteChanged();
			}
		}
		#endregion


		readonly ViewModelCommand inputCommand;
		public ViewModelCommand InputCommand { get { return inputCommand; } }
	}
}
