using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace VSExtensionForMomentum
{
    public class ProcessWorker
    {
        private static readonly List<(string Folder, string ExeFile)> folderAndExeFiles = new()
        {
            (@"\Server\","Momentum.Upgrade"),
            (@"\Server\","Momentum.Server"),
            (@"\Server\","MEScontrol.DataCenter.Server"),
            (@"\Server\","Momentum.PrintServer"),
            (@"\WebServer\","Momentum.WebServer"),
            (@"\Shell\","Momentum.Manager"),
            (@"\Shell\","MEScontrol.Configuration"),
            (@"\DMS\","MEScontrol.DMS.Upgrade"),
            (@"\DMS\","MEScontrol.DMS.Server"),
        };

        private static readonly string[] folders = folderAndExeFiles.Select(fef => fef.Folder).Distinct().ToArray();

        private readonly List<(string Folder, string ExeFile)> exeFilesForRestart;

        private readonly string instanceFolder;

        public async void Kill(BinaryFileInfo target)
        {
            try
            {
                foreach(var folder in folders){
                    if(target.Info.FullName.Contains(folder)){
                        var processes = folderAndExeFiles.Where(fef => fef.Folder == folder).SelectMany(fef => Process.GetProcessesByName(fef.ExeFile));
                        exeFilesForRestart.AddRange(folderAndExeFiles.Where(fef => fef.Folder == folder));
                        foreach(var process in processes){
                            process.Kill();
                            Logger.AddLine(LogType.Info, $"Process is killed : {process.ProcessName}");
                        }
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.AddLine(LogType.Error, $"Error in process killing for file {target.Info.FullName} : {ex.Message}");
            }
        }

        public void RestartProcesses()
        {
            try
            {
                if(exeFilesForRestart == null || exeFilesForRestart.Count == 0)
                    return;

                foreach(var file in exeFilesForRestart.Where(fef => !fef.ExeFile.Contains("Upgrade") && !fef.ExeFile.Contains("Configuration")).Distinct())
                {
                    var runCommand = $"{instanceFolder}{file.Folder}{file.ExeFile}.exe";
                    if(file.ExeFile.Contains("WebServer"))
                        Process.Start(runCommand, "-i");
                    else if(file.ExeFile.Contains("Manager"))
                        Process.Start(runCommand, "/i /USERNAME=\"Administrator\" /PASSWORD=\"password\"");
                    else
                        Process.Start(runCommand, "/i");
                    Logger.AddLine(LogType.Info, $"Process is restarted : {file.ExeFile}");
                }
            }
            catch(Exception ex)
            {
                Logger.AddLine(LogType.Error, $"Error in process restarting : {ex.Message}");
            }
            finally
            {    
                if(exeFilesForRestart != null)
                    exeFilesForRestart.Clear();
            }
        }

        public ProcessWorker(string _instanceFolder)
        {
            instanceFolder = _instanceFolder;
            exeFilesForRestart = new List<(string Folder, string ExeFile)>();
        }
    }
}
