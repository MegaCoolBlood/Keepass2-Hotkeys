# KeePass OTP Hotkeys

KeePass 2.x plugin MVP that adds a global OTP Auto-Type hotkey.

## Current behavior

- Hotkey: `Ctrl+Alt+T`
- Default OTP Auto-Type sequence: `{TIMEOTP}{ENTER}`
- Additional global hotkeys and their Auto-Type expressions can be managed through `Tools > Global Hotkeys...`.
- Entry matching and the multi-match selection dialog come from KeePass' native global Auto-Type implementation.
- The selected entry is then compiled and sent through KeePass Auto-Type, so existing OTP placeholder providers remain responsible for generating the code.

The `{TIMEOTP}` placeholder must be provided by the OTP plugin or Auto-Type integration installed by the user. Each configured expression is passed to KeePass Auto-Type unchanged.

## Build

The plugin targets .NET Framework 4.8 and references the KeePass executable of the intended KeePass installation. Install the .NET Framework 4.8 developer pack and Visual Studio or MSBuild, then run:

```powershell
msbuild Keepass2Hotkeys.sln /p:Configuration=Release /p:KeePassDir="C:\Program Files\KeePass Password Safe 2"
```

Copy `src\Keepass2Hotkeys\bin\Release\Keepass2Hotkeys.dll` to KeePass' `Plugins` directory.

The plugin currently uses the public `AutoType.PerformGlobal` entry point and its public compile filter event. This preserves KeePass' global matching and selection behavior while replacing the selected entry's sequence with the OTP sequence at compile time.

## Test

Build and run the dependency-free test executable with the same KeePass installation:

```powershell
msbuild tests\Keepass2Hotkeys.Tests\Keepass2Hotkeys.Tests.csproj `
  /t:Rebuild `
  /p:Configuration=Release `
  /p:KeePassDir="C:\Program Files\KeePass Password Safe 2"

tests\Keepass2Hotkeys.Tests\bin\Release\Keepass2Hotkeys.Tests.exe
```

The test verifies that normal Auto-Type sequences are preserved and that the OTP sequence is applied only while the OTP action is active.
