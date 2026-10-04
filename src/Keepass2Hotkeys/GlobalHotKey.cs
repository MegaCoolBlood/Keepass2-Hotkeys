using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Keepass2Hotkeys
{
    internal sealed class GlobalHotKey : NativeWindow, IDisposable
    {
        private const int WmHotKey = 0x0312;
        private const uint ModAlt = 0x0001;
        private const uint ModControl = 0x0002;
        private const uint ModShift = 0x0004;
        private const uint ModWin = 0x0008;
        private const uint ModNoRepeat = 0x4000;

        private static int s_nextId = 0x4B32;
        private readonly int m_id = Interlocked.Increment(ref s_nextId);
        private readonly Keys m_key;
        private bool m_registered;

        public event EventHandler Pressed;

        public GlobalHotKey(Control target, Keys key)
        {
            if (target == null) throw new ArgumentNullException("target");

            m_key = key;
            AssignHandle(target.Handle);
        }

        public void Register()
        {
            if (m_registered) return;

            uint modifiers = GetNativeModifiers(m_key) | ModNoRepeat;
            if (!RegisterHotKey(Handle, m_id, modifiers, (uint)(m_key & Keys.KeyCode)))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(),
                    "The OTP hotkey is already registered or cannot be registered.");
            }

            m_registered = true;
        }

        private static uint GetNativeModifiers(Keys key)
        {
            Keys modifiers = key & Keys.Modifiers;
            uint nativeModifiers = 0;

            if ((modifiers & Keys.Alt) != Keys.None) nativeModifiers |= ModAlt;
            if ((modifiers & Keys.Control) != Keys.None) nativeModifiers |= ModControl;
            if ((modifiers & Keys.Shift) != Keys.None) nativeModifiers |= ModShift;
            if ((modifiers & Keys.LWin) != Keys.None) nativeModifiers |= ModWin;

            return nativeModifiers;
        }

        public void Dispose()
        {
            if (m_registered)
            {
                UnregisterHotKey(Handle, m_id);
                m_registered = false;
            }

            ReleaseHandle();
        }

        protected override void WndProc(ref Message message)
        {
            if ((message.Msg == WmHotKey) &&
                (message.WParam.ToInt32() == m_id))
            {
                EventHandler handler = Pressed;
                if (handler != null) handler(this, EventArgs.Empty);
            }

            base.WndProc(ref message);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(
            IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
