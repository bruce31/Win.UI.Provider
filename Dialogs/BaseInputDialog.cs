namespace Win.Ui.Dialogs
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    public class BaseInputDialog : Form {
        protected Control InitiallyFocussedControl = null;
        protected Control ClientPanel { get; private set; }
        private Panel mBtnPanel;
         
        public BaseInputDialog(string title, MessageBoxButtons buttons) : base() {
            mBtnPanel = new Panel();
            mBtnPanel.Height = 50;
            mBtnPanel.Dock = DockStyle.Bottom;
            //btnPanel.BackColor = Color.Wheat;
            if (buttons == MessageBoxButtons.OK || buttons == MessageBoxButtons.OKCancel) {
                if (buttons==MessageBoxButtons.OKCancel) {
                    var cancel = new Button();
                    cancel.Text = "Cancel";
                    cancel.DialogResult = DialogResult.Cancel;
                    mBtnPanel.Controls.Add(cancel);
                }
                var ok = new Button();
                ok.Text = "OK";
                ok.DialogResult = DialogResult.OK;
                mBtnPanel.Controls.Add(ok);
            }
            this.Text = title;
            this.Width = 725;
            this.Height = 200;
            this.ClientPanel = new Panel { Dock = DockStyle.Fill };
            // Fill controls need to be added first
            this.Controls.Add(ClientPanel);
            this.Controls.Add(mBtnPanel);
            this.Shown += FormShown;
        }

        private void FormShown(object sender, EventArgs e) {
            if (this.InitiallyFocussedControl != null) {
                InitiallyFocussedControl.Focus();
            }
            AdjustButtons();
        }

        private void AdjustButtons() {
            const int Gap = 30;
            var btnPanel = mBtnPanel;
            var buttons = btnPanel.Controls.Cast<Button>().ToArray();
            var top = (btnPanel.ClientSize.Height - buttons[0].Height) / 2;
            var pos = (btnPanel.ClientSize.Width - Gap * (buttons.Length - 1) - buttons.Sum(b => b.Width)) / 2;
            foreach(var b in buttons) {
                b.Top = top;
                b.Left = pos;
                pos += b.Width + Gap;
            }
        }
    }
}