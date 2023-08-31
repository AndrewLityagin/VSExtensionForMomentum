using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VSExtensionForMomentum
{
	[ProvideOptionPage(typeof(OptionsProvider.SettingsOptions), "VS Extension For Momentum", "Settings", 0, 0, true)]
	[ProvideProfile(typeof(OptionsProvider.SettingsOptions), "VS Extension For Momentum", "Settings", 0, 0, true)]
	public sealed class OptionsPackage : ToolkitPackage
	{

	}

	internal partial class OptionsProvider
	{
		[ComVisible(true)]
		public class SettingsOptions : BaseOptionPage<Settings> { }
	}

	public class Settings : BaseOptionModel<Settings>
	{
		[Category("General")]
		[DisplayName("Repository folder")]
		[Description("Folder with repository")]
		[DefaultValue("")]
		public string RepositoryFolder { get; set; }

		[Category("General")]
		[DisplayName("Instance folder")]
		[Description("Folder with servers")]
		[DefaultValue("")]
		public string InstanceFolder { get; set; }

		[Category("General")]
		[DisplayName("Minutes after build")]
		[Description("Time period for searching updated binary files")]
		[DefaultValue(5)]
		public int Minutes { get; set; }
	}
}
