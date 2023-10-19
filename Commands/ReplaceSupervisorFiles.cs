using System.IO;
using System.Security.Policy;

namespace VSExtensionForMomentum
{
	[Command(PackageIds.ReplaceSupervisorFiles)]
	internal sealed class ReplaceSupervisorFiles : BaseCommand<ReplaceSupervisorFiles>
	{
		
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			await Logger.Activate();
			await Logger.Clear();
			Logger.AddLine(LogType.Info, "Replacing files from supervisor  wwwroot folder");
			await VS.StatusBar.ShowMessageAsync("Replacing files from supervisor wwwroot folder");
			await VS.StatusBar.StartAnimationAsync(StatusAnimation.Deploy);

			var settings = await Settings.GetLiveInstanceAsync();

			var supervisorInstanceFolder = settings.InstanceFolder + "\\WebServer\\Default\\Momentum.Supervisor\\wwwroot";
			var supervisorBinaryFolder = settings.RepositoryFolder + "\\Binaries\\Web\\Momentum.Web.Supervisor\\net6.0\\publish\\wwwroot";

			if(!Directory.Exists(supervisorInstanceFolder))
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				Logger.AddLine(LogType.Error, $"Folder with supervisor in instance is not find : {supervisorInstanceFolder}");
				await VS.StatusBar.ShowMessageAsync("Replacing files from supervisor  wwwroot folder is  failed");
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				return;
			}

			if(!Directory.Exists(supervisorBinaryFolder))
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				Logger.AddLine(LogType.Error, $"Folder with supervisor in repository is not find : {supervisorBinaryFolder}");
				await VS.StatusBar.ShowMessageAsync("Replacing files from supervisor  wwwroot folder is failed");
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				return;
			}
			var files = Directory.GetFiles(supervisorBinaryFolder, "*.*", SearchOption.AllDirectories);
			foreach(var file in files)
			{
				try
				{
					var newFileName = file.Replace(supervisorBinaryFolder, supervisorInstanceFolder);
					File.Replace(file, newFileName, file);
					Logger.AddLine(LogType.Info, $"{file} -> {newFileName}");
				}
				catch
				{
					Logger.AddLine(LogType.Error, $"File is not find : {file}");
				}
			}
			Logger.AddLine(LogType.Info, "Replacing files from supervisor wwwroot folder is completed");

			await VS.StatusBar.ShowMessageAsync("Replacing files from supervisor wwwroot folder is  completed");
			await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
		}
	}
}
