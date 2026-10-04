using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

using KeePass.App.Configuration;

namespace Keepass2Hotkeys
{
    internal static class HotkeySettings
    {
        private const string ConfigKey = "Keepass2Hotkeys_Hotkeys";
        private const string Separator = "\t";
        private const string RecordSeparator = "\n";

        public static List<HotkeyAction> Load(AceCustomConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");

            string serialized = config.GetString(ConfigKey, null);
            if (serialized == null)
            {
                return new List<HotkeyAction>
                {
                    new HotkeyAction
                    {
                        Hotkey = Keys.Control | Keys.Alt | Keys.T,
                        Sequence = "{TIMEOTP}{ENTER}"
                    }
                };
            }

            List<HotkeyAction> result = new List<HotkeyAction>();
            foreach (string record in serialized.Split(new[] { RecordSeparator },
                StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = record.Split(new[] { Separator }, 2,
                    StringSplitOptions.None);
                int keyValue;
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out keyValue) ||
                    string.IsNullOrWhiteSpace(parts[1]))
                {
                    continue;
                }

                result.Add(new HotkeyAction
                {
                    Hotkey = (Keys)keyValue,
                    Sequence = parts[1]
                });
            }

            return result;
        }

        public static void Save(AceCustomConfig config, IEnumerable<HotkeyAction> actions)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (actions == null) throw new ArgumentNullException("actions");

            List<string> records = new List<string>();
            foreach (HotkeyAction action in actions)
            {
                if (action == null || string.IsNullOrWhiteSpace(action.Sequence))
                    continue;

                records.Add(((int)action.Hotkey).ToString(
                    CultureInfo.InvariantCulture) + Separator + action.Sequence);
            }

            config.SetString(ConfigKey, string.Join(RecordSeparator, records));
        }
    }
}
