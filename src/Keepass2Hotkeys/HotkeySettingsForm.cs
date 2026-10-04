using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Keepass2Hotkeys
{
    internal sealed class HotkeySettingsForm : Form
    {
        private readonly ListView m_list;
        private readonly TextBox m_hotkey;
        private readonly TextBox m_sequence;
        private readonly List<HotkeyAction> m_actions;
        private readonly Func<Keys, bool> m_isHotkeyAvailable;
        private readonly Button m_addButton;
        private int m_editingIndex = -1;

        public HotkeySettingsForm(IEnumerable<HotkeyAction> actions,
            Func<Keys, bool> isHotkeyAvailable)
        {
            if (isHotkeyAvailable == null) throw new ArgumentNullException("isHotkeyAvailable");

            m_isHotkeyAvailable = isHotkeyAvailable;
            m_actions = new List<HotkeyAction>();
            foreach (HotkeyAction action in actions) m_actions.Add(action.Clone());

            Text = "KeePass OTP Hotkeys";
            MinimumSize = new Size(600, 360);
            StartPosition = FormStartPosition.CenterParent;

            m_list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false
            };
            m_list.Columns.Add("Tastenkombination", 180);
            m_list.Columns.Add("Ausdruck", 360);
            m_list.Columns.Add("Status", 180);
            m_list.SelectedIndexChanged += OnSelectionChanged;

            m_hotkey = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            m_hotkey.PreviewKeyDown += OnHotkeyPreviewKeyDown;
            m_hotkey.KeyDown += OnHotkeyKeyDown;
            m_sequence = new TextBox { Dock = DockStyle.Fill };

            m_addButton = new Button { Text = "Hinzufügen", AutoSize = true };
            m_addButton.Click += OnAdd;
            Button remove = new Button { Text = "Entfernen", AutoSize = true };
            remove.Click += OnRemove;
            Button save = new Button { Text = "Speichern und schließen", DialogResult = DialogResult.OK, AutoSize = true };
            save.Click += OnSave;
            Button cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, AutoSize = true };

            TableLayoutPanel editor = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 92,
                ColumnCount = 2,
                Padding = new Padding(8)
            };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            editor.Controls.Add(new Label { Text = "Tastenkombination:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            editor.Controls.Add(m_hotkey, 1, 0);
            editor.Controls.Add(new Label { Text = "Ausdruck:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            editor.Controls.Add(m_sequence, 1, 1);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(8)
            };
            buttons.Controls.Add(m_addButton);
            buttons.Controls.Add(remove);
            buttons.Controls.Add(new Label { Width = 170 });
            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);

            Controls.Add(m_list);
            Controls.Add(editor);
            Controls.Add(buttons);
            AcceptButton = save;
            CancelButton = cancel;

            RefreshList();
        }

        public IList<HotkeyAction> Actions { get { return m_actions; } }

        private void OnHotkeyPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.PrintScreen) e.IsInputKey = true;
        }

        private void OnHotkeyKeyDown(object sender, KeyEventArgs e)
        {
            if (IsModifierKey(e.KeyCode)) return;

            Keys modifiers = e.Modifiers & Keys.Modifiers;
            if (modifiers == Keys.None && e.KeyCode != Keys.PrintScreen)
            {
                m_hotkey.Text = string.Empty;
                return;
            }

            m_hotkey.Tag = modifiers | e.KeyCode;
            m_hotkey.Text = new KeysConverter().ConvertToString(m_hotkey.Tag);
            m_editingIndex = FindActionIndex((Keys)m_hotkey.Tag);
            UpdateAddButtonText();
            e.SuppressKeyPress = true;
        }

        private static bool IsModifierKey(Keys key)
        {
            return key == Keys.Control || key == Keys.ControlKey ||
                key == Keys.LControlKey || key == Keys.RControlKey ||
                key == Keys.Alt || key == Keys.Menu ||
                key == Keys.LMenu || key == Keys.RMenu ||
                key == Keys.Shift || key == Keys.ShiftKey ||
                key == Keys.LShiftKey || key == Keys.RShiftKey;
        }

        private void OnAdd(object sender, EventArgs e)
        {
            if (TryAddCurrentAction())
            {
                m_hotkey.Tag = null;
                m_hotkey.Text = string.Empty;
                m_sequence.Clear();
                m_editingIndex = -1;
                UpdateAddButtonText();
                RefreshList();
            }
        }

        private bool TryAddCurrentAction()
        {
            bool hasHotkey = m_hotkey.Tag != null;
            bool hasSequence = !string.IsNullOrWhiteSpace(m_sequence.Text);
            if (!hasHotkey && !hasSequence) return true;

            if (!hasHotkey || !hasSequence)
            {
                MessageBox.Show(this, "Bitte Tastenkombination und Ausdruck angeben.",
                    "Ungültiger Hotkey", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            Keys hotkey = (Keys)m_hotkey.Tag;
            int existingIndex = FindActionIndex(hotkey);
            if (existingIndex >= 0 && existingIndex != m_editingIndex)
            {
                MessageBox.Show(this, "Diese Tastenkombination ist bereits vorhanden.",
                    "Doppelter Hotkey", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            HotkeyAction updatedAction = new HotkeyAction
            {
                Hotkey = hotkey,
                Sequence = m_sequence.Text
            };
            if (m_editingIndex >= 0)
                m_actions[m_editingIndex] = updatedAction;
            else
                m_actions.Add(updatedAction);
            return true;
        }

        private void OnRemove(object sender, EventArgs e)
        {
            if (m_list.SelectedIndices.Count != 1) return;
            m_actions.RemoveAt(m_list.SelectedIndices[0]);
            m_editingIndex = -1;
            m_hotkey.Tag = null;
            m_hotkey.Text = string.Empty;
            m_sequence.Clear();
            UpdateAddButtonText();
            RefreshList();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (m_list.SelectedIndices.Count != 1) return;
            HotkeyAction action = m_actions[m_list.SelectedIndices[0]];
            m_editingIndex = m_list.SelectedIndices[0];
            m_hotkey.Tag = action.Hotkey;
            m_hotkey.Text = action.DisplayHotkey;
            m_sequence.Text = action.Sequence;
            UpdateAddButtonText();
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (!TryAddCurrentAction())
            {
                DialogResult = DialogResult.None;
                return;
            }

            m_hotkey.Tag = null;
            m_hotkey.Text = string.Empty;
            m_sequence.Clear();
            m_editingIndex = -1;
            UpdateAddButtonText();
            RefreshList();

            if (m_actions.Count == 0)
            {
                MessageBox.Show(this, "Mindestens ein globaler Hotkey ist erforderlich.",
                    "Keine Hotkeys", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.None;
            }
        }

        private void RefreshList()
        {
            m_list.Items.Clear();
            foreach (HotkeyAction action in m_actions)
            {
                string status = m_isHotkeyAvailable(action.Hotkey) ?
                    "Verfügbar" : "Warnung: bereits belegt";
                m_list.Items.Add(new ListViewItem(new[]
                {
                    action.DisplayHotkey, action.Sequence, status
                }));
            }
        }

        private int FindActionIndex(Keys hotkey)
        {
            for (int i = 0; i < m_actions.Count; i++)
            {
                if (m_actions[i].Hotkey == hotkey) return i;
            }

            return -1;
        }

        private void UpdateAddButtonText()
        {
            m_addButton.Text = m_editingIndex >= 0 ? "Aktualisieren" : "Hinzufügen";
        }
    }
}
