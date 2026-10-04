using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using KeePass.Util.Spr;

namespace Keepass2Hotkeys
{
    internal sealed class AutoTypeExpressionTextBox : TextBox
    {
        private sealed class Completion
        {
            public string Token;
            public string DescriptionKey;

            public override string ToString()
            {
                return Token + "    " + PluginStrings.Get(DescriptionKey);
            }
        }

        private sealed class CompletionPopup : Form
        {
            public readonly ListBox List = new ListBox();

            public CompletionPopup()
            {
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                ShowIcon = false;
                StartPosition = FormStartPosition.Manual;
                MinimizeBox = false;
                MaximizeBox = false;
                Controls.Add(List);
                List.Dock = DockStyle.Fill;
                List.BorderStyle = BorderStyle.FixedSingle;
                List.IntegralHeight = false;
                List.HorizontalScrollbar = true;
            }

            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000;
                    return cp;
                }
            }
        }

        private static readonly Completion[] s_builtInCompletions =
        {
            new Completion { Token = "{USERNAME}", DescriptionKey = "Username" },
            new Completion { Token = "{PASSWORD}", DescriptionKey = "Password" },
            new Completion { Token = "{TIMEOTP}", DescriptionKey = "TimeOtp" },
            new Completion { Token = "{HMACOTP}", DescriptionKey = "HmacOtp" },
            new Completion { Token = "{TITLE}", DescriptionKey = "Title" },
            new Completion { Token = "{URL}", DescriptionKey = "Url" },
            new Completion { Token = "{NOTES}", DescriptionKey = "Notes" },
            new Completion { Token = "{ENTER}", DescriptionKey = "Enter" },
            new Completion { Token = "{TAB}", DescriptionKey = "Tab" },
            new Completion { Token = "{SPACE}", DescriptionKey = "Space" },
            new Completion { Token = "{DELAY 100}", DescriptionKey = "Delay" },
            new Completion { Token = "{CLEARFIELD}", DescriptionKey = "ClearField" },
            new Completion { Token = "{HOME}", DescriptionKey = "Home" },
            new Completion { Token = "{END}", DescriptionKey = "End" }
        };

        private CompletionPopup m_popup;
        private int m_tokenStart;
        private bool m_updating;

        public AutoTypeExpressionTextBox()
        {
            Multiline = false;
            m_popup = new CompletionPopup();
            m_popup.List.Click += OnPopupClick;
            m_popup.Deactivate += delegate { HideCompletion(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_popup != null)
            {
                m_popup.Dispose();
                m_popup = null;
            }

            base.Dispose(disposing);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (!m_updating) ShowCompletion(false);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (m_popup.Visible)
            {
                if (e.KeyCode == Keys.Down)
                {
                    MoveSelection(1);
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode == Keys.Up)
                {
                    MoveSelection(-1);
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
                {
                    InsertSelectedCompletion();
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode == Keys.Escape)
                {
                    HideCompletion();
                    e.SuppressKeyPress = true;
                    return;
                }
            }

            if (e.Control && e.KeyCode == Keys.Space)
            {
                ShowCompletion(true);
                e.SuppressKeyPress = true;
                return;
            }

            base.OnKeyDown(e);
        }

        protected override bool ProcessCmdKey(ref Message message, Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Back &&
                (keyData & Keys.Control) == Keys.Control)
            {
                DeletePreviousWord();
                return true;
            }

            if (m_popup.Visible)
            {
                Keys key = keyData & Keys.KeyCode;
                if (key == Keys.Enter || key == Keys.Tab)
                {
                    InsertSelectedCompletion();
                    return true;
                }

                if (key == Keys.Escape)
                {
                    HideCompletion();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref message, keyData);
        }

        private void DeletePreviousWord()
        {
            if (SelectionLength > 0)
            {
                SelectedText = string.Empty;
                return;
            }

            int end = SelectionStart;
            if (end == 0) return;

            int start = end;
            while (start > 0 && char.IsWhiteSpace(Text[start - 1])) start--;

            if (start > 0 && Text[start - 1] == '}')
            {
                int expressionStart = Text.LastIndexOf('{', start - 1);
                if (expressionStart >= 0)
                {
                    Select(expressionStart, end - expressionStart);
                    SelectedText = string.Empty;
                    return;
                }
            }

            while (start > 0 && !char.IsWhiteSpace(Text[start - 1])) start--;

            Select(start, end - start);
            SelectedText = string.Empty;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (m_popup.Visible)
            {
                Keys key = keyData & Keys.KeyCode;
                if (key == Keys.Enter || key == Keys.Tab)
                {
                    InsertSelectedCompletion();
                    return true;
                }

                if (key == Keys.Escape)
                {
                    HideCompletion();
                    return true;
                }
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (!m_popup.ContainsFocus) HideCompletion();
        }

        private void ShowCompletion(bool showAll)
        {
            if (!Focused || m_popup == null) return;

            string prefix = string.Empty;
            if (!showAll && !TryGetTokenPrefix(out prefix))
            {
                HideCompletion();
                return;
            }

            Completion[] completions = GetCompletions();
            IEnumerable<Completion> matches = completions;
            if (!showAll)
            {
                matches = matches.Where(c => MatchesPrefix(c.Token, prefix));
            }

            Completion[] values = matches.ToArray();
            if (values.Length == 0)
            {
                HideCompletion();
                return;
            }

            m_popup.List.BeginUpdate();
            try
            {
                m_popup.List.Items.Clear();
                foreach (Completion value in values) m_popup.List.Items.Add(value);
                m_popup.List.SelectedIndex = 0;
            }
            finally
            {
                m_popup.List.EndUpdate();
            }

            Point location = PointToScreen(new Point(0, Height));
            int width = Math.Max(Width, 360);
            m_popup.Bounds = new Rectangle(location, new Size(width,
                Math.Min(190, Math.Max(24, values.Length * m_popup.List.ItemHeight + 2))));
            if (!m_popup.Visible) m_popup.Show();
        }

        private bool TryGetTokenPrefix(out string prefix)
        {
            prefix = string.Empty;
            int caret = SelectionStart;
            int openBrace = Text.LastIndexOf('{', Math.Max(0, caret - 1));
            if (openBrace < 0 || Text.IndexOf('}', openBrace, caret - openBrace) >= 0)
                return false;

            m_tokenStart = openBrace;
            prefix = Text.Substring(openBrace, caret - openBrace);
            return true;
        }

        private void MoveSelection(int offset)
        {
            int count = m_popup.List.Items.Count;
            if (count == 0) return;
            int index = (m_popup.List.SelectedIndex + offset + count) % count;
            m_popup.List.SelectedIndex = index;
        }

        private void InsertSelectedCompletion()
        {
            Completion completion = m_popup.List.SelectedItem as Completion;
            if (completion == null) return;

            int length = SelectionStart - m_tokenStart;
            m_updating = true;
            try
            {
                Select(m_tokenStart, length);
                SelectedText = completion.Token;
                SelectionStart = m_tokenStart + completion.Token.Length;
            }
            finally
            {
                m_updating = false;
            }

            HideCompletion();
        }

        private void OnPopupClick(object sender, EventArgs e)
        {
            InsertSelectedCompletion();
            Focus();
        }

        private static Completion[] GetCompletions()
        {
            List<Completion> completions = new List<Completion>();
            HashSet<string> knownTokens = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (Completion completion in s_builtInCompletions)
            {
                completions.Add(completion);
                knownTokens.Add(completion.Token);
            }

            foreach (string placeholder in SprEngine.FilterPlaceholderHints)
            {
                if (string.IsNullOrWhiteSpace(placeholder) ||
                    !knownTokens.Add(placeholder))
                    continue;

                completions.Add(new Completion
                {
                    Token = placeholder,
                    DescriptionKey = "PluginProvided"
                });
            }

            return completions.ToArray();
        }

        private static bool MatchesPrefix(string token, string prefix)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(prefix))
                return false;

            string normalizedToken = token.TrimStart('{');
            string normalizedPrefix = prefix.TrimStart('{');
            return normalizedToken.StartsWith(normalizedPrefix,
                StringComparison.OrdinalIgnoreCase);
        }

        private void HideCompletion()
        {
            if (m_popup != null && m_popup.Visible) m_popup.Hide();
        }
    }
}
