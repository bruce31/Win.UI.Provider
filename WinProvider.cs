namespace Win.Ui.Provider {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using System.Windows.Forms;

    using DevTools.Application.UI;
    using DevTools.Application.UI.Interface;
    using DevTools.Application.UI.Interface.Tables;

    using DevTools.Common.Extensions;
    using Win.Ui.Dialogs;

    public class WinProvider : IUiProvider {
        //public WinProvider() {
        //}

        public void ApplicationInitialize() {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        public IWindow CreateMainWindow(BaseApplication app) {
            var winWrap = new WindowWrapper(app);
            
            return winWrap;
        }

        public void ApplicationExecute(IWindow window) {
            var winWrap = window as WindowWrapper;
            //winWrap.Win.Load += ShowControlInfo;
            Application.Run(winWrap.Win);
        }

        public static void ShowControlInfo(object sender, EventArgs e) {
            var win = sender as Form;
            var msg = string.Join(Environment.NewLine, GetControlVisibility(win));
            win.DisplayInformationMessage(msg);
        }

        public static IEnumerable<string> GetControlVisibility(Control ctrl) {
            yield return GetControlInfo(ctrl);
            foreach (Control subCtrl in ctrl.Controls) {
                foreach (var l in GetControlVisibility(subCtrl)) {
                    yield return "\t" + l;
                }
            }
        }

        private static string GetControlInfo(Control ctrl) {
            return $"{ctrl.GetType().Name}: Pos: {ctrl.Top}y {ctrl.Left}x  Size: {ctrl.Height}h {ctrl.Width}w  Visible={ctrl.Visible}";
        }

        public IAboutView GetAboutDialog(IWindow window) {
            var win = (window as WindowWrapper).Win;
            return (IAboutView)new AboutDialog(win);
        }
    }


    public class HPanelWrapper : BaseControl, IHPanel {
        private int mNextLeft = 4;
        public HPanelWrapper() : base(new Panel()) {}

        public IControl Add(IControl ctrl, int pos, bool fill) {
            var panel = this.WinControl as Panel;
            var winCtrl = (ctrl as BaseControl).WinControl;
            winCtrl.Left = mNextLeft;
            panel.Controls.Add(winCtrl);
            panel.Height = panel.Controls.Cast<Control>().Max(c => c.Height);
            mNextLeft += winCtrl.Width;
            return ctrl;
        }
    }

    public class LabelWrapper : BaseControl, ILabel {
        public LabelWrapper(string text) : 
            base( new Label() {
                Text = text, AutoSize = true
            }
            ) {}

        public string Text {
            get { return (this.WinControl as Label).Text; }
            set { (this.WinControl as Label).Text = value; }
        }
    }

    public class ButtonWrapper : BaseControl, IButton {
        public ButtonWrapper(string text, EventHandler dele, ButtonClickHandler clickDele) : base(new Button()) {
            var button = this.WinControl as Button;
            button.AutoSize = true;
            button.Text = text;
            button.Click += dele;
            button.Tag = this;
            this.ClickDelegate = clickDele;
        }

        public ButtonClickHandler ClickDelegate { get; private set; }
    }

    public class ComboBoxWrapper<T> : BaseControl, IComboBox<T> {
        private IList<T> mItems;
        public ComboBoxWrapper() : base(new ComboBox()) {
            var combo = (this.WinControl as ComboBox);
            combo.Width = 500;
        }

        public void SetData(IList<T> items, Func<T, string> display) {
            mItems = items;
            var combo = (this.WinControl as ComboBox);
            combo.DataSource = items.Select(i => display(i)).ToList();
        }

        public T SelectedItem {
            get {
                var combo = (this.WinControl as ComboBox);
                return mItems[combo.SelectedIndex];
            }
        }
    }

    public class ListViewerWrapper : BaseControl, IListViewer {
        public ListViewerWrapper() : base(new Panel()) {
            var pnl = this.WinControl as Panel;
            var txtBox = new TextBox();
            txtBox.Multiline = true;
            txtBox.Dock = DockStyle.Fill;
            txtBox.ScrollBars = ScrollBars.Vertical;
            pnl.Controls.Add(txtBox);
        }

        public void SetDataSource(IList<string> items, string title = "", ListViewClickHandler dele = null) {
            var txtBox = (this.WinControl.Controls[0] as TextBox);
            txtBox.Lines = items.ToArray(); //, title); //, dele);
        }
    }

    public abstract class BaseControl : IControl {
        protected BaseControl(Control ctrl) {
            WinControl = ctrl;
        }

        public bool Visible {
            get { return WinControl.Visible; }
            set {
                WinControl.Visible = value;
            }
        }

        public Control WinControl { get; private set; }
    }

    public class WindowWrapper : IWindow {
        public Form Win;

        private BaseApplication mApp;

        private MenuStrip mMainMenu;
        private Panel mMainPanel;

        private const int VSpacing = 2;
        private int mNextTop = VSpacing;

        public WindowWrapper(BaseApplication app) {
            mApp = app;

            Win = new Form();

            Win.Height = 600;
            Win.Width = 800;

            mMainMenu = new MenuStrip() { Dock = DockStyle.Top };
            Win.Controls.Add(mMainMenu);
            mMainPanel = new Panel() {
                Top = mMainMenu.Height, Left = 0,
                Width = Win.ClientSize.Width, Height = Win.ClientSize.Height - mMainMenu.Height,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom }; 
            Win.Controls.Add(mMainPanel);
        }

        #region Window Properties
        public void SetTitle(string title) {
            this.Win.Text = title;
        }

        public void SetSize(int width, int height) {
            Win.Height = height;
            Win.Width = width;
        }
        #endregion

        #region Menus
        public void CreateMenu(IList<IMainMenuItem> menuItems, EventHandler dele) {

            foreach(var menu in menuItems) {
                var item = new ToolStripMenuItem(menu.Name);

                AddSubMenu(dele, menu, item);

                mMainMenu.Items.Add(item);
            }

            Win.MainMenuStrip = mMainMenu;

            mMainMenu.PerformLayout();
            mMainPanel.Visible = true;

            Win.PerformLayout();
        }

        private static void AddSubMenu(EventHandler dele, IMainMenuItem menu, ToolStripMenuItem item) {
            if (menu.SubMenuItems.Count > 0) {
                foreach (var subItem in menu.SubMenuItems) {
                    var subMenuItem = new ToolStripMenuItem(subItem.Name);
                    item.DropDownItems.Add(subMenuItem);
                    if (subItem.ActivateDelegate != null) {
                        subMenuItem.Tag = subItem;
                        subMenuItem.Click += dele;
                    }
                    AddSubMenu(dele, subItem, subMenuItem);
                }
            }
        }

        public void DisableMenuItem(string menuName) {
            SetMenuItemEnabled(menuName, false);
        }

        public void EnableMenuItem(string menuName) {
            SetMenuItemEnabled(menuName, true);
        }

        private void SetMenuItemEnabled(string itemName, bool enabledValue) {
            var item = this.mMainMenu.Items.Cast<ToolStripItem>().FirstOrDefault(i => i.Text.Equals(itemName));
            if (item != null) {
                item.Enabled = enabledValue;
            }
        }

        public ISubMenuItem GetMenuItem(object sender) {
            var menuItem = sender as ToolStripMenuItem;
            return menuItem.Tag as ISubMenuItem;
        }
        #endregion

        public IButton GetButtonItem(object sender) {
            var btnItem = sender as Button;
            return btnItem.Tag as IButton;
        }

        public IButton CreateButton(string text, ButtonClickHandler clickDele) {
            return new ButtonWrapper(text, mApp.OnButtonClick, clickDele);
        }

        public IComboBox<T> CreateComboBox<T>() {
            return new ComboBoxWrapper<T>();
        }

        public void AddAreaControl(IArea areaControl, bool fill) {

            var winControl = (areaControl as BaseControl).WinControl;
            winControl.Top = mNextTop;
            winControl.Width = mMainPanel.ClientSize.Width;
            // Anchor
            winControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            if (fill) {
                var newHeight = mMainPanel.ClientSize.Height - winControl.Top;
                var txtBox = winControl as TextBox;
                if (txtBox != null) {
                    txtBox.MinimumSize = new Size(txtBox.MinimumSize.Width, newHeight);
                } else {
                    winControl.Height = newHeight;
                }
                winControl.Anchor |= AnchorStyles.Bottom;
            }
            mNextTop += winControl.Height + VSpacing;
            mMainPanel.Controls.Add(winControl);
        }

        public IHPanel CreateHPanel() {
            return new HPanelWrapper();
        }

        public IVPanel CreateVPanel() {
            throw new NotImplementedException();
        }

        public ILabel CreateLabel(string text) {
            return new LabelWrapper(text);
        }

        public IListViewer CreateListViewer() {
            return new ListViewerWrapper();
        }

        #region Messages
        public bool DisplayConfirmation(string msg) {
            return Win.DisplayConfirmation(msg);
        }

        public void DisplayInformationMessage(string msg) {
            Win.DisplayInformationMessage(msg);
        }

        public void DisplayErrorMessage(string msg) {
            Win.DisplayErrorMessage(msg);
        }

        public void DisplayExceptionMessage(Exception ex) {
            Win.DisplayExceptionMessage(ex);
        }
        #endregion

        #region Process Wrappers
        public void WrapInErrorHandler(Action act) {
            Win.WrapInErrorHandler(act);
        }

        public bool WrapInProgressWindow(Action<IProgressBar> action, string title) {
            return Win.WrapInProgressWindow(action, title);
        }
        #endregion

        #region Input Dialogs
        public void DisplayFile(string filePath) {
            // temporary
            //WinProvider.ShowControlInfo(Win, null);
            TextFileEditorDialog.DisplayFile(filePath);
        }

        public void EditFile(string filePath) {
            TextFileEditorDialog.EditFile(filePath);
        }

        public string GetInput(string prompt, bool hidden = false) {
            return InputTextDialog.Input(prompt, hidden);
        }

        public T GetCombo<T>(string prompt, IList<T> items, Func<T,string> selector) {
            return InputComboDialog.GetCombo<T>(prompt, items, selector);
        }

        public ITreeInfo GetFromTree(string prompt, ITreeInfo options) {
            throw new NotImplementedException();
        }

        public string SelectFolder(string prompt, string startFolder = "") {
            throw new NotImplementedException();
        }
        #endregion


        #region Dialogs
        public void DisplayList(IList<string> inList, string title) {
            DisplayListDialog.DisplayList(inList, title);
        }

        public void DisplayRtfText(IList<string> inList, IList<IRtfInfo> details, string title) {
            DisplayRtfTextDialog.DisplayText(inList, details, title);
        }

        public void DisplayTable(ITableInfo options) {
            DisplayTableDialog.DisplayTable(options);
        }
        #endregion
    }


    public static class WindowExtensions {
        public static bool DisplayConfirmation(this Form window, string msg) {
            var result = window.DisplayMessage(msg, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            return result == DialogResult.OK;
        }

        public static DialogResult DisplayMessage(this Form window, /*MessageType msgType, */ string msg, string caption, MessageBoxButtons buttons, MessageBoxIcon msgIcon) {
            return MessageBox.Show(window, msg, caption, buttons, msgIcon);
        }

        public static void DisplayInformationMessage(this Form window, string msg) {
            window.DisplayMessage(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void DisplayErrorMessage(this Form window, string msg) {
            window.DisplayMessage(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void DisplayExceptionMessage(this Form window, Exception ex) {
            var stackTrace = ex.StackTrace.Split("\n".ToCharArray())[0];
            var msg = "Error\n{0}\n{1}".FormatWith(ex.Message, stackTrace);
            window.DisplayErrorMessage(msg);
        }


        public static bool WrapInProgressWindow(this Form window, System.Action<IProgressBar> action, string title) {
            var dialog = new ProgressDialog(title);

            try {
                dialog.ExecuteAction(action);
                dialog.ShowDialog();
                if (dialog.ActionException == null) return true;
                window.DisplayExceptionMessage(dialog.ActionException);
                return false;
            } catch (Exception ex) {
                window.DisplayExceptionMessage(ex);
                return false;
            }
        }

        public static bool WrapInErrorHandler(this Form window, System.Action action) {
            try {
                action();
                return true;
            } catch (Exception ex) {
                window.DisplayExceptionMessage(ex);
                return false;
            }
        }
    }

}
