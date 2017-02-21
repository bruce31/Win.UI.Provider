namespace Win.Ui.Dialogs {
    using System;
    using System.Drawing;

    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    
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
            return  mGrid.CurrentCell.RowIndex;
        }

        /// <summary>
        /// Sets the current row.
        /// </summary>
        /// <param name="row">Row. zero-based</param>
        public void SetCurrentRow(int row) {
            mCurrChange = Array.IndexOf(mChangeAreaStartLines, row);
            mGrid.CurrentCell = mGrid.Rows[row].Cells[0];
            mGrid.CurrentCell.Selected = false;
        }

        protected void OnLoad(object sender, EventArgs args) {
            mGrid.CurrentCell.Selected = false;
            mGrid.Paint -= OnLoad;
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
            mPrevButton.Enabled = (mCurrChange > 0);
            mNextButton.Enabled = (mCurrChange < mChangeAreaStartLines.Length -1);
        }

        private Button mPrevButton;
        private Button mNextButton;
        private Button mStackedButton;
        private Button mSideBySideButton;

        private DataGridView mGrid;

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

            
            mGrid = new DataGridView() { Dock = DockStyle.Fill};

            mGrid.ReadOnly = true;

            mGrid.AutoGenerateColumns = false;
            mGrid.ColumnCount = 4;

            mGrid.DataSource = diffs.Select(d=>new DiffDisplay(d)).ToArray();
            mGrid.CellFormatting += Table_CellFormatting;
            //grid.AutoSize = true;

            
            mGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            mGrid.Columns[0].Width = 40;
            mGrid.Columns[0].DataPropertyName = "OrigNum";

            mGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            mGrid.Columns[1].DataPropertyName = "OrigLine";
            mGrid.Columns[1].HeaderText = left;

            mGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            mGrid.Columns[2].Width = 40;
            mGrid.Columns[2].DataPropertyName = "CurrNum";

            mGrid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            mGrid.Columns[3].DataPropertyName = "CurrLine";
            mGrid.Columns[3].HeaderText = right;

            mGrid.Paint += OnLoad;

            mChangeAreaStartLines = IdentifyChangeAreas(diffs).ToArray();
            mFirstChange = mChangeAreaStartLines.First();
            mLastChange = mChangeAreaStartLines.Last();

            SetOrientationButtons(sideBySide: true);
            SetPrevNextEnabled();

            this.Controls.Add(mGrid);
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
    }

}
