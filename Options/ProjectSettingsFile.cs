using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace VSExtensionForMomentum
{
	/// <summary>
	/// Settings file which is stored in the repository (near the solution or in any parent folder).
	/// Values from this file have higher priority than values from the options page
	/// (Tools -> Options -> VS Extension For Momentum -> Settings).
	/// </summary>
	internal sealed class ProjectSettingsFile
	{
		public const string FILE_NAME = "momentum.replacefiles.json";

		private readonly Dictionary<string, object> values;

		/// <summary>Full path of the loaded settings file.</summary>
		public string FullPath { get; }

		/// <summary>Folder of the loaded settings file, relative paths are resolved against it.</summary>
		public string Folder => Path.GetDirectoryName(FullPath);

		private ProjectSettingsFile(string fullPath, Dictionary<string, object> values)
		{
			this.FullPath = fullPath;
			this.values = values;
		}

		/// <summary>
		/// Searches <see cref="FILE_NAME"/> in <paramref name="startFolder"/> and in all its parent folders.
		/// Returns null if the file is not found or can not be parsed.
		/// </summary>
		public static ProjectSettingsFile Find(string startFolder)
		{
			if(string.IsNullOrEmpty(startFolder))
				return null;

			try
			{
				var folder = new DirectoryInfo(startFolder);
				while(folder != null)
				{
					var file = Path.Combine(folder.FullName, FILE_NAME);
					if(File.Exists(file))
						return Load(file);

					folder = folder.Parent;
				}
			}
			catch(Exception ex)
			{
				Logger.AddLine(LogType.Warning, $"Searching of {FILE_NAME} is failed : {ex.Message}");
			}
			return null;
		}

		private static ProjectSettingsFile Load(string file)
		{
			try
			{
				var content = File.ReadAllText(file);
				var parsed = new JavaScriptSerializer().DeserializeObject(content) as Dictionary<string, object>;

				if(parsed == null)
				{
					Logger.AddLine(LogType.Warning, $"Settings file contains no settings, options page will be used : {file}");
					return null;
				}

				// keys in the file can be written in any case : "InstanceFolder", "instanceFolder", "instancefolder"
				var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				foreach(var pair in parsed)
					values[pair.Key] = pair.Value;

				Logger.AddLine(LogType.Info, $"Settings file is used : {file}");
				return new ProjectSettingsFile(file, values);
			}
			catch(Exception ex)
			{
				Logger.AddLine(LogType.Warning, $"Settings file is not valid, options page will be used : {file} : {ex.Message}");
				return null;
			}
		}

		/// <summary>Reads a non empty string value.</summary>
		public bool TryGetString(string key, out string value)
		{
			value = null;

			if(!values.TryGetValue(key, out var raw) || raw == null)
				return false;

			var text = raw.ToString().Trim();
			if(string.IsNullOrEmpty(text))
			{
				Logger.AddLine(LogType.Warning, $"Setting '{key}' is empty in {FILE_NAME}, value from options page will be used");
				return false;
			}

			value = text;
			return true;
		}

		/// <summary>Reads a folder value. Relative paths are resolved against the folder of the settings file.</summary>
		public bool TryGetFolder(string key, out string value)
		{
			value = null;

			if(!TryGetString(key, out var text))
				return false;

			try
			{
				var path = Path.IsPathRooted(text) ? text : Path.Combine(Folder, text);
				value = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				return true;
			}
			catch(Exception ex)
			{
				Logger.AddLine(LogType.Warning, $"Setting '{key}' is not a valid path in {FILE_NAME} : {text} : {ex.Message}");
				return false;
			}
		}

		/// <summary>Reads a positive integer value.</summary>
		public bool TryGetInt(string key, out int value)
		{
			value = 0;

			if(!TryGetString(key, out var text))
				return false;

			if(!int.TryParse(text, out var parsed) || parsed <= 0)
			{
				Logger.AddLine(LogType.Warning, $"Setting '{key}' is not a positive number in {FILE_NAME} : {text}, value from options page will be used");
				return false;
			}

			value = parsed;
			return true;
		}

		/// <summary>Reads a boolean value.</summary>
		public bool TryGetBool(string key, out bool value)
		{
			value = false;

			if(!TryGetString(key, out var text))
				return false;

			if(!bool.TryParse(text, out var parsed))
			{
				Logger.AddLine(LogType.Warning, $"Setting '{key}' is not true/false in {FILE_NAME} : {text}, value from options page will be used");
				return false;
			}

			value = parsed;
			return true;
		}
	}
}
