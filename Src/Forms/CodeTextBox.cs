using AutocompleteMenuNS;
using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;

namespace NFox.Pycad
{
    public class CodeTextBox : Scintilla, ITextBoxWrapper
    {

        private AutocompleteMenu _menu;
        private ToolStripDropDown _tsdd = new ToolStripDropDown();

        public ImageList ImageList
        {
            get { return _menu.ImageList; }
            set { _menu.ImageList = value; }
        }

        public CodeTextBox()
        {

            _menu = new AutocompleteMenu();
            _menu.AppearInterval = 5;
            
            _menu.Items = new string[0];
            _menu.MaximumSize = new Size(360, 200);
            _menu.MinFragmentLength = 1;
            _menu.TargetControlWrapper = this;
            _menu.SetAutocompleteItems(Engine.Keywords);

            _tsdd.Items.Add(new ToolStripControlHost(new Label()));
            _tsdd.Items.Add(new ToolStripControlHost(new Label()));
            _tsdd.ShowItemToolTips = false;
            _tsdd.AutoClose = false;
            _tsdd.GotFocus += _tsdd_GotFocus;

            StyleResetDefault();
            Lexer = Lexer.Python;
            Dock = DockStyle.Fill;
            BorderStyle = BorderStyle.None;
            TabIndex = 0;

            // INITIAL VIEW CONFIG
            WrapMode = WrapMode.None;
            IndentationGuides = IndentView.LookBoth;

            // STYLING
            InitColors();

            InitSyntaxColoring();

            // NUMBER MARGIN
            InitNumberMargin();

            // BOOKMARK MARGIN
            InitBookmarkMargin();

            // CODE FOLDING MARGIN
            InitCodeFolding();


        }

        private void _tsdd_GotFocus(object sender, EventArgs e)
        {
            Focus();
        }

        int _currline;

        protected override void OnUpdateUI(UpdateUIEventArgs e)
        {

            base.OnUpdateUI(e);

            if ((e.Change & (UpdateChange.HScroll | UpdateChange.VScroll)) > 0)
            {
                Scroll?.Invoke(
                    this,
                    new ScrollEventArgs(
                        ScrollEventType.LargeIncrement,
                        0));
            }

            if ((e.Change & UpdateChange.Selection) > 0)
            {
                int start = CurrentPosition;
                if (_tsdd.Visible)
                {
                    if (CurrentLine == _currline && _rangebegin <= start)
                    {
                        var s = Text.Substring(_rangebegin, start - _rangebegin);
                        if (s.Count(c => c == '(') != s.Count(c => c == ')'))
                            _tsdd.Close();
                    }
                    else
                    {
                        _tsdd.Close();
                    }
                }

                if (CurrentLine != _currline || GetDots(ref start) != _totoldots)
                    Engine.Keywords.Properties.Clear();
                _menu.Update();

                if (_currline != CurrentLine)
                {
                    //变量自动完成
                    _currline = CurrentLine;
                    GetVariants();
                }

            }
        }

        int _totoldots;
        int _rangebegin;

        protected override void OnCharAdded(CharAddedEventArgs e)
        {

            base.OnCharAdded(e);

            var curr = CurrentPosition;
            var start = WordStartPosition(curr, true);
            var len = curr - start;
            dynamic obj;

            if (curr > 0)
            {
                var currchar = Text[curr - 1];
                if (currchar == '(')
                {
                    //函数提示
                    _rangebegin = curr;
                    InsertText(curr, ")");
                    obj = GetObject(curr - 1, out _totoldots);
                    if (obj != null && obj.Callable)
                    {
                        _tsdd.Items[0].Text = obj.ToolTipTitle;
                        _tsdd.Items[1].Text = obj.ToolTipText;
                        _tsdd.Show(
                            this,
                            PointXFromPosition(curr) + 2,
                            PointYFromPosition(curr) + FontHeight + 2);
                        Focus();
                    }
                }
                else if (currchar == '.')
                {
                   //属性自动完成
                    obj = GetObject(curr - 1, out _totoldots);
                    if (obj != null)
                        Engine.Keywords.SetProperties(obj.GetItems());
                }

            }


        }

        private int GetFirstPosition(Line line)
        {
            return line.Length - line.Text.TrimStart().Length;
        }

