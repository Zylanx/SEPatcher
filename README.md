# SEPatcher
A Patcher and Mod Loader for the game Stationeers

# Project Structure
SEPatcher - This project contains the library that does all the assembly patching  
SEPatcherConsole - This project contains a _very_ basic command-line frontend that will be changed and replaced with a GUI in future  
SEModLoader - This project contains the actual Mod Loader DLL file  
HelmetLockMod - This is a very simple mod that adds a "Lock Helmet" keybind to the controls menu, that when pushed, toggles the mask lock state  

# Using The Patcher
In order to use the patcher, open up the command line and navigate to the folder that you extracted it to.  
Then just type in 'SEPatcher.exe "Drive:/Path/To/Stationeers/Folder"'  
Example: 'SEPatcher.exe "C:/Progam Files(x86)/Steam/steamapps/common/Stationeers/"  
> Warning: The Patcher does not currently restore the original assembly before backing up and patching again.  
>          So make sure to go into the backup "Unpatched" folder in the Stationeers directory and restore it before trying to patch again.

# Installing HelmetLockMod
In order to install the mod, first make sure you have patched your folder.  
Then simply extract the zip file into the Stationeers directory. Make sure the DLL goes into the "Mods" folder.  
Then just launch the game.

# Warning
This is currently very WIP and is effectively just quick prototype  
The code is uncommented currently as it will soon be completely rewritten to have a much improved interface and API.  
The Mod Loader will also become more of a Mod Manager and Mod API with Loader, to make things easier  

In other words, this is just for fun. Don't be too mean about it  
