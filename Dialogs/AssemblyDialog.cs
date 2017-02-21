namespace Win.Ui.Dialogs
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    

    using DevTools.Application.Versioning;
    using DevTools.Common.Extensions;

    public interface IAssemblyDialog {
		void SetReferences (IList<AssemblyVersionInfo> references);
		void Execute ();
	}

	public class AssemblyDialog : Form, IAssemblyDialog
	{
		private const int DialogDefaultHeight = 300;
		private const int DialogDefaultWidth = 400;

		private readonly Form mParent;
		private Panel mTable;

		public AssemblyDialog (Form parent)
		{
			this.mParent = parent;
			this.StartPosition = FormStartPosition.CenterParent;

            mTable = new Panel(); 
            mTable.Dock = DockStyle.Fill;

			var bottomPanel = new Panel { Height = 40, Dock = DockStyle.Bottom };
			var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = false, Dock = DockStyle.Fill };
            
            bottomPanel.Controls.Add(okButton);

			this.Controls.Add(mTable);

			this.Controls.Add(bottomPanel);

			this.Width = DialogDefaultWidth;
			this.Height = DialogDefaultHeight;
		}

		#region IAssemblyDialog implementation

		public void SetReferences (IList<AssemblyVersionInfo> references)
		{
            var top = 0;
			foreach ( var refer in references) {
				AddReference (top, refer);
                top += ReferencePanelHeight;
            }
		}
      		
		private readonly Padding mCellPadding = new Padding(3);
        private const int ReferencePanelHeight = 40;

		private void AddReference(int top, AssemblyVersionInfo verInfo) {
            var rowTable = new TableLayoutPanel {
                LayoutSettings = { ColumnCount = 3, RowCount = 1 }, Height = ReferencePanelHeight, Top = top,
                Width = mTable.ClientSize.Width,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var pLbl = new Label { Text = verInfo.Name, Padding = this.mCellPadding, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
			rowTable.Controls.Add(pLbl, 0, 0);

			var wLbl = new Label { Text = verInfo.Version, Padding = this.mCellPadding, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
			rowTable.Controls.Add(wLbl, 1, 0);

			var btn = new Button {Text = "Details"};
			btn.Click += HandleClicked;
			btn.Tag = verInfo;

            rowTable.Controls.Add(btn, 2, 0);//.SetAlignment (XAlign.Left, YAlign.Middle);

            rowTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 60 });
            rowTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });
            rowTable.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.Percent, Width = 20 });

            mTable.Controls.Add(rowTable);

        }

        private void HandleClicked (object sender, System.EventArgs e)
		{
			var verInfo =(AssemblyVersionInfo) (sender as Button).Tag;
			var dialog = new AboutDialog (this);
			dialog.PopulateFromVersionInfo (verInfo);
			dialog.Execute ();
		}

		public void Execute ()
		{
			this.ShowDialog (mParent);
		}

		#endregion
	}
}

