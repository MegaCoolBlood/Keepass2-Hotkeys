using System;
using System.Collections.Generic;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePass.Util;

namespace Keepass2Hotkeys
{
    public sealed class Keepass2HotkeysExt : Plugin
    {
        private const Keys OtpHotKey = Keys.Control | Keys.Alt | Keys.T;

        private IPluginHost m_host;
        private GlobalHotKey m_hotKey;
        private readonly OtpSequenceOverride m_otpSequence = new OtpSequenceOverride();

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            if (host.MainWindow == null) return false;

            m_host = host;
            AutoType.FilterCompilePre += OnFilterCompilePre;

            try
            {
                m_hotKey = new GlobalHotKey(host.MainWindow, OtpHotKey);
                m_hotKey.Pressed += OnHotKeyPressed;
                m_hotKey.Register();
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception ||
                                       ex is InvalidOperationException)
            {
                AutoType.FilterCompilePre -= OnFilterCompilePre;
                if (m_hotKey != null) m_hotKey.Dispose();
                m_hotKey = null;
                MessageBox.Show(host.MainWindow,
                    ex.Message,
                    "KeePass OTP Hotkeys",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        public override void Terminate()
        {
            AutoType.FilterCompilePre -= OnFilterCompilePre;

            if (m_hotKey != null)
            {
                m_hotKey.Pressed -= OnHotKeyPressed;
                m_hotKey.Dispose();
                m_hotKey = null;
            }

            m_host = null;
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            if (t != PluginMenuType.Main) return null;

            ToolStripMenuItem item = new ToolStripMenuItem(
                "Global OTP Auto-Type (Ctrl+Alt+T)");
            item.Click += OnHotKeyPressed;
            return item;
        }

        private void OnHotKeyPressed(object sender, EventArgs e)
        {
            if (m_host == null || m_host.MainWindow == null) return;

            List<KeePassLib.PwDatabase> databases =
                m_host.MainWindow.DocumentManager.GetOpenDatabases();
            if (databases == null || databases.Count == 0) return;

            m_otpSequence.Enabled = true;
            try
            {
                AutoType.PerformGlobal(databases, m_host.MainWindow.ClientIcons);
            }
            finally
            {
                m_otpSequence.Enabled = false;
            }
        }

        private void OnFilterCompilePre(object sender, AutoTypeEventArgs e)
        {
            m_otpSequence.Apply(e);
        }
    }
}
