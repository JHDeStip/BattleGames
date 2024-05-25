# Battle Games
The Battle Games suite consists of two applications.  
One is `Stipstonks`, which is a basic implementation of the `beursfuif` game.  
The other is `Beer Battle`, which lets different groups compete to drink the most.

Detailed user guides for the individual applications are available in separate files:
* [Stipstonks](Stipstonks.Readme.md)
* [Beer Battle](BeerBattle.Readme.md)

## Codebase structure
Both applications have their own project.  
The BattleGames.Common project contains shared infrastructure and UI compoments and styles.

## Building
The solution can be opened and built as normal in `Visual Studio`.

To publish a new release, first make sure the version is set correctly in `GlobalAssemblyInfo.cs`, the run the `UtilityScripts/Publish.ps1` script. If the script finishes successfully, a zip file for each application should exist inside the `Output` folder.