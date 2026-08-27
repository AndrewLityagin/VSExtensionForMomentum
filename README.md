This is Visual Studio Extension (v 1.2.3) for replacing .dll, .exe and resource files from repository into BrightEye's Momentum Instance. It is actual for developers and integrators of Momentum. 
How to start:
1. Clone repos and compile project. Compilation required component "_Visual Studio extension development_".
2. Install *.vsix file.
3. Open Visual Studio : Tools -> Options -> VSExtensionForMomentum
4. Fill fields:
![image](https://github.com/user-attachments/assets/4faea18f-f942-421a-91ac-4e014410300e)
- _.Net version_ : version .net of project ( some version of Momentum has .net6 web projects, some .net8). It is required for replacing wwwroot folders.
- _Custom project name_ : if you work with custom project, you should insert project name into this field. This field can be empty if you work with MEScontrol.net Standard or Extensions.
- _Custom sourse folder_ and _Custom target folder_ : if you want to replace all files from sourse folder to target folder you should fill this fields.
- _Instance folder_ : Is required field, you need to add path of your Momentum Instance.
- _Kill the locking process_ : Momentum servers will be automatically closed if they block replacing files.  
- _Minutes after build_ : This field setup's how many minutes after build builded files can be replaced for Instance.
- _Repository folder_ : Folder with repository of MEScontrol.net Standard or Extensions or Custom project.
- _Start processes_ : Killed momentum serves will automatically run after replacing files.
5. Build your project
6. Open Tools and run command "_Replace binary files_"
![image](https://github.com/user-attachments/assets/3b5b83f1-877d-4d62-b367-11e86fbc6337)
7. Builded files will be replaced. You can see result in Output log 
![image](https://github.com/AndrewLityagin/VSExtensionForMomentum/assets/99161672/8c4cea89-ca3a-4b0d-929a-61c1e3759bab)
8. Enjoy :relaxed:
  
Settings from a file in the repository:

Instead of filling the options page every time you switch to another mirror, you can store the settings in the repository. Create a file **momentum.replacefiles.json** near your solution (or in any parent folder, for example in the root of the repository):
```json
{
  "RepositoryFolder": ".",
  "InstanceFolder": "C:\\Path\\To\\Your\\Momentum\\Instance",
  "NetVersion": "net8.0",
  "Minutes": 20,
  "KillProcess": true,
  "StartProcesses": true
}
```
- The file is searched from the folder of the opened solution up to the root of the disk, and after that from the _Repository folder_ of the options page. The first found file is used.
- Names of the settings are the same as properties of the options page : _RepositoryFolder_, _InstanceFolder_, _Minutes_, _CustomSourceFolder_, _CustomTargetFolder_, _CustomProjectName_, _NetVersion_, _KillProcess_, _StartProcesses_. Character case in names is not important.
- Paths can be written in any style : with forward slashes `"D:/ProgramData/BrightEye/Momentum/Momentum_m1"` or with doubled backslashes `"D:\\ProgramData\\BrightEye\\Momentum\\Momentum_m1"` (a single backslash is not allowed by json).
- Relative folder paths are resolved against the folder of the settings file, so `"RepositoryFolder": "."` means "the folder with this file".
- Every setting which is absent, empty or not valid is taken from the options page (Tools -> Options -> VS Extension For Momentum). If the file does not exist or contains invalid json - all settings are taken from the options page.
- The used file and the resolved folders are written into the Output log, so you can always check which configuration is applied.

See [momentum.replacefiles.example.json](momentum.replacefiles.example.json).

Remarks:
- For auto-closing and auto-running Momementum Servers Visual Studio must be run as administrator.
- if replacing is not success - try to allowed changes for folder (with instance) in Windows security and close all servers or services of Momentum Instance.
- Commands "_Replace Supervisor wwwroot folder_" and  "_Replace Operator wwwroot folder_" replace styles, js  and fonts files from MEScontrol.net Standard. For Extensions and Custom project this command will not work.
- Command "_Replace custom folder_" replace all files from _Custom sourse folder_ to _Custom target folder_.
