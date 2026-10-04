using System;
using System.Collections.Generic;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePass.Util;

namespace Keepass2Hotkeys
{
    public sealed class Keepass2HotkeysExt : Plugin
    {
        private IPluginHost m_host;
        private readonly List<GlobalHotKey> m_hotKeys = new List<GlobalHotKey>();
        private List<HotkeyAction> m_actions;
        private readonly OtpSequenceOverride m_otpSequence = new OtpSequenceOverride();

        public override bool Initialize(IPluginHost host)
        {
            if (host == null) return false;
            if (host.MainWindow == null) return false;

            m_host = host;
            m_actions = HotkeySettings.Load(host.CustomConfig);
            AutoType.FilterCompilePre += OnFilterCompilePre;

            try
            {
                RegisterHotkeys();
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception ||
                                       ex is InvalidOperationException)
            {
                AutoType.FilterCompilePre -= OnFilterCompilePre;
                DisposeHotkeys();
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

            DisposeHotkeys();

            m_host = null;
        }

        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            if (t != PluginMenuType.Main) return null;

            ToolStripMenuItem item = new ToolStripMenuItem("Global Hotkeys...");
            item.Click += OnSettingsClicked;
            return item;
        }

        private void RegisterHotkeys()
        {
            DisposeHotkeys();
            foreach (HotkeyAction action in m_actions)
            {
                GlobalHotKey hotKey = new GlobalHotKey(m_host.MainWindow, action.Hotkey);
                hotKey.Pressed += delegate
                {
                    ExecuteHotkey(action);
                };
                hotKey.Register();
                m_hotKeys.Add(hotKey);
            }
        }

        private void DisposeHotkeys()
        {
            foreach (GlobalHotKey hotKey in m_hotKeys) hotKey.Dispose();
            m_hotKeys.Clear();
        }

        private void ExecuteHotkey(HotkeyAction action)
        {
            if (m_host == null || m_host.MainWindow == null) return;

            List<KeePassLib.PwDatabase> databases =
                m_host.MainWindow.DocumentManager.GetOpenDatabases();
            if (databases == null || databases.Count == 0) return;

            m_otpSequence.Enabled = true;
            m_otpSequence.Sequence = action.Sequence;
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

        private void OnSettingsClicked(object sender, EventArgs e)
        {
            using (HotkeySettingsForm form = new HotkeySettingsForm(m_actions))
            {
                if (form.ShowDialog(m_host.MainWindow) != DialogResult.OK) return;

                List<HotkeyAction> updated = new List<HotkeyAction>();
                foreach (HotkeyAction action in form.Actions)
                    updated.Add(action.Clone());

                try
                {
                    HotkeySettings.Save(m_host.CustomConfig, updated);
                    m_actions = updated;
                    RegisterHotkeys();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(m_host.MainWindow, ex.Message, "KeePass OTP Hotkeys",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