        private void GetVariants()
        {
            Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();
            var line = Lines[CurrentLine];
            int currpos = GetFirstPosition(line);
            for (int i = CurrentLine - 1; i > -1; i--)
            {
                var other = Lines[i];
                int otherpos = GetFirstPosition(other);
                if (otherpos <= currpos)
                {
                    currpos = otherpos;
                    if (Regex.IsMatch(other.Text, "with.*?as.*?"))
                    {

                    }
                    else if (Regex.IsMatch(other.Text, "from.*?import.*?"))
                    {

                    }
                    else if (Regex.IsMatch(other.Text, "import.*?"))
                    {

                    }
                    else if (Regex.IsMatch(other.Text, "def.*?\\(.*?\\)"))
                    {

                    }
                    else if (Regex.IsMatch(other.Text, ".*?=.*?"))
                    {

                    }
                }
            }
        }

        private int GetDots(ref int start)
        {
            int i = 0;
            while (start > 0 && Text[start - 1] == '.')
            {
                start = WordStartPosition(start - 1, true);
                i++;
            }
            return i;
        }

        private dynamic GetObject(int curr, out int dots)
        {
            dots = Text[curr] == '.' ? 1 : 0;
            int start = WordStartPosition(curr - 1, true);
            dots += GetDots(ref start);
            var word = Text.Substring(start, curr - start);
            try { return Engine.Keywords.GetValue(word); }
            catch { return null; }
        }

       
        private void InitColors()
        {
            SetSelectionBackColor(true, IntToColor(0x114D9C));
        }

        private void InitSyntaxColoring()
        {

            // Configure the default style
            XElement xe = 
                XElement.Load(
                    AutoRegAssem.GetFileFullName(
                        Engine.SupportPath, "Config.xml"));
            XElement xef = xe.Element("Font");
            XElement xstyles = xe.Element("Styles");
            XElement xes = xstyles.Element("Style");

            Font = 
                new Font(
                    xef.Attribute("Name").Value, 
                    int.Parse(xef.Attribute("Size").Value));

            var backcolor = GetColor(xes, "BackColor");
            Styles[Style.Default].BackColor = backcolor;
            Styles[Style.Default].ForeColor = CaretForeColor =  GetColor(xes, "ForeColor");

            var highlight = GetColor(xes, "HighLight");
            _menu.Colors =
                new Colors()
                {
                    BackColor = backcolor,
                    ForeColor = CaretForeColor,
                    HighlightingColor = highlight,
                    SelectedBackColor = backcolor,
                    SelectedBackColor2 = GetColor(xes, "BackColor2"),
                    SelectedForeColor = highlight,
                };
            _tsdd.BackColor = backcolor;
            _tsdd.ForeColor = CaretForeColor;

            StyleClearAll();

            Type tsp = typeof(Style.Python);
            foreach (var e in xes.Elements())
            {
                var fsp = tsp.GetField(e.Name.ToString());
                int id = (int)fsp.GetValue(null);
                Styles[id].ForeColor = GetColor(e, "ForeColor");
            }

            SetKeywords(0, string.Join(" ", Engine.Keywords.Statements));
            SetKeywords(1, string.Join(" ", Engine.Keywords.Builtins));

        }

