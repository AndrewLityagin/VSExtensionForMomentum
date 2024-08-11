using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace VSExtensionForMomentum
{
    public static class ProcessKiller
    {
        private static readonly List<(string Folder, string ExeFile)> folderAndExeFiles = new()
        {
            (@"\Server\","Momentum.Upgrade"),
            (@"\Server\","Momentum.Server"),
            (@"\Server\","MEScontrol.DataCenter.Server"),
            (@"\Server\","Momentum.PrintServer"),
            (@"\Server\","MEScontrol.OpcEmulator"),
            (@"\WebServer\","Momentum.Web.OEE"),
            (@"\WebServer\","Momentum.Web.Supervisor"),
            (@"\WebServer\","Momentum.Web.Operator"),
            (@"\WebServer\","Momentum.Web.StoryBook"),
            (@"\WebServer\","Momentum.Web.WebServices.Doc"),
            (@"\WebServer\","Momentum.WebServer"),
            (@"\Shell\","Momentum.Manager"),
            (@"\Shell\","MEScontrol.Configuration"),
            (@"\DMS\","MEScontrol.DMS.Upgrade"),
            (@"\DMS\","MEScontrol.DMS.Server"),
        };

        private static readonly string[] folders = folderAndExeFiles.Select(fef => fef.Folder).Distinct().ToArray();

        public static async void Kill(BinaryFileInfo target)
        {
            try
            {
                foreach(var folder in folders){
                    if(target.Info.FullName.Contains(folder)){
                        var processes = folderAndExeFiles.Where(fef => fef.Folder == folder).SelectMany(fef => Process.GetProcessesByName(fef.ExeFile));
                        foreach(var process in processes){
                            process.Kill();
                            Logger.AddLine(LogType.Info, $"Process is killed:{process.ProcessName}");
                        }
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.AddLine(LogType.Error, $"Error in process killing for file:{target.Info.FullName}");
            }
        }
    }
}
