namespace Win.Ui.Dialogs
{
    using System.Windows.Forms;

    public class InputTextDialog : BaseInputDialog {

        public static string Input(string prompt, bool hidden) {
            var dlg = new InputTextDialog("Input", prompt, hidden);

            if (dlg.ShowDialog()== DialogResult.OK) {
                return dlg.InputText;
            }
            return null;
        }

        public InputTextDialog(string title, string prompt, bool hidden) : base(title, MessageBoxButtons.OKCancel) {
            this.Text = title;
            this.Height = 130;
            const int xPad = 40;
            const int yPad = 20;
            var promptLabel = new Label() { Left = xPad, Top = yPad, Text = prompt, AutoSize = true };
            this.Controls.Add(promptLabel);
            var x = 45 + promptLabel.Width;
            var txtBox = new TextBox() { Left = x, Top = yPad - 4, Width = this.ClientSize.Width - xPad - x };
            if (hidden) {
                txtBox.PasswordChar = '*';
            }
            this.ClientPanel.Controls.Add(txtBox);
            this.InitiallyFocussedControl = txtBox;
        }

        public string InputText {
            get {
                var txtBox = this.ClientPanel.Controls[0] as TextBox;
                return txtBox.Text;
            }
        }
    }
}