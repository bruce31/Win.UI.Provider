namespace Win.Ui.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using System.Windows.Forms;

    using DevTools.Application.UI.Interface.Tables;

    public class DisplayTableDialog : BaseInputDialog {

        public static void DisplayTable(ITableInfo options) {

            using (var dlg = new DisplayTableDialog(options)) {
                dlg.ShowDialog();
            }
        }

        public DisplayTableDialog(ITableInfo options) : base(options.TableTitle, MessageBoxButtons.OK) {

            //this.Text = title;
            this.Height = 600;
            var txtBox = new DataGridView() { Dock = DockStyle.Fill };

            txtBox.ReadOnly = true;
            foreach (var col in options.GetColumnInfo()) {
                //txtBox. Columns.Add(new DataGridViewColumn());
            }
            txtBox.AutoGenerateColumns = true;
            txtBox.DataSource = CreateTable(options);// options.TableRows;
            //txtBox.Select(0, 0);

            this.ClientPanel.Controls.Add(txtBox);
            this.InitiallyFocussedControl = txtBox;
        }

        private DataTable CreateTable(ITableInfo options) {
            var tbl = new DataTable();
            foreach (var col in options.GetColumnInfo()) {
                tbl.Columns.Add(new DataColumn(col.Heading, col.DataType)); // txtBox. Columns.Add(new DataGridViewColumn());
            }
            foreach(var row in options.TableRows) {
                tbl.Rows.Add(row.RowData);
            }
            return tbl;
        }
    }
}