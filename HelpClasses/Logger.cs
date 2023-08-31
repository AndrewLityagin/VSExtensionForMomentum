using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSExtensionForMomentum
{
	public static class Logger
	{
		private static OutputWindowPane pane;

		private const string PANE_NAME = "VSExtensionForMomentum";

		public static async Task InitializeAsync()
		{
			pane = await VS.Windows.CreateOutputWindowPaneAsync(PANE_NAME);
		}
		
		public static void AddLine(LogType logTypeValue, string line)
		{
			if(pane == null)
			{
				VS.MessageBox.ShowError("Logger is not initialized, please call method InitializeAsync before use");
				return;
			}
			pane.WriteLine($"{Enum.GetName(typeof(LogType),logTypeValue)} {DateTime.Now} : {line}");
		}

		public static async Task Activate()
		{
			await pane.ActivateAsync();
		}

		public static async Task Clear()
		{
			await pane.ClearAsync();
		}
	}
	public enum LogType
	{
		Error,
		Info,
		Warning
	}

}
