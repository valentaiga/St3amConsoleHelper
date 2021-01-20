<h1 align="center">
  Steam Console Helper
</h1>
<p align="center">
  Steam cards automatic recycle implementation.
</p>
<p align="center">.Net Core 3.1 REQUIRED</p>
<br>

## Detailed setup instructions
- Download & Install [.NET Core 3.1](https://dotnet.microsoft.com/download).
- [FUTURE]: Type your login and password credentials into "appsettings.json".


## Start a background service
 - First you need to publish a project with "Release" into your hidden folder.
 - **Windows: open your "Powershell", than type "sc.exe create SteamConsoleHelper binpath= <yourpath> start= auto"**
 - **Linux:** I dont know, find it by yourself.

