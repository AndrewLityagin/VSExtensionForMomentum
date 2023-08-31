using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Mono.Cecil;

namespace VSExtensionForMomentum
{
	[Command(PackageIds.ReplaceBinaryFiles)]
	internal sealed class ReplaceBinaryFiles : BaseCommand<ReplaceBinaryFiles>
	{
		private const string MOMENTUM = "Momentum",
							 MESCRONTROL = "MEScontrol";

		private BinaryFileInfo[] binaryFiles;

		private BinaryFileInfo[] instanceFiles;

		private int minutes = 5;

		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			Logger.AddLine(LogType.Info, "Replacing binaries");
			await VS.StatusBar.ShowMessageAsync("Replacing binaries");
			await VS.StatusBar.StartAnimationAsync(StatusAnimation.Deploy);

			var settings = await Settings.GetLiveInstanceAsync();

			var instanceFolder = settings.InstanceFolder;
			var binaryFolder = settings.RepositoryFolder + "\\Binaries";

			if(!Directory.Exists(instanceFolder))
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				Logger.AddLine(LogType.Error, $"Instance folder not found : {instanceFolder}");
				await VS.StatusBar.ShowMessageAsync("Replacing binaries failed");
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				return;
			}

			if(!Directory.Exists(binaryFolder))
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				Logger.AddLine(LogType.Error, $"Repository folder not found : {binaryFolder}");
				await VS.StatusBar.ShowMessageAsync("Replacing binaries failed");
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
				return;
			}

			if(settings.Minutes > 0)
				minutes = settings.Minutes;

			instanceFiles = GetFiles(instanceFolder);
			Logger.AddLine(LogType.Info, $"Binary files in instance:{instanceFiles.Length}");

			binaryFiles = GetFiles(binaryFolder);
			Logger.AddLine(LogType.Info, $"Binary files in repos:{binaryFiles.Length}");

			ReplaceCompiledFiles();
			
			Logger.AddLine(LogType.Info, "Replacing binary files is completed");

			await VS.StatusBar.ShowMessageAsync("Replacing binaries complete");
			await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
		}

		private BinaryFileInfo[] GetFiles(string folder)
		{
			var directoryInfo = new DirectoryInfo(folder);
			var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
									 .Where(f => f.Name.StartsWith(MOMENTUM) || f.Name.StartsWith(MESCRONTROL))
									 .Where(f => f.Extension.Equals(".dll") || f.Extension.Equals(".exe")).ToArray();

			var binaryFiles = new List<BinaryFileInfo>();
			foreach(var file in files)
			{
				try
				{
					AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(file.FullName);
					var customAttribute = assembly.CustomAttributes.FirstOrDefault(c => c.AttributeType.Name == typeof(TargetFrameworkAttribute).Name);
					binaryFiles.Add(new BinaryFileInfo()
					{
						Info = file,
						AssemblyName = assembly.Name.Name,
						AssemblyVersion = assembly.Name.Version,
						TargetFramework = customAttribute != null ? (string)customAttribute.ConstructorArguments[0].Value : string.Empty,
					});
				}
				catch(Exception ex)
				{
					binaryFiles.Add(new BinaryFileInfo()
					{
						Info = file,
						AssemblyName = string.Empty,
						AssemblyVersion = null,
						TargetFramework = string.Empty,
					});
				}
			}
			return binaryFiles.ToArray();
		}
		public void ReplaceCompiledFiles()
		{
			DateTime currentTime = (DateTime.Now).AddMinutes(minutes * (-1));
			var modifiedFiles = binaryFiles.Where(bf => bf.Info.LastWriteTime >= currentTime).ToArray();
			int replacedNumber = 0;
			int isNotReplacedNumber = 0;
			foreach(var mf in modifiedFiles)
			{
				var filesToReplace = new List<BinaryFileInfo>();

				if(mf.IsUnreadableAssembly)
					filesToReplace = instanceFiles.Where(ftr => ftr.IsUnreadableAssembly)
												 .Where(ftr => ftr.Info.Name == mf.Info.Name).ToList();
				else
					filesToReplace = instanceFiles.Where(ftr => ftr.AssemblyName == mf.AssemblyName
															&& ftr.AssemblyVersion == mf.AssemblyVersion
															&& ftr.TargetFramework == mf.TargetFramework).ToList();

				foreach(var ftr in filesToReplace)
				{
					try
					{
						File.Copy(mf.Info.FullName, ftr.Info.FullName, true);
						File.Copy(mf.Info.FullName.Replace(".dll",".pdb"), ftr.Info.FullName.Replace(".dll", ".pdb"), true);
						Logger.AddLine(LogType.Info, $"Replaced : {mf.Info.Name} -> {ftr.Info.FullName}");
						replacedNumber++;
					}
					catch(Exception ex)
					{
						Logger.AddLine(LogType.Error, $" {ex.Message}: {mf.Info.Name} -> {ftr.Info.FullName}");
						isNotReplacedNumber++;
					}
				}
			}
			Logger.AddLine(LogType.Info, $"Files replaced:{replacedNumber}");
			Logger.AddLine(LogType.Info, $"Files are not replaced:{isNotReplacedNumber}");
		}
	}
}
