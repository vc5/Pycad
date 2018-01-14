using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace NFox.Pycad
{
    public class ConsoleTextBox : TextBox
    {

        MenuItem _mnuCut, _mnuCopy, _mnuPaste, _mnuDel;
        int _pos;

        public ConsoleTextBox()
        {

            Multiline = true;
            ScrollBars = ScrollBars.Vertical;

            ContextMenu =
                new ContextMenu(
                    new MenuItem[]
                    {
                        _mnuCut = new MenuItem("剪切(T)", (s, e) => Cut(), Shortcut.CtrlX),
                        _mnuCopy = new MenuItem("复制(C)", (s, e) => Copy(), Shortcut.CtrlC),
                        _mnuPaste = new MenuItem("粘贴(P)", (s, e) => Paste(), Shortcut.CtrlV),
                        _mnuDel = new MenuItem("删除(D)", (s, e) => Delete(), Shortcut.Del)
                    });

            ShortcutsEnabled = false;
            foreach (MenuItem item in ContextMenu.MenuItems)
                item.ShowShortcut = false;

            Write(">>>");

        }


        #region Events

        private int _preStart;
        private int _preLen;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 512 || m.Msg == 256 || m.Msg == 177)
            {
                //选择文本改变时
                if (SelectionStart != _preStart || SelectionLength != _preLen)
                {
                    bool notselectnull = SelectionLength > 0;
                    bool canchange = SelectionStart >= _pos;
                    bool hasnotnewline = !SelectedText.Contains("\n");
                    _mnuCut.Enabled = canchange && notselectnull && hasnotnewline;
                    _mnuCopy.Enabled = notselectnull;
                    _mnuPaste.Enabled = canchange && GetClipboardData() != null;
                    _mnuDel.Enabled = canchange && notselectnull;
                    _preStart = SelectionStart;
                    _preLen = SelectionLength;
                }
            }
        }

        public delegate void CommandHandle(object sender, CommandEventArgs e);
        public event CommandHandle CommandStartting;
        public event CommandHandle CommandEnded;

        public class CommandEventArgs : EventArgs
        {
            public string Command { get; }
            public object Result { get; set; }
            public CommandEventArgs(string cmd)
            {
                Command = cmd;
            }
        }

        private string GetClipboardData()
        {
            IDataObject iData = Clipboard.GetDataObject();
            string copy = null;
            if (iData.GetDataPresent(DataFormats.Text))
                copy = (String)iData.GetData(DataFormats.Text);
            return copy;
        }

        protected override void OnEnter(EventArgs e)
        {
            SelectionStart = Text.Length;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    break;
                default:
                    e.Handled = SelectionStart < _pos;
                    break;
            }
        }


        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            int id = Text.LastIndexOf('\n');
            var key = (Keys)e.KeyChar;

            switch (key)
            {
                case Keys.Back:
                    e.Handled = SelectionStart <= _pos;
                    break;
                case Keys.Enter:
                    e.Handled = true;
                    if (SelectionStart >= _pos && SelectionLength == 0)
                    {
                        Eval(Text.Substring(_pos));
                        Select(Text.Length, 0);
                        ScrollToCaret();
                    }
                    break;
                default:
                    e.Handled = SelectionStart < _pos;
                    break;
            }
        }

        #endregion

        #region Edit

        public new void Cut()
        {
            if (_mnuCut.Enabled)
                base.Cut();
        }

        public new void Copy()
        {
            if (_mnuCopy.Enabled)
                base.Copy();
        }

        public new void Paste()
        {
            if (_mnuPaste.Enabled && !GetClipboardData().Contains("\n"))
                base.Paste();
        }

        public void Delete()
        {
            if (_mnuDel.Enabled)
                SelectedText = "";
        }

        #endregion

        #region Console

        public void Write(string message)
        {
            Text += message;
            _pos = Text.Length;
            Select(Text.Length, 0);
            ScrollToCaret();
        }

        public void WriteNewLine()
        {
            Write(Environment.NewLine);
        }

        List<string> _buffer = new List<string>();
        bool _eof;
        static Regex _regex = new Regex(@"^\s*def (.*?)():\s*$");

        public bool IsRead { get; set; }

        private void Eval(string message)
        {

            if (IsRead)
            {
                ConsoleStream.Buffer = message + Environment.NewLine;
                IsRead = false;
                WriteNewLine();
                return;
            }

            bool newline = true;
            if (_eof)
            {
                if (message == "")
                {
                    _eof = false;
                }
            }
            else if (_regex.IsMatch(message))
            {
                _eof = true;
            }

            if (newline)
                WriteNewLine();

            if (message != "")
                _buffer.Add(message);

            if (!_eof && _buffer.Count > 0)
            {
                var e = new CommandEventArgs(string.Join("\r\n", _buffer));
                _buffer.Clear();
                try
                {
                    CommandStartting?.Invoke(this, e);
                }
                catch (Exception ex)
                {
                    Write(ex.Message);
                }
                finally
                {
                    CommandEnded?.Invoke(this, e);
                }
            }

            if (Text[Text.Length - 1] != '\n')
                WriteNewLine();

            Write(_eof ? "   " : ">>>");

        }

        #endregion


    }
}


