namespace Win.Ui.Dialogs {
    using System;
    using System.Drawing;

    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
   // using Gtk;
   // using Extensions;
   // using Widgets;
   using DevCore.Compare;

    public class DisplayDifferenceDialog : BaseInputDialog {
        public static void DisplayDifferences(IList<DiffDisplayLine> diffs, string leftTitle, string rightTitle) {
            DisplayDiffs(diffs, leftTitle, rightTitle);
        }

        public static void DisplayDiffs(IList<DiffDisplayLine> diffs, string leftTitle, string rightTitle) {
            using (var dlg = new DisplayDifferenceDialog(diffs, leftTitle, rightTitle)) {
                dlg.ShowDialog();
            }
        }

        private IList<DiffDisplayLine> mDiffs;
        private string mLeft;
        private string mRight;

        public DisplayDifferenceDialog(IList<DiffDisplayLine> diffs, string leftTitle, string rightTitle) : base("File Differences", MessageBoxButtons.OK) {
            mDiffs = diffs;
            mLeft = leftTitle;
            mRight = rightTitle;

            this.Height = 600;
            var diffView = new DifferenceView(diffs, leftTitle, rightTitle);

            this.ClientPanel.Controls.Add(diffView);
            this.InitiallyFocussedControl = diffView;
        }
    }


    public class DifferenceView : Panel {
        public void SetNextDifferenceRow() {
            var currRow = GetCurrentRow();
            var next = mChangeAreaStartLines.FirstOrDefault(n => n > currRow);
            if (next > currRow) {
                SetCurrentRow(next);
            }
        }

        public void SetPrevDifferenceRow() {
            var currRow = GetCurrentRow();
            var prev = mChangeAreaStartLines.LastOrDefault(n => n < currRow);
            if (prev < currRow && prev >= mFirstChange) {
                SetCurrentRow(prev);
            }
        }

        public int GetCurrentRow() {
            //TreeIter iter;
            //mTreeView.Selection.GetSelected(out iter);
            //var path = mTreeView.Model.GetPath(iter);
            //if (path == null) {
            //    return mCurrChange;
            //}
            return 0;// path.Indices[0];
        }

        /// <summary>
        /// Sets the current row.
        /// </summary>
        /// <param name="row">Row. zero-based</param>
        public void SetCurrentRow(int row) {
            //var path = new TreePath(new[] { row });
            //mTreeView.ScrollToCell(path, mCurrNoCol, true, 0.5f, 0.5f);
            //mTreeView.Selection.UnselectAll();
            mCurrChange = row;
        }

        protected void OnPrev(object sender, EventArgs args) {
            SetPrevDifferenceRow();
            SetPrevNextEnabled();
        }

        protected void OnNext(object sender, EventArgs args) {
            SetNextDifferenceRow();
            SetPrevNextEnabled();
        }

        protected void OnStacked(object sender, EventArgs args) {
            // Do Nothing at present
        }

        protected void OnSideBySide(object sender, EventArgs args) {
            // Do Nothing at present
        }

        private void SetPrevNextEnabled() {
            mPrevButton.Enabled = (mCurrChange != mFirstChange);
            mNextButton.Enabled = (mCurrChange != mLastChange);
        }

        private Button mPrevButton;
        private Button mNextButton;
        private Button mStackedButton;
        private Button mSideBySideButton;

        public DifferenceView(IList<DiffDisplayLine> diffs, string left, string right) : base() {
            var hBox = new Panel() { Dock= DockStyle.Top, Height = 30 };
            var nextLeft = 0;

            mStackedButton = new Button() { Text = "Stacked", Top = 0, Left = nextLeft };
            mStackedButton.Click += OnStacked;
            hBox.Controls.Add(mStackedButton);
            nextLeft += 4 + mStackedButton.Width;

            mSideBySideButton = new Button() { Text = "Side-by-Side", Top = 0, Left = nextLeft };
            mSideBySideButton.Click += OnSideBySide;
            hBox.Controls.Add(mSideBySideButton);
            nextLeft += 40 + mSideBySideButton.Width;

            mPrevButton = new Button() {Text= "<-- Prev", Top = 0, Left = nextLeft };
            mPrevButton.Click += OnPrev;
            hBox.Controls.Add(mPrevButton);
            nextLeft += 4 + mPrevButton.Width;

            mNextButton = new Button() { Text = "Next -->", Top = 0, Left = nextLeft };
            mNextButton.Click += OnNext;
            hBox.Controls.Add(mNextButton);

            
            var grid = new DataGridView() { Dock = DockStyle.Fill};

            grid.ReadOnly = true;

            grid.AutoGenerateColumns = false;
            grid.ColumnCount = 4;

            grid.DataSource = diffs.Select(d=>new DiffDisplay(d)).ToArray();
            grid.CellFormatting += Table_CellFormatting;
            //grid.AutoSize = true;

            
            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns[0].Width = 40;
            grid.Columns[0].DataPropertyName = "OrigNum";

            grid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns[1].DataPropertyName = "OrigLine";
            grid.Columns[1].HeaderText = left;

            grid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            grid.Columns[2].Width = 40;
            grid.Columns[2].DataPropertyName = "CurrNum";

            grid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns[3].DataPropertyName = "CurrLine";
            grid.Columns[3].HeaderText = right;
            //grid.Columns[2].Width = 100;

            mChangeAreaStartLines = IdentifyChangeAreas(diffs).ToArray();
            mFirstChange = mChangeAreaStartLines.First();
            mLastChange = mChangeAreaStartLines.Last();

            SetOrientationButtons(sideBySide: true);

            //this.Controls.Add(mid);
            this.Controls.Add(grid);
            this.Controls.Add(hBox);
            this.Dock = DockStyle.Fill;
        }

