namespace Win.Ui.Dialogs
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using DevTools.Application.UI;

    public class DisplayRtfTextDialog : BaseInputDialog {

        public static void DisplayText(IList<string> inList, IList<IRtfInfo> details, string title) {

            using (var dlg = new DisplayRtfTextDialog(title, inList, details)) {
                dlg.ShowDialog();
            }
        }

        public DisplayRtfTextDialog(string title, IList<string> inList, IList<IRtfInfo> details) : base(title, MessageBoxButtons.OK) {

            this.Text = title;
            this.Height = 600;
            var richTextBox = new RichTextBox() { Dock = DockStyle.Fill, Multiline = true };

            richTextBox.ReadOnly = true;
            richTextBox.Lines = inList.ToArray();
            foreach (var x in details) {
                var start = x.StartColumn;
                if (start == -1) {
                    start = 0;
                }
                //Select the line from it's number
                var startIndex = richTextBox.GetFirstCharIndexFromLine(x.Line);
                richTextBox.Select(startIndex + start, x.EndColumn+1 - start);
                richTextBox.SelectionBackColor = x.Background; 
            }
            richTextBox.Select(0, 0);

            this.ClientPanel.Controls.Add(richTextBox);
            this.InitiallyFocussedControl = richTextBox;
        }
    }
}