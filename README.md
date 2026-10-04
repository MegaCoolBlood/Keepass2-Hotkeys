# KeePass OTP Hotkeys

KeePass 2.x plugin for configurable global Auto-Type hotkeys.

## Current behavior

- Hotkey: `Ctrl+Alt+T`
- Default OTP Auto-Type sequence: `{TIMEOTP}{ENTER}`
- Additional global hotkeys and their Auto-Type expressions can be managed through `Tools > Global Hotkeys...`.
- Hotkeys already registered by another program are shown with `Warnung: bereits belegt` and are not registered by the plugin.
- The expression field provides IDE-style completion: type `{` to filter KeePass placeholders, use `Up`/`Down` to select, `Enter`/`Tab` to insert, `Esc` to close, or `Ctrl+Space` to show all suggestions.
- Entry matching and the multi-match selection dialog come from KeePass' native global Auto-Type implementation.
- The selected entry is then compiled and sent through KeePass Auto-Type, so existing OTP placeholder providers remain responsible for generating the code.

The `{TIMEOTP}` placeholder must be provided by the OTP plugin or Auto-Type integration installed by the user. Each configured expression is passed to KeePass Auto-Type unchanged.

## Requirements

- KeePass 2.x on Windows
- .NET Framework 4.8
- An OTP plugin or integration that provides `{TIMEOTP}` for OTP Auto-Type

## Installation

1. Download the latest release archive from the repository's GitHub Releases page.
2. Extract `Keepass2Hotkeys.dll`.
3. Close KeePass.
4. Copy the DLL to KeePass' `Plugins` directory.
5. Start KeePass and verify the plugin under `Tools > Plugins`.
6. Configure hotkeys under `Tools > Global Hotkeys...`.

The plugin does not include an OTP provider. If `{TIMEOTP}` is unavailable in the installed KeePass setup, configure an expression supported by your OTP integration.

## Build

The plugin targets .NET Framework 4.8 and references the KeePass executable of the intended KeePass installation. Install the .NET Framework 4.8 developer pack and Visual Studio or MSBuild, then run:

```powershell
msbuild Keepass2Hotkeys.sln /p:Configuration=Release /p:KeePassDir="C:\Program Files\KeePass Password Safe 2"
```

Copy `src\Keepass2Hotkeys\bin\Release\Keepass2Hotkeys.dll` to KeePass' `Plugins` directory.

The plugin currently uses the public `AutoType.PerformGlobal` entry point and its public compile filter event. This preserves KeePass' global matching and selection behavior while replacing the selected entry's sequence with the OTP sequence at compile time.

## Test

Build and run the test executable with the same KeePass installation:

```powershell
msbuild tests\Keepass2Hotkeys.Tests\Keepass2Hotkeys.Tests.csproj `
  /t:Rebuild `
  /p:Configuration=Release `
  /p:KeePassDir="C:\Program Files\KeePass Password Safe 2"

tests\Keepass2Hotkeys.Tests\bin\Release\Keepass2Hotkeys.Tests.exe
```

The test verifies that normal Auto-Type sequences are preserved and that the OTP sequence is applied only while the OTP action is active.

## Release package

A release archive should contain:

```text
Keepass2Hotkeys.dll
README.md
LICENSE
CHANGELOG.md
```

The source code is available in this repository and is released under the
GNU General Public License version 2 or any later version. See [LICENSE](LICENSE).

## KeePass compatibility

The plugin references the `KeePass.exe` from the KeePass installation it is
built and tested against. For best compatibility, build against the KeePass
version that will be used by the target installation.
