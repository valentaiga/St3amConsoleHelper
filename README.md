<h1 align="center">
  Steam Console Helper
</h1>
<p align="center">
  Steam cards automatic sell implementation.
</p>
<p align="center">.Net Core 3.1 REQUIRED</p>
<br>

## Detailed setup instructions
 - Download & Install [.NET Core 3.1](https://dotnet.microsoft.com/download)
 - Create telegram bot and change telegram credentials in "appsettings.json" (used for steam authentication)
 - Run app

## Start a background service [FUTURE]
 - First you need to publish a project with "Release" into your hidden folder
 - **Windows:** open your "Powershell", than type "sc.exe create SteamConsoleHelper binpath=<yourpath> start= auto"
 - **Linux:** I havent work with linux, find it by yourself

