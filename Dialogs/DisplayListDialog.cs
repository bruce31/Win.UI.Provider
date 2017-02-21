namespace Win.Ui.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    public class DisplayListDialog : BaseInputDialog {

        public static void DisplayList(IList<string> inList, string title) {

            using (var dlg = new DisplayListDialog(title, inList)) {
                dlg.ShowDialog();
            }
        }

        public DisplayListDialog(string title, IList<string> inList) : base(title, MessageBoxButtons.OK) {

            this.Text = title;
            this.Height = 600;
            var txtBox = new TextBox() { Dock = DockStyle.Fill, Multiline = true };

            txtBox.ReadOnly = true;
            txtBox.Lines = inList.ToArray();
            txtBox.Select(0, 0);

            this.ClientPanel.Controls.Add(txtBox);
            this.InitiallyFocussedControl = txtBox;
        }
    }
}