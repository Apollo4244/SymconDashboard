namespace SymconDashboard
{
    internal sealed class PageManagerForm : Form
    {
        public List<PageEntry> ResultPages       { get; private set; } = [];
        public int             ResultActiveIndex { get; private set; }
        public bool            HasAppliedChanges { get; private set; }

        private readonly List<PageEntry> _pages;
        private int  _activeIndex;
        private int  _selectedIndex = -1;
        private bool _updatingList;

        private readonly ListBox _listBox;
        private readonly TextBox _txtName;
        private readonly TextBox _txtUrl;
        private readonly Button  _btnApply;
        private readonly Button  _btnRemove;
        private readonly Button  _btnUp;
        private readonly Button  _btnDown;

        public PageManagerForm(List<PageEntry> pages, int activeIndex)
        {
            _pages       = pages.Select(p => new PageEntry { Name = p.Name, Url = p.Url }).ToList();
            _activeIndex = Math.Clamp(activeIndex, 0, Math.Max(0, pages.Count - 1));

            Text                = Strings.DlgPagesTitle;
            AutoScaleMode       = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(7F, 15F);
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            StartPosition       = FormStartPosition.CenterParent;
            ClientSize          = new Size(760, 440);
            MinimizeBox         = false;
            MaximizeBox         = false;

            // Button dimensions – uniform across all buttons
            const int btnH  = 40;   // height shared by all buttons
            const int btnW  = 130;  // width of Apply / OK / Cancel
            const int crudW = 48;   // width of CRUD buttons (+/−/↑/↓)

            // --- Left column: ListBox ---
            _listBox = new ListBox
            {
                Left = 12, Top = 12, Width = 210, Height = 308,
                IntegralHeight = false
            };
            _listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;

            // CRUD buttons below ListBox
            const int bTop = 328;
            var btnAdd = Btn("+",  12,                   bTop, crudW, btnH);
            _btnRemove = Btn("−",  12 + crudW + 4,       bTop, crudW, btnH);
            _btnUp     = Btn("↑",  12 + crudW * 2 + 12,  bTop, crudW, btnH);
            _btnDown   = Btn("↓",  12 + crudW * 3 + 16,  bTop, crudW, btnH);
            btnAdd.Click     += BtnAdd_Click;
            _btnRemove.Click += BtnRemove_Click;
            _btnUp.Click     += BtnUp_Click;
            _btnDown.Click   += BtnDown_Click;

            // --- Right column: Name + URL ---
            const int rx = 234, rw = 514;
            var lblName = new Label { Text = Strings.DlgPageName, Left = rx, Top = 12,  Width = rw, Height = 20 };
            _txtName    = new TextBox                             { Left = rx, Top = 36,  Width = rw };
            var lblUrl  = new Label { Text = Strings.DlgPageUrl,  Left = rx, Top = 78,  Width = rw, Height = 20 };
            _txtUrl     = new TextBox                             { Left = rx, Top = 102, Width = rw };
            // Separator
            var separator = new Panel
            {
                Left = 12, Top = 378, Width = 736, Height = 1,
                BackColor = SystemColors.ControlDark
            };

            // --- Apply / OK / Cancel ---
            const int btnTop = 390;
            _btnApply = new Button
            {
                Text   = Strings.DlgPageApply,
                Left   = 760 - 12 - btnW - 8 - btnW - 8 - btnW,
                Top    = btnTop,
                Width  = btnW,
                Height = btnH
            };
            _btnApply.Click += BtnApply_Click;
            var btnOk = new Button
            {
                Text         = "OK",
                Left         = 760 - 12 - btnW - 8 - btnW,
                Top          = btnTop,
                Width        = btnW,
                Height       = btnH,
                DialogResult = DialogResult.OK
            };
            var btnCancel = new Button
            {
                Text         = Strings.DlgCancel,
                Left         = 760 - 12 - btnW,
                Top          = btnTop,
                Width        = btnW,
                Height       = btnH,
                DialogResult = DialogResult.Cancel
            };
            btnOk.Click += BtnOk_Click;
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Controls.AddRange([_listBox, btnAdd, _btnRemove, _btnUp, _btnDown,
                               lblName, _txtName, lblUrl, _txtUrl,
                               separator, _btnApply, btnOk, btnCancel]);

            PopulateList();
            if (_listBox.Items.Count > 0)
                _listBox.SelectedIndex = 0;
        }

        private static Button Btn(string text, int x, int y, int w, int h) =>
            new() { Text = text, Left = x, Top = y, Width = w, Height = h };

        // Listeninhalt neu aufbauen; selectIndex = -1 → aktuelle Auswahl beibehalten
        private void PopulateList(int selectIndex = -1)
        {
            _updatingList = true;
            int restore = selectIndex >= 0 ? selectIndex : Math.Max(0, _listBox.SelectedIndex);
            _listBox.Items.Clear();
            for (int i = 0; i < _pages.Count; i++)
                _listBox.Items.Add((i == 0 ? "✦ " : "  ") + _pages[i].Name);
            int newSel = Math.Min(restore, _listBox.Items.Count - 1);
            if (newSel >= 0) _listBox.SelectedIndex = newSel;
            _selectedIndex = newSel;
            _updatingList  = false;
            LoadSelectedEntry();
        }

        // Felder + Button-Zustände für den selektierten Eintrag laden
        private void LoadSelectedEntry()
        {
            bool hasSel       = _selectedIndex >= 0 && _selectedIndex < _pages.Count;
            _txtName.Text     = hasSel ? _pages[_selectedIndex].Name : "";
            _txtUrl.Text      = hasSel ? _pages[_selectedIndex].Url  : "";
            _btnApply.Enabled  = hasSel;
            _btnRemove.Enabled = hasSel && _pages.Count > 1;
            _btnUp.Enabled    = _selectedIndex > 0;
            _btnDown.Enabled  = hasSel && _selectedIndex < _pages.Count - 1;
        }

        private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingList) return;
            _selectedIndex = _listBox.SelectedIndex;
            LoadSelectedEntry();
        }

        // Gibt false zurück wenn Validierung fehlschlägt (Dialog bleibt offen)
        private bool ApplyCurrentEdit()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _pages.Count) return true;
            string name = _txtName.Text.Trim();
            string url  = _txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(Strings.DlgPageNameEmpty, Strings.DlgInvalidInput,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                MessageBox.Show(Strings.DlgUrlInvalid, Strings.DlgError,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            _pages[_selectedIndex].Name = name;
            _pages[_selectedIndex].Url  = url;
            PopulateList(_selectedIndex);
            return true;
        }

        private void BtnApply_Click(object? sender, EventArgs e)
        {
            if (!ApplyCurrentEdit()) return;
            ResultPages       = _pages;
            ResultActiveIndex = Math.Clamp(_activeIndex, 0, Math.Max(0, _pages.Count - 1));
            HasAppliedChanges = true;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            _pages.Add(new PageEntry { Name = Strings.DlgPageNewName, Url = "http://localhost:3777/" });
            PopulateList(_pages.Count - 1);
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            if (_selectedIndex < 0 || _pages.Count <= 1) return;
            int removed = _selectedIndex;
            _pages.RemoveAt(removed);
            if (_activeIndex == removed)     _activeIndex = Math.Min(removed, _pages.Count - 1);
            else if (_activeIndex > removed) _activeIndex--;
            PopulateList(Math.Min(removed, _pages.Count - 1));
        }

        private void BtnUp_Click(object? sender, EventArgs e)
        {
            if (_selectedIndex <= 0) return;
            (_pages[_selectedIndex], _pages[_selectedIndex - 1]) =
                (_pages[_selectedIndex - 1], _pages[_selectedIndex]);
            if      (_activeIndex == _selectedIndex)     _activeIndex--;
            else if (_activeIndex == _selectedIndex - 1) _activeIndex++;
            PopulateList(_selectedIndex - 1);
        }

        private void BtnDown_Click(object? sender, EventArgs e)
        {
            if (_selectedIndex < 0 || _selectedIndex >= _pages.Count - 1) return;
            (_pages[_selectedIndex], _pages[_selectedIndex + 1]) =
                (_pages[_selectedIndex + 1], _pages[_selectedIndex]);
            if      (_activeIndex == _selectedIndex)     _activeIndex++;
            else if (_activeIndex == _selectedIndex + 1) _activeIndex--;
            PopulateList(_selectedIndex + 1);
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (!ApplyCurrentEdit()) { DialogResult = DialogResult.None; return; }
            ResultPages       = _pages;
            ResultActiveIndex = Math.Clamp(_activeIndex, 0, Math.Max(0, _pages.Count - 1));
        }
    }
}
