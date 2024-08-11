using System.IO;
using System.Security.Policy;

namespace VSExtensionForMomentum
{
	[Command(PackageIds.ReplaceOperatorFiles)]
	internal sealed class ReplaceOperatorFiles : BaseCommand<ReplaceOperatorFiles>
	{
		
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			await Logger.Activate();
			await Logger.Clear();
			Logger.AddLine(LogType.Info, "Replacing files from Operator  wwwroot folder");
			await VS.StatusBar.ShowMessageAsync("Replacing files from Operator wwwroot folder");
			await VS.StatusBar.StartAnimationAsync(StatusAnimation.Deploy);

			var settings = await Settings.GetLiveInstanceAsync();

			var OperatorInstanceFolder = $"{settings.InstanceFolder}\\WebServer\\Default\\Momentum.Operator\\wwwroot";
			var OperatorBinaryFolder = $"{settings.RepositoryFolder}\\Binaries\\Web\\Momentum.Web.Operator\\{settings.NetVersion}\\publish\\wwwroot";

			if(!Directory.Exists(OperatorInstanceFolder))
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				Logger.AddLine(LogType.Error, $"Folder with Operator in instance is not find : {OperatorInstanceFolder}");
				await VS.StatusBar.ShowMessageAsync("Replacing files from Operator  wwwroot folder is  failed");
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				return;
			}

			if(!Directory.Exists(OperatorBinaryFolder))
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				Logger.AddLine(LogType.Error, $"Folder with Operator in repository is not find : {OperatorBinaryFolder}");
				await VS.StatusBar.ShowMessageAsync("Replacing files from Operator  wwwroot folder is failed");
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				return;
			}
			var files = Directory.GetFiles(OperatorBinaryFolder, "*.*", SearchOption.AllDirectories);
			foreach(var file in files)
			{
				try
				{
					var newFileName = file.Replace(OperatorBinaryFolder, OperatorInstanceFolder);
					File.Delete(newFileName);
					File.Copy(file,newFileName);
					Logger.AddLine(LogType.Info, $" Replaced: {file} -> {newFileName}");
				}
				catch(Exception ex)
				{
					Logger.AddLine(LogType.Error, $"Exception : {ex.Message}");
				}
			}
			Logger.AddLine(LogType.Info, "Replacing files from Operator wwwroot folder is completed");

			await VS.StatusBar.ShowMessageAsync("Replacing files from Operator wwwroot folder is  completed");
			await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
		}
	}
}
