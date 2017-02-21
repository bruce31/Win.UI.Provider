namespace Win.Ui.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    public class InputComboDialog : BaseInputDialog {

        public static T GetCombo<T>(string prompt, IList<T> items, Func<T, string> display) {

            var dlg = new InputComboDialog("Input", prompt);
            dlg.SetData(items, display);
            if (dlg.ShowDialog() == DialogResult.OK) {
                return items[dlg.Index];
            }
            return default(T);
        }
    
        public InputComboDialog(string title, string prompt) : base(title, MessageBoxButtons.OKCancel) {
            this.Text = title;
            this.Height = 130;
            const int xPad = 40;
            const int yPad = 20;
            var promptLabel = new Label() { Left = xPad, Top = yPad, Text = prompt, AutoSize = true };
            this.Controls.Add(promptLabel);
            var x = 45 + promptLabel.Width;
            var combo = new ComboBox() { Left = x, Top = yPad - 4, Width = this.ClientSize.Width - xPad - x };

            this.ClientPanel.Controls.Add(combo);
            this.InitiallyFocussedControl = combo;
        }

        public void SetData<T>(IList<T> items, Func<T, string> display) {
            var combo = this.ClientPanel.Controls[0] as ComboBox;
            combo.DataSource = items.Select(i => display(i)).ToList();
        }

        public int Index {
            get {
                var combo = this.ClientPanel.Controls[0] as ComboBox;
                return combo.SelectedIndex;
            }
        }
    }
}