        private Color GetColor(XElement xe, string attname)
        {
            var value = xe.Attribute(attname).Value;
            if (Regex.IsMatch(value, "0x[0-9A-F]*?"))
            {
                return IntToColor(Convert.ToInt32(value, 16));
            }
            else
            {
                Type tc = typeof(Color);
                PropertyInfo fc = tc.GetProperty(value);
                return (Color)fc.GetValue(null);
            }
        }

        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                Styles[Style.Default].Font = Font.Name;
                Styles[Style.Default].Size = (int)Font.Size;
                _menu.Font = Font;
                _tsdd.Items[0].Font = new Font(Font, FontStyle.Bold | FontStyle.Italic);
                _tsdd.Items[1].Font = Font;
            }
        }

        public Control TargetControl
        {
            get { return this; }
        }

        string ITextBoxWrapper.SelectedText
        {
            get { return SelectedText; }
            set
            {
                //Store the start of the selection.
                int start = SelectionStart;

                //Delete the current text between selections.
                DeleteRange(SelectionStart, (SelectionEnd - SelectionStart));

                //Add the text in the same postion.
                InsertText(start, value);

                //Clear selection and make sure the caret is at the end.
                SelectionStart = (start + value.Length);
                SelectionEnd = (start + value.Length);
            }
        }

        public int SelectionLength
        {
            get { return (SelectionEnd - SelectionStart); }
            set { SelectionEnd = (SelectionStart + value); }
        }

        public bool Readonly
        {
            get { return ReadOnly; }
        }

        public Point GetPositionFromCharIndex(int pos)
        {
            int y = PointYFromPosition(pos);
            if (_tsdd.Visible)
                y += _tsdd.Height;
            return new Point(PointXFromPosition(pos), y);
        }

        #region Numbers, Bookmarks, Code Folding

        /// <summary>
        /// the background color of the text area
        /// </summary>
        private const int BACK_COLOR = 0x272727;

        /// <summary>
        /// default text color of the text area
        /// </summary>
        private const int FORE_COLOR = 0x99FFFF;

        /// <summary>
        /// change this to whatever margin you want the line numbers to show in
        /// </summary>
        private const int NUMBER_MARGIN = 1;

        /// <summary>
        /// change this to whatever margin you want the bookmarks/breakpoints to show in
        /// </summary>
        private const int BOOKMARK_MARGIN = 2;
        private const int BOOKMARK_MARKER = 2;

        /// <summary>
        /// change this to whatever margin you want the code folding tree (+/-) to show in
        /// </summary>
        private const int FOLDING_MARGIN = 3;

        /// <summary>
        /// set this true to show circular buttons for code folding (the [+] and [-] buttons on the margin)
        /// </summary>
        private const bool CODEFOLDING_CIRCULAR = true;

        public event ScrollEventHandler Scroll;

        private void InitNumberMargin()
        {

            Styles[Style.LineNumber].BackColor = IntToColor(BACK_COLOR);
            Styles[Style.LineNumber].ForeColor = IntToColor(FORE_COLOR);
            Styles[Style.IndentGuide].ForeColor = IntToColor(FORE_COLOR);
            Styles[Style.IndentGuide].BackColor = IntToColor(BACK_COLOR);

            var nums = Margins[NUMBER_MARGIN];
            nums.Width = 30;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            MarginClick += TextArea_MarginClick;
        }

        private void InitBookmarkMargin()
        {

            //TextArea.SetFoldMarginColor(true, IntToColor(BACK_COLOR));

            var margin = Margins[BOOKMARK_MARGIN];
            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);
            //margin.Cursor = MarginCursor.Arrow;

            var marker = Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor(IntToColor(0xFF003B));
            marker.SetForeColor(IntToColor(0x000000));
            marker.SetAlpha(100);

        }

        private void InitCodeFolding()
        {

            SetFoldMarginColor(true, IntToColor(BACK_COLOR));
            SetFoldMarginHighlightColor(true, IntToColor(BACK_COLOR));

            // Enable code folding
            SetProperty("fold", "1");
            SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
            Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
            Margins[FOLDING_MARGIN].Sensitive = true;
            Margins[FOLDING_MARGIN].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                Markers[i].SetForeColor(IntToColor(BACK_COLOR)); // styles for [+] and [-]
                Markers[i].SetBackColor(IntToColor(FORE_COLOR)); // styles for [+] and [-]
            }

            // Configure folding markers with respective symbols
            Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
            Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
            Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
            Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
            Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

        }

        private void TextArea_MarginClick(object sender, MarginClickEventArgs e)
        {
            if (e.Margin == BOOKMARK_MARGIN)
            {
                // Do we have a marker for this line?
                const uint mask = (1 << BOOKMARK_MARKER);
                var line = Lines[LineFromPosition(e.Position)];
                if ((line.MarkerGet() & mask) > 0)
                {
                    // Remove existing bookmark
                    line.MarkerDelete(BOOKMARK_MARKER);
                }
                else
                {
                    // Add bookmark
                    line.MarkerAdd(BOOKMARK_MARKER);
                }
            }
        }

        #endregion


        #region Indent / Outdent

        private void Indent()
        {
            // we use this hack to send "Shift+Tab" to scintilla, since there is no known API to indent,
            // although the indentation function exists. Pressing TAB with the editor focused confirms this.
            GenerateKeystrokes("{TAB}");
        }

        private void Outdent()
        {
            // we use this hack to send "Shift+Tab" to scintilla, since there is no known API to outdent,
            // although the indentation function exists. Pressing Shift+Tab with the editor focused confirms this.
            GenerateKeystrokes("+{TAB}");
        }

        private void GenerateKeystrokes(string keys)
        {
            HotKeyManager.Enable = false;
            Focus();
            SendKeys.Send(keys);
            HotKeyManager.Enable = true;
        }

        #endregion


        #region Utils

        public static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        public void InvokeIfNeeded(Action action)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        #endregion

    }
}
