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
  
Remarks:
- For auto-closing and auto-running Momementum Servers Visual Studio must be run as administrator.
- if replacing is not success - try to allowed changes for folder (with instance) in Windows security and close all servers or services of Momentum Instance.
- Commands "_Replace Supervisor wwwroot folder_" and  "_Replace Operator wwwroot folder_" replace styles, js  and fonts files from MEScontrol.net Standard. For Extensions and Custom project this command will not work.
- Command "_Replace custom folder_" replace all files from _Custom sourse folder_ to _Custom target folder_.
