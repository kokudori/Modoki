using System;
using System.Windows;

using Livet;

namespace Modoki
{
	public partial class App : Application
	{
		void OnStartup(object sender, StartupEventArgs e)
		{
			DispatcherHelper.UIDispatcher = Dispatcher;
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnErrorHandle);
		}

		void OnErrorHandle(object sender, UnhandledExceptionEventArgs e)
		{
			MessageBox.Show(
				"不明なエラーが発生しました。アプリケーションを終了します。",
				"エラー",
				MessageBoxButton.OK,
				MessageBoxImage.Error
			);

			Environment.Exit(1);
		}
	}
}
