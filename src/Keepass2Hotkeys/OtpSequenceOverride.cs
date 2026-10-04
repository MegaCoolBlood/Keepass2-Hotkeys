using System;

using KeePass.Util;

namespace Keepass2Hotkeys
{
    internal sealed class OtpSequenceOverride
    {
        private const string OtpSequence = "{TIMEOTP}";

        public bool Enabled { get; set; }
        public string Sequence { get; set; }

        public void Apply(AutoTypeEventArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (Enabled) args.Sequence = string.IsNullOrEmpty(Sequence) ?
                OtpSequence : Sequence;
        }
    }
}
