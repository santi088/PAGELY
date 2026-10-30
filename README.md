# PAGELY

Mobile reading and book-community app (.NET MAUI) with a separate ASP.NET Core backend.

## Projects

- Mobile app: `PAGELY` (this repository), target framework `net8.0-android`
- Backend API: `PAGELY-Book-Community/BookCommunity.Api`, target framework `net8.0`

## Prerequisites

- .NET SDK 8.0 (the MAUI project pins SDK 8.0.132 via `global.json`)
- .NET SDK 10.0 for the separate backend repository (do not change it)
- MAUI Android workload (`maui-android`)
- Android SDK at `~/Android/Sdk` (or set `ANDROID_HOME`/`ANDROID_SDK_ROOT`)
- JDK 17 (set `JAVA_HOME` to JDK 17; JDK 25 is not compatible with the .NET 8 Android workload)
- PostgreSQL for the backend
- For phone deployment: USB debugging enabled and the device authorized

## Environment variables (Linux)

Add to `~/.bashrc.d/dotnet-maui.sh` (or export in your shell):

```sh
export DOTNET_ROOT="/usr/lib64/dotnet"
export JAVA_HOME="/home/<user>/.local/opt/jdk-17"
export ANDROID_HOME="/home/<user>/Android/Sdk"
export ANDROID_SDK_ROOT="/home/<user>/Android/Sdk"
export PATH="$DOTNET_ROOT:$JAVA_HOME/bin:$ANDROID_HOME/platform-tools:$PATH"
```

## Backend setup

```sh
cd ~/Documents/PAGELY-Book-Community
dotnet restore
dotnet run --project BookCommunity.Api --urls "http://0.0.0.0:5242"
```

Verify:

```sh
curl http://127.0.0.1:5242/health
```

Configure the database connection and JWT key in `BookCommunity.Api/appsettings.json` or user secrets. Never commit real secrets.

## Mobile app setup

```sh
cd ~/Documents/PAGELY
source ~/.bashrc
dotnet restore
dotnet build -f net8.0-android
```

Deploy to a phone:

```sh
adb reverse tcp:5242 tcp:5242
dotnet build -t:Run -f net8.0-android
```

In the app Settings, set **Backend API base URL** to `http://127.0.0.1:5242` when using `adb reverse`. On an emulator, use `http://10.0.2.2:5242`.

## Windows (PowerShell) equivalents

Prerequisites: .NET SDK 8.0/10.0, MAUI workload, Android SDK, JDK 17.

Backend:

```powershell
cd $HOME\Documents\PAGELY-Book-Community
dotnet restore
dotnet run --project BookCommunity.Api --urls "http://0.0.0.0:5242"
```

Mobile:

```powershell
cd $HOME\Documents\PAGELY
dotnet restore
dotnet build -f net8.0-android
adb reverse tcp:5242 tcp:5242
dotnet build -t:Run -f net8.0-android
```

## Troubleshooting

- `XA5300`: set `ANDROID_HOME`/`ANDROID_SDK_ROOT` or pass `-p:AndroidSdkDirectory=...`.
- JDK 25 is not supported by the .NET 8 Android workload; use JDK 17.
- Physical phone `localhost` refers to the phone, not the dev machine. Use `adb reverse` or the laptop's LAN IP.
- If install fails with `INSTALL_FAILED_USER_RESTRICTED`, enable `Install via USB` and disable MIUI optimization.
- Fast Deployment crashes (`No assemblies found`) can be fixed by a full `dotnet build -t:Run`.
