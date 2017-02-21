namespace Win.Ui.Dialogs
{
    using System.IO;
    using System.Windows.Forms;

    public class TextFileEditorDialog : BaseInputDialog {

        public static void DisplayFile(string filepath) {
            ShowFile(filepath, false);
        }

        public static void EditFile(string filepath) {
            ShowFile(filepath, true);
        }

        private static void ShowFile(string filepath, bool edit) {
            var file = new FileInfo(filepath);
            var prefix = (edit) ? "Edit" : "View";
            var title = $"{prefix}: {file.FullName}";
            var btns = (edit) ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK;
            using (var dlg = new TextFileEditorDialog(title, file, edit, btns)) {
                if (dlg.ShowDialog() == DialogResult.OK && edit) {
                    dlg.SaveFile();
                }
            }
        }

        private FileInfo mFile;
        private string mOrigText;
        private bool mFileValid = false;

        public void SaveFile() {
            if (mFileValid) {
                var txtBox = this.ClientPanel.Controls[0] as TextBox;
                if (txtBox.Text != mOrigText) {
                    File.WriteAllText(mFile.FullName, txtBox.Text);
                }
            }
        }

        public TextFileEditorDialog(string title, FileInfo file, bool editable, MessageBoxButtons  btns) : base(title, btns) {
            mFile = file;

            if (mFile.Exists) {
                mFileValid = true;
                mOrigText = File.ReadAllText(mFile.FullName);
            } else {
                mOrigText = $"File not found: {mFile.FullName}";
            }
            this.Text = title;
            this.Height = 600;
            var txtBox = new TextBox() { Dock = DockStyle.Fill, Multiline = true };

            txtBox.ReadOnly = !editable;
            txtBox.Text = mOrigText;
            txtBox.Select(0, 0);
            this.ClientPanel.Controls.Add(txtBox);
            this.InitiallyFocussedControl = txtBox;
        }
    }
}