        public class DiffDisplay {
            public string OrigNum { get; private set; }
            public string OrigLine { get; private set; }
            public bool OrigDel { get; private set; }
            public string CurrNum { get; private set; }
            public string CurrLine { get; private set; }
            public bool CurrAdd { get; private set; }

            public DiffDisplay(DiffDisplayLine diff) {
                this.OrigNum = (diff.OrigNo == -1) ? "" : diff.OrigNo.ToString("0000");
                this.OrigLine = diff.OrigLine;
                this.OrigDel = diff.OrigDel;
                this.CurrNum = (diff.CurrNo == -1) ? "" : diff.CurrNo.ToString("0000");
                this.CurrLine = diff.CurrLine;
                this.CurrAdd = diff.CurrAdd;
            }
        }

        private void Table_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            var currLine = (DiffDisplay)(sender as DataGridView).Rows[e.RowIndex].DataBoundItem;
            if (currLine.OrigDel && e.ColumnIndex < 2) {
                e.CellStyle.BackColor = DifferenceView.BackgroundColor[(int)DiffDisplayLine.DisplayType.Deleted];
            }
            if (currLine.CurrAdd && e.ColumnIndex > 1) {
                e.CellStyle.BackColor = DifferenceView.BackgroundColor[(int)DiffDisplayLine.DisplayType.Added];
            }
        }

        private void SetOrientationButtons(bool sideBySide) {
            mStackedButton.Enabled = sideBySide;
            mSideBySideButton.Enabled = !sideBySide;
        }

        //private TreeView mTreeView;
        private int[] mChangeAreaStartLines;
        private int mFirstChange;
        private int mLastChange;
        private int mCurrChange = -1;

        private IEnumerable<int> IdentifyChangeAreas(IList<DiffDisplayLine> diffs) {

            var prevIsChange = false;
            for (var no = 0; no < diffs.Count(); no++) {
                if (diffs[no].IsChange != prevIsChange) {
                    if (!prevIsChange) {
                        yield return no;
                    }
                    prevIsChange = !prevIsChange;
                }
            }
        }


        public static readonly Color[] BackgroundColor = {
             Color.FromArgb(255, 255, 255), // unchanged = white
			 Color.FromArgb(255, 150, 150), // deleted = light red
			 Color.FromArgb (100, 255, 100) // added = light green
		};


        //private void RenderCell(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter) {
        //    var render = (cell as Gtk.CellRendererText);

        //    var line = (DiffDisplayLine)model.GetValue(iter, 0);

        //    var txt = "";
        //    var backColor = BackgroundColor[0];

        //    if (column == mChangeCol) {
        //        //txt = ChangeText [(int)line.Type];

        //    } else if (column == mOrigNoCol) {
        //        txt = (line.OrigNo > 0) ? line.OrigNo.ToString("0000") : "";
        //        if (line.OrigDel && line.OrigNo > 0) {
        //            backColor = BackgroundColor[1];
        //        }

        //    } else if (column == mOrigLineCol) {
        //        txt = (line.OrigNo > 0) ? line.OrigLine : "";
        //        if (line.OrigDel && line.OrigNo > 0) {
        //            backColor = BackgroundColor[1];
        //        }

        //    } else if (column == mCurrNoCol) {
        //        txt = (line.CurrNo > 0) ? line.CurrNo.ToString("0000") : "";
        //        if (line.CurrAdd && line.CurrNo > 0) {
        //            backColor = BackgroundColor[2];
        //        }

        //    } else if (column == mCurrLineCol) {
        //        txt = (line.CurrNo > 0) ? line.CurrLine : "";
        //        if (line.CurrAdd && line.CurrNo > 0) {
        //            backColor = BackgroundColor[2];
        //        }
        //    }

        //    render.BackgroundGdk = backColor;
        //    render.Text = txt;
        //}

        //private ListStore GetModel(IList<DiffDisplayLine> diffs) {
        //    var model = new ListStore(typeof(DiffDisplayLine));

        //    foreach (var d in diffs) {
        //        model.AppendValues(d);
        //    }

        //    return model;
        //}
    }

}
