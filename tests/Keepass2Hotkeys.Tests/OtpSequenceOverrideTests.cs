using System;

using KeePass.Util;

namespace Keepass2Hotkeys.Tests
{
    internal static class OtpSequenceOverrideTests
    {
        private static int s_assertions;

        private static void AssertEqual(string expected, string actual, string message)
        {
            s_assertions++;
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
                throw new InvalidOperationException(message);
        }

        private static AutoTypeEventArgs CreateArgs(string sequence)
        {
            return new AutoTypeEventArgs(sequence, false, null, null);
        }

        public static void Run()
        {
            Keepass2Hotkeys.OtpSequenceOverride overrideSequence =
                new Keepass2Hotkeys.OtpSequenceOverride();

            AutoTypeEventArgs unchanged = CreateArgs("{USERNAME}{TAB}{PASSWORD}");
            overrideSequence.Apply(unchanged);
            AssertEqual("{USERNAME}{TAB}{PASSWORD}", unchanged.Sequence,
                "Disabled OTP override changed the sequence.");

            overrideSequence.Enabled = true;
            AutoTypeEventArgs otp = CreateArgs("{USERNAME}");
            overrideSequence.Apply(otp);
            AssertEqual("{TIMEOTP}", otp.Sequence,
                "Enabled OTP override did not use the OTP sequence.");

            overrideSequence.Enabled = false;
            overrideSequence.Apply(otp);
            AssertEqual("{TIMEOTP}", otp.Sequence,
                "Disabling the override unexpectedly rewrote an existing sequence.");

            Console.WriteLine("PASS: {0} assertions", s_assertions);
        }

        public static int Main()
        {
            try
            {
                Run();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FAIL: " + ex.Message);
                return 1;
            }
        }
    }
}
