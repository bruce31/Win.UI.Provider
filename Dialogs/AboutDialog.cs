namespace Win.Ui.Dialogs
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;

	using System.Windows.Forms;

	using DevTools.Application.Versioning;

    using DevTools.Application.UI.Interface;

	public static class AboutViewExtensions {

		public static void PopulateFromVersionInfo(this IAboutView view, AssemblyVersionInfo verInfo) {

			view.SetProgramName (verInfo.Name);

			view.SetVersion (verInfo.VersionBuild);

			view.SetDescription (verInfo.Description);

			view.SetCopyright (verInfo.Copyright);

			if (verInfo.VersionHistory != null) {
				view.SetVersionHistory (verInfo.VersionHistory);
			}
		} 
	}

	public class AboutDialog : Form, IAboutView
	{
		private const int DialogDefaultHeight = 400;
		private const int DialogDefaultWidth = 500;

        private readonly Form mParent;

        private TableLayoutPanel mTable;
		private Label mProgramName;
		private Label mVersion;
		private Label mCopyright;
		private Label mDescription;
        private readonly Button mReferencedAssembliesButton;
        private readonly CheckBox mSystemAssembliesCheck;

        private IList<AssemblyVersionInfo> mReferences;


		public AboutDialog(Form parent)
		{
			mParent = parent;
			this.StartPosition = FormStartPosition.CenterParent;

			this.Text = "About";

			mTable = new TableLayoutPanel { LayoutSettings = { ColumnCount = 2, RowCount = 6}, Dock = DockStyle.Fill };
			
			var row = 1;
			mProgramName = AddLabelRow (mTable, row++, "Program Name: ");

			mVersion = AddLabelRow (mTable, row++, "Version: ");
			mCopyright = AddLabelRow (mTable, row++, "Copyright: ");
			mDescription = AddLabelRow (mTable, row++, "Description: "); //, YAlign.Top);


			this.Controls.Add(mTable);

			foreach (ColumnStyle styl in mTable.ColumnStyles)
			{
				styl.SizeType = SizeType.AutoSize;
			}

			var bottomPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Bottom, Height = 40 };

			bottomPanel.Controls.Add(mSystemAssembliesCheck = new CheckBox { Text = "Include System Assemblies", AutoSize = true, CheckAlign = ContentAlignment.MiddleRight }); //, Dock = DockStyle.Left, 
			bottomPanel.Controls.Add(mReferencedAssembliesButton = new Button() { Text = "Referenced Assemblies", AutoSize = true, Dock = DockStyle.Fill }); //
			var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true, Dock = DockStyle.Right };
			bottomPanel.Controls.Add(okButton);

			this.Controls.Add(bottomPanel);

			mReferencedAssembliesButton.Click += this.ShowReferencedAssemblies;


			this.Width = DialogDefaultWidth;
			this.Height = DialogDefaultHeight;
		}

		private void ShowReferencedAssemblies(object sender, EventArgs args)
		{
			if (mReferences.Count > 0)
			{
				IAssemblyDialog asmDialog = new AssemblyDialog(this);
				IList<AssemblyVersionInfo> asmsToDisplay = (mSystemAssembliesCheck.Checked) ? mReferences : mReferences.NonSystem().ToList();
				asmDialog.SetReferences(asmsToDisplay);
				asmDialog.Execute();
//				MessageBox.Show(this, "Assembly count = {0}".FormatWith(asmsToDisplay.Count), "Referenced Assemblies");
			}
		}

		private readonly Padding mCellPadding = new Padding(3);

		private Label AddLabelRow (TableLayoutPanel tbl, int row, string prompt) //, float yAlign = YAlign.Middle)
		{
			var pLbl = new Label { Text = prompt, Padding = this.mCellPadding, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
			tbl.Controls.Add(pLbl, 0, row);

			var wLbl = new Label { Text = "", Padding = this.mCellPadding, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
			tbl.Controls.Add(wLbl, 1, row);

			return wLbl;
		}

		private void AddHistoryRow(TableLayoutPanel tbl, int row, string version, string description)
		{
            var pLbl = new Label { Text = version, Padding = this.mCellPadding, Dock = DockStyle.Left, TextAlign = ContentAlignment.TopRight };   // .SetAlignment (XAlign.Right, YAlign.Top);
			tbl.Controls.Add(pLbl, 0, row);

            var wLbl = new Label { Text = description, Padding = this.mCellPadding, Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopLeft };   //.SetAlignment(XAlign.Left, YAlign.Top);
			tbl.Controls.Add(wLbl, 1, row);
		}

		#region IAboutView implementation

		public void SetProgramName (string programName)
		{
			mProgramName.Text = programName;
		}

		public void SetVersion (string version)
		{
			mVersion.Text = version;
		}

		public void SetCopyright (string copyright)
		{
			mCopyright.Text = copyright;
		}

		public void SetDescription (string description)
		{
			mDescription.Text = description;
		}

		public void SetVersionHistory (IList<string> history)
		{
			if (history.Count > 0) {
				int row = mTable.RowCount;
				mTable.RowCount += 1;

				var pLbl = new Label { Text = "Version History", Padding = this.mCellPadding, TextAlign = ContentAlignment.TopRight };   // .SetAlignment (XAlign.Right, YAlign.Top);
				mTable.Controls.Add(pLbl, 0, row);
				var histTbl = new TableLayoutPanel { LayoutSettings = { ColumnCount = 2, RowCount = history.Count }, Dock = DockStyle.Fill };


				var histRow = 0;
				foreach (var line in history) {
					var parts = line.Split ("\t".ToCharArray());
					AddHistoryRow (histTbl, histRow++, parts [0], parts [1]);
				}

				mTable.Controls.Add(histTbl, 1, row);

                foreach (ColumnStyle styl in histTbl.ColumnStyles)
                {
                    styl.SizeType = SizeType.AutoSize;
                }
			}
		}

		public void SetReferences (IList<AssemblyVersionInfo> references)
		{
			mReferences = references;
		}

		public void Execute() {
			var referencesVisible = (mReferences != null && mReferences.Count > 0);
			mReferencedAssembliesButton.Visible = referencesVisible;
			mSystemAssembliesCheck.Visible = referencesVisible;

			this.ShowDialog (mParent);
		}
		#endregion
	}
}

