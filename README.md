# WinMDF
### Windows Multiple Displays Fix - is a program which makes it easier to use laptop with external display(s).

There is quite an old bug which resets multiple displays mode when the laptop lid is being opened (see [here](https://answers.microsoft.com/en-us/windows/forum/all/how-to-prevent-display-settings-from-re-setting/0566d918-f27f-4c4a-b755-8bb1661d31ea), [here](https://superuser.com/questions/1231659/how-to-keep-display-settings-after-opening-laptop-lid-it-resets-to-first-scree/1407953) and [here](https://superuser.com/questions/437115/how-to-prevent-monitor-reconfiguration-upon-opening-of-laptop-lid)). 

This program (WPF app) uses quite a dirty way to fix the bug. Basically, it detects when you open the lid of your laptop and re-sets multiple displays mode to preferred one.

You can download the app from [Releases](https://github.com/nVoxel/WinMDF/releases) or build it from sources.

## Build
Clone the repository and build the application in Visual Studio.

### Build Requirements

 - Visual Studio 2019 or newer
 - .NET desktop development workload
 - .NET Framework 4.8

## Configuration

Configuration of the application is stored in App.config file.

 - PreferredDisplayMode - Multiple displays mode to be set after opening the lid
	 - 1 - Clone
	 - 2 - Extend
	 - 3 - Internal only
	 - 4 - External only (Default)

 - ShowNotification- Show WinMDF icon in notification area
	 - true - Do (Default)
	 - false - Don't

 - AddToStartup- Add WinMDF to startup
	 - true - Do (Default)
	 - false - Don't