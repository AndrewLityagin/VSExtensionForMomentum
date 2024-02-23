This is Visual Studio Extension for replacing .dll, .exe and resource files from repository into BrightEye's Momentum Instance. It will be actual for developers and integrators of Momentum. 
How to start:
1. Clone repos and compile project. Compilation required component "_Visual Studio extension development_".
2. Install *.vsix file.
3. Open Visual Studio : Tools -> Options -> VSExtensionForMomentum
4. Fill fields:
![image](https://github.com/AndrewLityagin/VSExtensionForMomentum/assets/99161672/21389669-c3c8-47f4-ab29-1212fadc193e)
- _Custom project name_ : if you work with custom project, you should insert project name into this field. This field can be empty if you work with MEScontrol.net Standard or Extensions.
- _Custom sourse folder_ and _Custom target folder_ : if you want to replace all files from sourse folder to target folder you should fill this fields.
- _Instance folder_ : Is required field, you need to add path of your Momentum Instance.
- _Minutes after build_ : This field setup's how many minutes after build builded files can be replaced for Instance.
- _Repository folder_ : Folder with repository of MEScontrol.net Standard or Extensions or Custom project.
5. Build your project
6. Open Tools and run command "_Replace binary files_"
  ![Screenshot 2024-02-23 215106](https://github.com/AndrewLityagin/VSExtensionForMomentum/assets/99161672/3072d560-6278-4d76-8403-603ac69bdc98)
7. Builded files will be replaced. You can see result in Output log 
![image](https://github.com/AndrewLityagin/VSExtensionForMomentum/assets/99161672/8c4cea89-ca3a-4b0d-929a-61c1e3759bab)

Remarks: 
- if replacing is not success - try to allowed changes for folder (with instance) in Windows security and close all servers or services of Momentum Instance.
- Command "_Replace Supervisor wwwroot folder_" replace styles, js  and fonts files from MEScontrol.net Standard. For Extensions and Custom project this command will not work.
- Command "_Replace custom folder_" replace all files from _Custom sourse folder_ to _Custom target folder_.
