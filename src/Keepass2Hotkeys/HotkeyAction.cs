using System;
using System.Windows.Forms;

namespace Keepass2Hotkeys
{
    internal sealed class HotkeyAction
    {
        public Keys Hotkey { get; set; }
        public string Sequence { get; set; }

        public HotkeyAction Clone()
        {
            return new HotkeyAction { Hotkey = Hotkey, Sequence = Sequence };
        }

        public string DisplayHotkey
        {
            get { return new KeysConverter().ConvertToString(Hotkey); }
        }

        public override string ToString()
        {
            return DisplayHotkey + "  " + Sequence;
        }
    }
}
