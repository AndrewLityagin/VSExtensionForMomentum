using System.IO;
using System.Security.Policy;

namespace VSExtensionForMomentum
{
	[Command(PackageIds.ReplaceCustomFolderFiles)]
	internal sealed class ReplaceCustomFolderFiles : BaseCommand<ReplaceCustomFolderFiles>
	{
		
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			var settings = await Settings.GetLiveInstanceAsync();

			var sourceFolder = settings.CustomSourceFolder;
			var targetFolder = settings.CustomTargetFolder;

			await Logger.Activate();
			await Logger.Clear();
			Logger.AddLine(LogType.Info, $"Replacing files from {sourceFolder} to {targetFolder}");
			await VS.StatusBar.ShowMessageAsync("Replacing custom folder");
			await VS.StatusBar.StartAnimationAsync(StatusAnimation.Deploy);

			if(!Directory.Exists(sourceFolder))
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				Logger.AddLine(LogType.Error, $"Folder is not find : {sourceFolder}");
				await VS.StatusBar.ShowMessageAsync("Replacing custom folder failed");
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				return;
			}

			if(!Directory.Exists(targetFolder))
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				Logger.AddLine(LogType.Error, $"Folder is not find : {targetFolder}");
				await VS.StatusBar.ShowMessageAsync("Replacing custom folder failed");
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				return;
			}

			var files = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
			foreach(var file in files)
			{
				try
				{
					var newFileName = file.Replace(sourceFolder, targetFolder);
					File.Delete(newFileName);
					File.Copy(file, newFileName);
					Logger.AddLine(LogType.Info, $" Replaced: {file} -> {newFileName}");
				}
				catch(Exception ex)
				{
					Logger.AddLine(LogType.Error, $"Exception : {ex.Message}");
				}
			}
			Logger.AddLine(LogType.Info, "Replacing files from custom folder is completed");

			await VS.StatusBar.ShowMessageAsync("Replacing files from custom folder is completed");
			await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
		}
	}
}
