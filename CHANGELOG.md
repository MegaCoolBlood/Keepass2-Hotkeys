# Changelog

All notable changes to KeePass OTP Hotkeys are documented in this file.

## [0.1.0] - 2026-10-05

### Added

- Global hotkeys for KeePass Auto-Type.
- Default hotkey `Ctrl+Alt+T` with `{TIMEOTP}{ENTER}`.
- Configuration of multiple hotkeys and Auto-Type expressions.
- KeePass-native global entry matching and multi-match selection.
- Detection and display of hotkeys already registered by another program.
- IDE-style Auto-Type expression completion.
- Dynamic completion entries from `SprEngine.FilterPlaceholderHints`.
- German and English user interface text.
- Unit test executable for the Auto-Type sequence override.

### Notes

- `{TIMEOTP}` is provided by the installed OTP integration or plugin, not by KeePass itself.
- The plugin currently targets Windows and .NET Framework 4.8.
