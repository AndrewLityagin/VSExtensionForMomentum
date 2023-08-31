using System.IO;

namespace VSExtensionForMomentum
{
	public class BinaryFileInfo
	{
		public FileInfo Info { get; set; }

		public string TargetFramework { get; set; }

		public string AssemblyName { get; set; }

		public Version AssemblyVersion { get; set; }

		public bool IsUnreadableAssembly
		{
			get
			{
				return string.IsNullOrEmpty(AssemblyName) || AssemblyVersion == null || string.IsNullOrEmpty(TargetFramework);
			}
		}

		public override string ToString()
		{
			return $"File: {Info.FullName} |AssemblyVersion: {AssemblyVersion} |TargetFramework: {TargetFramework}";
		}
	}
}
