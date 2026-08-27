using System.IO;
using System.Threading.Tasks;

namespace VSExtensionForMomentum
{
	/// <summary>
	/// Settings which are actually used by commands.
	/// Values are taken from <see cref="ProjectSettingsFile"/> (if it exists in the repository)
	/// and every value which is absent or not valid there is taken from the options page.
	/// </summary>
	internal sealed class ResolvedSettings
	{
		public string RepositoryFolder { get; private set; }

		public string InstanceFolder { get; private set; }

		public int Minutes { get; private set; }

		public string CustomSourceFolder { get; private set; }

		public string CustomTargetFolder { get; private set; }

		public string CustomProjectName { get; private set; }

		public string NetVersion { get; private set; }

		public bool KillProcess { get; private set; }

		public bool StartProcesses { get; private set; }

		/// <summary>Path of the used settings file, empty if only the options page is used.</summary>
		public string SettingsFilePath { get; private set; } = string.Empty;

		public static async Task<ResolvedSettings> ResolveAsync()
		{
			var options = await Settings.GetLiveInstanceAsync();

			var resolved = new ResolvedSettings()
			{
				RepositoryFolder = options.RepositoryFolder,
				InstanceFolder = options.InstanceFolder,
				Minutes = options.Minutes,
				CustomSourceFolder = options.CustomSourceFolder,
				CustomTargetFolder = options.CustomTargetFolder,
				CustomProjectName = options.CustomProjectName,
				NetVersion = options.NetVersion,
				KillProcess = options.KillProcess,
				StartProcesses = options.StartProcesses,
			};

			var file = ProjectSettingsFile.Find(await GetSolutionFolderAsync())
					?? ProjectSettingsFile.Find(options.RepositoryFolder);

			if(file == null)
			{
				Logger.AddLine(LogType.Info, $"File {ProjectSettingsFile.FILE_NAME} is not found, settings from options page are used");
				return resolved;
			}

			resolved.SettingsFilePath = file.FullPath;

			if(file.TryGetFolder(nameof(RepositoryFolder), out var repositoryFolder))
				resolved.RepositoryFolder = repositoryFolder;

			if(file.TryGetFolder(nameof(InstanceFolder), out var instanceFolder))
				resolved.InstanceFolder = instanceFolder;

			if(file.TryGetFolder(nameof(CustomSourceFolder), out var customSourceFolder))
				resolved.CustomSourceFolder = customSourceFolder;

			if(file.TryGetFolder(nameof(CustomTargetFolder), out var customTargetFolder))
				resolved.CustomTargetFolder = customTargetFolder;

			if(file.TryGetString(nameof(CustomProjectName), out var customProjectName))
				resolved.CustomProjectName = customProjectName;

			if(file.TryGetString(nameof(NetVersion), out var netVersion))
				resolved.NetVersion = netVersion;

			if(file.TryGetInt(nameof(Minutes), out var minutes))
				resolved.Minutes = minutes;

			if(file.TryGetBool(nameof(KillProcess), out var killProcess))
				resolved.KillProcess = killProcess;

			if(file.TryGetBool(nameof(StartProcesses), out var startProcesses))
				resolved.StartProcesses = startProcesses;

			Logger.AddLine(LogType.Info, $"Repository folder : {resolved.RepositoryFolder}");
			Logger.AddLine(LogType.Info, $"Instance folder : {resolved.InstanceFolder}");

			return resolved;
		}

		private static async Task<string> GetSolutionFolderAsync()
		{
			try
			{
				var solution = await VS.Solutions.GetCurrentSolutionAsync();

				if(solution == null || string.IsNullOrEmpty(solution.FullPath))
					return null;

				return Path.GetDirectoryName(solution.FullPath);
			}
			catch(Exception ex)
			{
				Logger.AddLine(LogType.Warning, $"Current solution folder is not detected : {ex.Message}");
				return null;
			}
		}
	}
}
