using System.Globalization;

namespace Keepass2Hotkeys
{
    internal static class PluginStrings
    {
        private static bool IsGerman
        {
            get
            {
                return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                    .Equals("de", System.StringComparison.OrdinalIgnoreCase);
            }
        }

        public static string Get(string key)
        {
            switch (key)
            {
                case "PluginName": return "KeePass2Hotkeys";
                case "GlobalHotkeys": return IsGerman ? "Globale Hotkeys..." : "Global Hotkeys...";
                case "Hotkey": return IsGerman ? "Tastenkombination" : "Hotkey";
                case "Expression": return IsGerman ? "Ausdruck" : "Expression";
                case "Status": return IsGerman ? "Status" : "Status";
                case "Add": return IsGerman ? "Hinzufügen" : "Add";
                case "Update": return IsGerman ? "Aktualisieren" : "Update";
                case "Remove": return IsGerman ? "Entfernen" : "Remove";
                case "SaveAndClose": return IsGerman ? "Speichern und schließen" : "Save and close";
                case "Cancel": return IsGerman ? "Abbrechen" : "Cancel";
                case "Available": return IsGerman ? "Verfügbar" : "Available";
                case "Occupied": return IsGerman ? "Warnung: bereits belegt" : "Warning: already in use";
                case "InvalidHotkey": return IsGerman ? "Ungültiger Hotkey" : "Invalid hotkey";
                case "DuplicateHotkey": return IsGerman ? "Doppelter Hotkey" : "Duplicate hotkey";
                case "NoHotkeys": return IsGerman ? "Keine Hotkeys" : "No hotkeys";
                case "EnterHotkeyAndExpression":
                    return IsGerman ? "Bitte Tastenkombination und Ausdruck angeben." :
                        "Please enter a hotkey and an expression.";
                case "DuplicateHotkeyMessage":
                    return IsGerman ? "Diese Tastenkombination ist bereits vorhanden." :
                        "This hotkey already exists.";
                case "AtLeastOneHotkey":
                    return IsGerman ? "Mindestens ein globaler Hotkey ist erforderlich." :
                        "At least one global hotkey is required.";
                case "HotkeyRegistrationFailed":
                    return IsGerman ? "Der Hotkey ist bereits belegt oder konnte nicht registriert werden." :
                        "The hotkey is already in use or could not be registered.";
                case "PluginProvided":
                    return IsGerman ? "Von einem KeePass-Plugin bereitgestellt" :
                        "Provided by a KeePass plugin";
                case "Username": return IsGerman ? "Benutzername des Eintrags" : "Entry username";
                case "Password": return IsGerman ? "Passwort des Eintrags" : "Entry password";
                case "TimeOtp": return IsGerman ? "Aktueller TOTP-Wert" : "Current TOTP value";
                case "HmacOtp": return IsGerman ? "Aktueller HOTP-Wert" : "Current HOTP value";
                case "Title": return IsGerman ? "Titel des Eintrags" : "Entry title";
                case "Url": return IsGerman ? "URL des Eintrags" : "Entry URL";
                case "Notes": return IsGerman ? "Notizen des Eintrags" : "Entry notes";
                case "Enter": return IsGerman ? "Enter-Taste senden" : "Send Enter key";
                case "Tab": return IsGerman ? "Tab-Taste senden" : "Send Tab key";
                case "Space": return IsGerman ? "Leerzeichen senden" : "Send space";
                case "Delay": return IsGerman ? "Verzögerung in Millisekunden" : "Delay in milliseconds";
                case "ClearField": return IsGerman ? "Eingabefeld leeren" : "Clear input field";
                case "Home": return IsGerman ? "Home-Taste senden" : "Send Home key";
                case "End": return IsGerman ? "End-Taste senden" : "Send End key";
                default: return key;
            }
        }
    }
}
