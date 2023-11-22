using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using Mono.Cecil;

namespace VSExtensionForMomentum
{
	[Command(PackageIds.ReplaceBinaryFiles)]
	internal sealed class ReplaceBinaryFiles : BaseCommand<ReplaceBinaryFiles>
	{
		private const string MOMENTUM = "Momentum",
							 MESCRONTROL = "MEScontrol";

		private string customProjectName = string.Empty;

		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			await Logger.Activate();
			await Logger.Clear();
			Logger.AddLine(LogType.Info, "Replacing binaries");
			await VS.StatusBar.ShowMessageAsync("Replacing binaries");
			await VS.StatusBar.StartAnimationAsync(StatusAnimation.Deploy);

			var settings = await Settings.GetLiveInstanceAsync();

			this.customProjectName = settings.CustomProjectName;
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
			var minutes = 5;
			if(settings.Minutes > 0)
				minutes = settings.Minutes;
			var (binaryFiles, instanceFiles) = await Task.Run(() => GetAllFiles(binaryFolder, instanceFolder, minutes));
			await Task.Run(() => ReplaceCompiledFiles(binaryFiles, instanceFiles));
			
			Logger.AddLine(LogType.Info, "Replacing binary files is completed");

			await VS.StatusBar.ShowMessageAsync("Replacing binaries complete");
			await VS.StatusBar.EndAnimationAsync(StatusAnimation.Deploy);
		}

		private BinaryFileInfo[] GetFiles(string folder, int minutes = 0)
		{
			var directoryInfo = new DirectoryInfo(folder);

			DateTime currentTime = (DateTime.Now).AddMinutes(minutes * (-1));

			var files = minutes > 0 ? directoryInfo.GetFiles("*", SearchOption.AllDirectories)
									 .Where(f => f.Name.StartsWith(MOMENTUM) || f.Name.StartsWith(MESCRONTROL) || f.Name.StartsWith(customProjectName))
									 .Where(f => f.Extension.Equals(".dll") || f.Extension.Equals(".exe"))
									 .Where(f => f.LastWriteTime >= currentTime)
									 .ToArray()
									 : directoryInfo.GetFiles("*", SearchOption.AllDirectories)
									 .Where(f => f.Name.StartsWith(MOMENTUM) || f.Name.StartsWith(MESCRONTROL) || f.Name.StartsWith(customProjectName))
									 .Where(f => f.Extension.Equals(".dll") || f.Extension.Equals(".exe"))
									 .ToArray();

			var binaryFiles = new List<BinaryFileInfo>();
			foreach(var file in files)
			{
				try
				{
					using(AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(file.FullName))
					{
						var customAttribute = assembly.CustomAttributes.FirstOrDefault(c => c.AttributeType.Name == typeof(TargetFrameworkAttribute).Name);
						binaryFiles.Add(new BinaryFileInfo()
						{
							Info = file,
							AssemblyName = assembly.Name.Name,
							AssemblyVersion = assembly.Name.Version,
							TargetFramework = customAttribute != null ? (string)customAttribute.ConstructorArguments[0].Value : string.Empty,
						});
					}
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

		private (BinaryFileInfo[] binaryFiels, BinaryFileInfo[] instanceFiles) GetAllFiles(string binaryFolder, string instanceFolder, int minutes)
		{
			var instanceFiles = GetFiles(instanceFolder);
			Logger.AddLine(LogType.Info, $"Binary files in instance:{instanceFiles.Length}");

			var binaryFiles = GetFiles(binaryFolder, minutes);
			Logger.AddLine(LogType.Info, $"Binary files in repos:{binaryFiles.Length}");

			return (binaryFiles, instanceFiles);
		}

		private void ReplaceCompiledFiles(BinaryFileInfo[] binaryFiles, BinaryFileInfo[] instanceFiles)
		{
			int replacedNumber = 0;
			int isNotReplacedNumber = 0;
			
			var isNotReplacedFiles = new List<(BinaryFileInfo, BinaryFileInfo)>();

			foreach(var mf in binaryFiles)
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
					ReplaceFile(mf, ftr, ref replacedNumber, ref isNotReplacedNumber, isNotReplacedFiles);
				}
			}
			if(isNotReplacedFiles.Any())
			{
				Logger.AddLine(LogType.Info, $"Files are not replaced:{isNotReplacedNumber}");
				Logger.AddLine(LogType.Info, $"Try again...");
				foreach(var files in isNotReplacedFiles)
				{
					ReplaceFile(files.Item1, files.Item2, ref replacedNumber, ref isNotReplacedNumber, null);
				}
			}
			Logger.AddLine(LogType.Info, $"Files replaced:{replacedNumber}");
			Logger.AddLine(LogType.Info, $"Files are not replaced:{isNotReplacedNumber}");
		}

		private void ReplaceFile(BinaryFileInfo sourse, BinaryFileInfo target, ref int replacedNumber, ref int isNotReplacedNumber, List<(BinaryFileInfo, BinaryFileInfo)> isNotReplacedFiles)
		{
			try
			{
				File.Copy(sourse.Info.FullName, target.Info.FullName, true);
				Logger.AddLine(LogType.Info, $"Replaced : {sourse.Info.Name} -> {target.Info.FullName}");
				replacedNumber++;
				if(isNotReplacedFiles == null)
					isNotReplacedNumber--;
			}
			catch(Exception ex)
			{
				Logger.AddLine(LogType.Error, $" {ex.Message}: {sourse.Info.Name}");
				if(isNotReplacedFiles != null)
				{
					isNotReplacedFiles.Add((sourse, target));
					isNotReplacedNumber++;
				}
			}

			try
			{
				var sourcePDBFileNameSimple = sourse.Info.Name.Replace(".dll", ".pdb");
				var sourcePDBFileName = sourse.Info.FullName.Replace(".dll", ".pdb");
				var targetPDBFileName = target.Info.FullName.Replace(".dll", ".pdb");

				if(!File.Exists(sourcePDBFileName) || !File.Exists(targetPDBFileName))
					return;

				File.Copy(sourcePDBFileName, targetPDBFileName, true);
			}
			catch(Exception ex)
			{
				Logger.AddLine(LogType.Error, $" {ex.Message}");
			}
		}
	}
}
